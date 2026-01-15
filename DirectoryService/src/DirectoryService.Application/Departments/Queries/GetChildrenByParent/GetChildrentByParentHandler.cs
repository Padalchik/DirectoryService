using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.GetChildrenByParent;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.GetChildrenByParent;

public class GetChildrentByParentHandler : ICommandHandler<GetChildrenByParentResponse, GetChildrentByParentCommand>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetChildrentByParentHandler> _logger;
    private readonly TimeSpan _cacheTtl;

    public GetChildrentByParentHandler(
        IDbConnectionFactory dbConnectionFactory,
        ICacheService cacheService,
        ILogger<GetChildrentByParentHandler> logger,
        IDepartmentsCachePolicy cachePolicy)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _cacheService = cacheService;
        _logger = logger;
        _cacheTtl = cachePolicy.Ttl;
    }

    public async Task<Result<GetChildrenByParentResponse, Errors>> Handle(
        GetChildrentByParentCommand command,
        CancellationToken cancellationToken)
    {
        string cacheKey = BuildCacheKey(command);

        // 1️⃣ Try get from cache
        var response = await _cacheService.GetAsync<GetChildrenByParentResponse>(cacheKey, cancellationToken);

        // 2️⃣ DB query
        if (response is null)
        {
            string sql =
                """
                 WITH roots AS (SELECT * FROM public.departments d WHERE d.id = @DepartmentId ORDER BY created_at)
                 SELECT
                     *,
                     (EXISTS (SELECT 1 FROM public.departments WHERE parent_id = roots.id OFFSET @Offset LIMIT @Limit)) AS has_more_children
                 FROM roots

                 UNION ALL

                 SELECT
                     c.*,
                     (EXISTS (SELECT 1 FROM public.departments WHERE parent_id = c.id)) AS has_more_children
                 FROM roots r CROSS JOIN LATERAL (
                     SELECT * FROM public.departments d WHERE d.parent_id = r.id
                     ORDER BY created_at
                     LIMIT @Limit
                 ) c
                 """;

            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

            var result = await connection.QueryAsync<DepartmentInfoDto>(sql, new
            {
                DepartmentId = command.DepartmentId,
                Offset = (command.Request.Page - 1) * command.Request.Size,
                Limit = command.Request.Size,
            });

            response = new GetChildrenByParentResponse(result.ToList());

            // 3️⃣ Save to cache
            await _cacheService.SetAsync(
                cacheKey,
                response,
                _cacheTtl,
                cancellationToken);

            _logger.LogInformation($"Cache hit: {cacheKey}");
        }

        return Result.Success<GetChildrenByParentResponse, Errors>(response);
    }

    private static string BuildCacheKey(GetChildrentByParentCommand command)
    {
        string mainPart = "departments:children_by_parent";
        string parentPart = $"parent={command.DepartmentId}";
        string pagePart = $"page={command.Request.Page}";
        string sizePart = $"size={command.Request.Size}";

        return $"{mainPart}:{parentPart}:{pagePart}:{sizePart}";
    }
}
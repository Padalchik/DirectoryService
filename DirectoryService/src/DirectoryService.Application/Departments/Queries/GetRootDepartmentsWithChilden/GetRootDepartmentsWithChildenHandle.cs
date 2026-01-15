using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.GetRootDepartmentsWithChilden;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.GetRootDepartmentsWithChilden;

public class GetRootDepartmentsWithChildenHandle : ICommandHandler<GetRootDepartmentsWithChildenResponse, GetRootDepartmentsWithChildenCommand>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetRootDepartmentsWithChildenHandle> _logger;
    private readonly TimeSpan _cacheTtl;

    public GetRootDepartmentsWithChildenHandle(
        IDbConnectionFactory dbConnectionFactory,
        ICacheService cacheService,
        ILogger<GetRootDepartmentsWithChildenHandle> logger,
        IDepartmentsCachePolicy cachePolicy)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _cacheService = cacheService;
        _logger = logger;
        _cacheTtl = cachePolicy.Ttl;
    }

    public async Task<Result<GetRootDepartmentsWithChildenResponse, Errors>> Handle(
        GetRootDepartmentsWithChildenCommand command, CancellationToken cancellationToken)
    {
        string cacheKey = BuildCacheKey(command);

        // 1️⃣ Try get from cache
        var response = await _cacheService.GetAsync<GetRootDepartmentsWithChildenResponse>(cacheKey, cancellationToken);

        if (response is null)
        {
            string sql =
                $"""
                 with roots as (
                     SELECT * FROM public.departments d WHERE d.parent_id is null
                     ORDER by created_at
                     OFFSET @Offset LIMIT @RootLimit
                 )
                 select
                     *,
                     (EXISTS (SELECT 1 FROM public.departments WHERE parent_id = roots.id OFFSET @ChildLimit LIMIT 1)) as has_more_children
                 from roots

                 UNION ALL

                 SELECT 
                     c.*,
                     (EXISTS(select 1 from public.departments WHERE parent_id = c.id)) as has_more_children
                 FROM roots r cross join lateral(
                      SELECT * FROM public.departments d WHERE d.parent_id = r.id
                      ORDER BY created_at
                       LIMIT @ChildLimit
                 ) c
                 """;

            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

            var result = await connection.QueryAsync<DepartmentInfoDto>(sql, new
            {
                Offset = (command.Request.Page - 1) * command.Request.Size,
                RootLimit = command.Request.Size,
                ChildLimit = command.Request.Prefetch,
            });

            response = new GetRootDepartmentsWithChildenResponse(result.ToList());

            // 3️⃣ Save to cache
            await _cacheService.SetAsync(
                cacheKey,
                response,
                _cacheTtl,
                cancellationToken);

            _logger.LogInformation($"Cache hit: {cacheKey}");
        }

        return Result.Success<GetRootDepartmentsWithChildenResponse, Errors>(response);
    }

    private static string BuildCacheKey(GetRootDepartmentsWithChildenCommand command)
    {
        string mainPart = "departments:root_departments_with_childen";
        string pagePart = $"page={command.Request.Page}";
        string sizePart = $"size={command.Request.Size}";
        string prefetchPart = $"prefetch={command.Request.Prefetch}";

        return $"{mainPart}:{pagePart}:{sizePart}:{prefetchPart}";
    }
}
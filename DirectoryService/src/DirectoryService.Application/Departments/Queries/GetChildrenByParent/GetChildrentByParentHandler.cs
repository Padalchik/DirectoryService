using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Shared;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.GetChildrenByParent;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Caching.Hybrid;

namespace DirectoryService.Application.Departments.Queries.GetChildrenByParent;

public class GetChildrentByParentHandler : ICommandHandler<GetChildrenByParentResponse, GetChildrentByParentCommand>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly HybridCache _cache;
    private readonly IDepartmentsCachePolicy _cachePolicy;

    public GetChildrentByParentHandler(
        IDbConnectionFactory dbConnectionFactory,
        IDepartmentsCachePolicy cachePolicy,
        HybridCache cache)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _cachePolicy = cachePolicy;
        _cache = cache;
    }

    public async Task<Result<GetChildrenByParentResponse, Errors>> Handle(
        GetChildrentByParentCommand command,
        CancellationToken cancellationToken)
    {
        string cacheKey = CacheKeyBuilder.Build(
            $"{_cachePolicy.Prefix}:children",
            ("parentId", command.DepartmentId),
            ("page", command.Request.Page),
            ("size", command.Request.Size));

        var response = await _cache.GetOrCreateAsync(
            key: cacheKey,
            factory: async ct => await LoadFromDatabaseAsync(command, ct),
            options: CreateCacheOptions(),
            cancellationToken: cancellationToken);

        return Result.Success<GetChildrenByParentResponse, Errors>(response);
    }

    private async Task<GetChildrenByParentResponse> LoadFromDatabaseAsync(
        GetChildrentByParentCommand command,
        CancellationToken cancellationToken)
    {
        const string sql =
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

        var result = await connection.QueryAsync<DepartmentInfoDto>(
            sql,
            new
            {
                DepartmentId = command.DepartmentId,
                Offset = (command.Request.Page - 1) * command.Request.Size,
                Limit = command.Request.Size,
            });

        return new GetChildrenByParentResponse(result.ToList());
    }

    private HybridCacheEntryOptions CreateCacheOptions()
    {
        return new HybridCacheEntryOptions
        {
            Expiration = _cachePolicy.Ttl,
        };
    }
}
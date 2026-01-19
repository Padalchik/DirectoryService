using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Shared;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.GetRootDepartmentsWithChilden;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.GetRootDepartmentsWithChilden;

public class GetRootDepartmentsWithChildenHandle : ICommandHandler<GetRootDepartmentsWithChildenResponse, GetRootDepartmentsWithChildenCommand>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly HybridCache _cache;
    private readonly IDepartmentsCachePolicy _cachePolicy;

    public GetRootDepartmentsWithChildenHandle(
        IDbConnectionFactory dbConnectionFactory,
        IDepartmentsCachePolicy cachePolicy,
        HybridCache cache)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _cachePolicy = cachePolicy;
        _cache = cache;
    }

    public async Task<Result<GetRootDepartmentsWithChildenResponse, Errors>> Handle(
        GetRootDepartmentsWithChildenCommand command, CancellationToken cancellationToken)
    {
        string cacheKey = CacheKeyBuilder.Build(
            $"{_cachePolicy.Prefix}:root_departments_with_children",
            ("page", command.Request.Page),
            ("size", command.Request.Size),
            ("prefetch", command.Request.Prefetch));

        var response = await _cache.GetOrCreateAsync(
            key: cacheKey,
            factory: async ct => await LoadFromDatabaseAsync(command, ct),
            options: CreateCacheOptions(),
            cancellationToken: cancellationToken);

        return Result.Success<GetRootDepartmentsWithChildenResponse, Errors>(response);
    }

    private async Task<GetRootDepartmentsWithChildenResponse> LoadFromDatabaseAsync(
        GetRootDepartmentsWithChildenCommand command,
        CancellationToken cancellationToken)
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

        return new GetRootDepartmentsWithChildenResponse(result.ToList());
    }

    private HybridCacheEntryOptions CreateCacheOptions()
    {
        return new HybridCacheEntryOptions
        {
            Expiration = _cachePolicy.Ttl,
        };
    }
}
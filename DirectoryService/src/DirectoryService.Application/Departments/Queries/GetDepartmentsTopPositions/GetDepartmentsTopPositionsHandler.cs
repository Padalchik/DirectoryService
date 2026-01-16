using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentsTopPositions;

public class GetDepartmentsTopPositionsHandler : ICommandHandler<GetTopDepartmentsResponse, GetDepartmentsTopPositionsCommand>
{
    private readonly IReadDbConext _readDbConext;
    private readonly HybridCache _cache;
    private readonly ILogger<GetDepartmentsTopPositionsHandler> _logger;
    private readonly IDepartmentsCachePolicy _cachePolicy;

    public GetDepartmentsTopPositionsHandler(
        IReadDbConext readDbConext,
        IDepartmentsCachePolicy cachePolicy,
        ILogger<GetDepartmentsTopPositionsHandler> logger,
        HybridCache cache)
    {
        _readDbConext = readDbConext;
        _cachePolicy = cachePolicy;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<GetTopDepartmentsResponse, Errors>> Handle(
        GetDepartmentsTopPositionsCommand command,
        CancellationToken cancellationToken)
    {
        string cacheKey = BuildCacheKey(command);

        var response = await _cache.GetOrCreateAsync(
            key: cacheKey,
            factory: async ct => await LoadFromDatabaseAsync(command, ct),
            options: CreateCacheOptions(),
            cancellationToken: cancellationToken);

        return Result.Success<GetTopDepartmentsResponse, Errors>(response);
    }

    private async Task<GetTopDepartmentsResponse> LoadFromDatabaseAsync(
        GetDepartmentsTopPositionsCommand command,
        CancellationToken cancellationToken)
    {
        IQueryable<Department> departmentsQuery = _readDbConext.DepartmentsRead
            .Include(d => d.Positions);

        departmentsQuery = departmentsQuery.OrderByDescending(d => d.Positions.Count).ThenBy(d => d.Name.Name);
        departmentsQuery = departmentsQuery.Take(5);

        var departments = await departmentsQuery
            .Select(d => new DepartmentInfoDto
            {
                Id = d.Id,
                Name = d.Name.Name,
                Identifier = d.Identifier.Identifier,
                ParentId = d.ParentId,
                Path = d.Path,
                Depth = d.Depth,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                ChildrenCount = d.ChildrenCount,
            })
            .ToListAsync(cancellationToken);

        return new GetTopDepartmentsResponse(departments);
    }

    private HybridCacheEntryOptions CreateCacheOptions()
    {
        return new HybridCacheEntryOptions
        {
            Expiration = _cachePolicy.Ttl,
        };
    }
    
    private string BuildCacheKey(GetDepartmentsTopPositionsCommand command)
    {
        return $"{_cachePolicy.Prefix}:top_positions";
    }
}
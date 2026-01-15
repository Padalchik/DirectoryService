using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentsTopPositions;

public class GetDepartmentsTopPositionsHandler : ICommandHandler<GetTopDepartmentsResponse, GetDepartmentsTopPositionsCommand>
{
    private readonly IReadDbConext _readDbConext;
    private readonly ICacheService _cacheService;
    private readonly TimeSpan _cacheTtl;
    private readonly ILogger<GetDepartmentsTopPositionsHandler> _logger;

    public GetDepartmentsTopPositionsHandler(
        IReadDbConext readDbConext,
        ICacheService cacheService,
        IDepartmentsCachePolicy cachePolicy,
        ILogger<GetDepartmentsTopPositionsHandler> logger)
    {
        _readDbConext = readDbConext;
        _cacheService = cacheService;
        _logger = logger;
        _cacheTtl = cachePolicy.Ttl;
    }

    public async Task<Result<GetTopDepartmentsResponse, Errors>> Handle(
        GetDepartmentsTopPositionsCommand command,
        CancellationToken cancellationToken)
    {
        string cacheKey = BuildCacheKey(command);

        // 1️⃣ Try get from cache
        var response = await _cacheService.GetAsync<GetTopDepartmentsResponse>(cacheKey, cancellationToken);

        if (response is null)
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

            response = new GetTopDepartmentsResponse(departments);

            // 3️⃣ Save to cache
            await _cacheService.SetAsync(
                cacheKey,
                response,
                _cacheTtl,
                cancellationToken);

            _logger.LogInformation($"Cache hit: {cacheKey}");
        }

        return response;
    }

    private static string BuildCacheKey(GetDepartmentsTopPositionsCommand command)
    {
        return $"departments:top_positions";
    }
}
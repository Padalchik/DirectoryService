using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationsHandler : IQueryHandler<GetLocationsResponse, GetLocationsQuery>
{
    private readonly IReadDbConext _readDbConext;

    public GetLocationsHandler(IReadDbConext readDbConext)
    {
        _readDbConext = readDbConext;
    }

    public async Task<Result<GetLocationsResponse, Errors>> Handle(
        GetLocationsQuery query,
        CancellationToken cancellationToken)
    {
        var locationsQuery = _readDbConext.LocationsRead;

        if (query.Request.DepartmentIds != null)
        {
            locationsQuery = locationsQuery
                .Include(d => d.Departments)
                .Where(l => l.Departments.Any(dl => query.Request.DepartmentIds.Contains(dl.DepartmentId)))
                .AsNoTracking();
        }

        if (!string.IsNullOrEmpty(query.Request.Search))
            locationsQuery = locationsQuery.Where(l => l.Name.Name.ToLower().Contains(query.Request.Search.ToLower()));

        if (query.Request.IsActive != null)
            locationsQuery = locationsQuery.Where(l => l.IsActive == query.Request.IsActive);

        long totalCount = await locationsQuery.LongCountAsync(cancellationToken);

        locationsQuery = locationsQuery
            .OrderBy(l => l.Name.Name)
            .ThenBy(l => l.CreatedAt.ToUniversalTime())
            .Skip((query.Request.Page - 1) * query.Request.PageSize)
            .Take(query.Request.PageSize);

        var locations = await locationsQuery.Select(l => new GetLocationResponse
        {
            Id = l.Id,
            Name = l.Name.Name,
            City = l.Address.City,
            Street = l.Address.Street,
            HouseNumber = l.Address.HouseNumber,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt,
            IsActive = l.IsActive,
            Timezone = l.Timezone.Value,
            Departments = _readDbConext.DepartmentsRead
                .Where(d => l.Departments.Select(ld => ld.DepartmentId).Contains(d.Id))
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
                .AsNoTracking()
                .ToList(),
        })
          .ToListAsync(cancellationToken);

        var getLocationsResponse = new GetLocationsResponse(locations, totalCount);

        return getLocationsResponse;
    }
}
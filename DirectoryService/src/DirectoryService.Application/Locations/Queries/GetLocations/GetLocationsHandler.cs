using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationsHandler : ICommandHandler<GetLocationsResponse, GetLocationsCommand>
{
    private readonly IReadDbConext _readDbConext;

    public GetLocationsHandler(IReadDbConext readDbConext)
    {
        _readDbConext = readDbConext;
    }

    public async Task<Result<GetLocationsResponse, Errors>>? Handle(
        GetLocationsCommand command,
        CancellationToken cancellationToken)
    {
        var locationsQuery = _readDbConext.LocationsRead;

        if (command.Request.DepartmentIds != null)
        {
            locationsQuery = locationsQuery
                .Include(d => d.Departments)
                .Where(l => l.Departments.Any(dl => command.Request.DepartmentIds.Contains(dl.DepartmentId)))
                .AsNoTracking();
        }

        if (!string.IsNullOrEmpty(command.Request.Search))
            locationsQuery = locationsQuery.Where(l => l.Name.Name.ToLower().Contains(command.Request.Search.ToLower()));

        if (command.Request.IsActive != null)
            locationsQuery = locationsQuery.Where(l => l.IsActive == command.Request.IsActive);

        long totalCount = await locationsQuery.LongCountAsync(cancellationToken);

        locationsQuery = locationsQuery
            .OrderBy(l => l.Name.Name)
            .ThenBy(l => l.CreatedAt.ToUniversalTime())
            .Skip((command.Request.Page - 1) * command.Request.PageSize)
            .Take(command.Request.PageSize);

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
                .Include(d => d.Parent)
                .Select(d => new GetDepartmentResponse
                {
                    Id = d.Id,
                    Name = d.Name.Name,
                    Identifier = d.Identifier.Identifier,
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
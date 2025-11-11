using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Locations.Queries.GetLocationById;

public class GetLocationByIdHandler : ICommandHandler<GetLocationResponse, GetLocationByIdCommand>
{
    private readonly IReadDbConext _readDbConext;

    public GetLocationByIdHandler(IReadDbConext readDbConext)
    {
        _readDbConext = readDbConext;
    }

    public async Task<Result<GetLocationResponse, Errors>> Handle(GetLocationByIdCommand command, CancellationToken cancellationToken)
    {
        var locationDto = await _readDbConext.LocationsRead
            .Include(l => l.Departments)
            .AsNoTracking()
            .Where(l => l.Id == command.LocationId)
            .Select(l => new GetLocationResponse
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
            .FirstOrDefaultAsync(cancellationToken);

        if (locationDto == null)
            return GeneralErrors.NotFound(command.LocationId).ToErrors();

        return locationDto;
    }
}
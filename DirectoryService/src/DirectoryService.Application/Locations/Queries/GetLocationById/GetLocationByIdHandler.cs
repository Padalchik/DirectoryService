using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Locations.Queries.GetLocationById;

public class GetLocationByIdHandler : ICommandHandler<GetLocationDto, GetLocationByIdCommand>
{
    private readonly IReadDbConext _readDbConext;

    public GetLocationByIdHandler(IReadDbConext readDbConext)
    {
        _readDbConext = readDbConext;
    }

    public async Task<Result<GetLocationDto, Errors>> Handle(GetLocationByIdCommand command, CancellationToken cancellationToken)
    {
        var location = await _readDbConext.LocationsRead.Include(l => l.Departments).AsNoTracking().FirstOrDefaultAsync(l => l.Id == command.LocationId, cancellationToken);

        if (location == null)
            return GeneralErrors.NotFound(command.LocationId).ToErrors();

        var departments = _readDbConext.DepartmentsRead.Where(d => location.Departments.Select(ld => ld.DepartmentId).Contains(d.Id)).Include(d => d.Parent).AsNoTracking();
        var departmentsDto = new List<GetDepartmentDto>();

        foreach (var department in departments)
        {
            departmentsDto.Add(CreateDepartmentDto(department));
        }

        var locationDto = new GetLocationDto()
        {
            Id = location.Id,
            Name = location.Name.Name,
            City = location.Address.City,
            Street = location.Address.Street,
            HouseNumber = location.Address.HouseNumber,
            CreatedAt = location.CreatedAt,
            UpdatedAt = location.UpdatedAt,
            IsActive = location.IsActive,
            Timezone = location.Timezone.Value,
            Departments = departmentsDto,
        };

        return locationDto;
    }

    private GetDepartmentDto CreateDepartmentDto(Department department)
    {
        return new GetDepartmentDto
        {
            Id = department.Id,
            Name = department.Name.Name,
            Identifier = department.Identifier.Identifier,
            Parent = department.Parent != null ? CreateDepartmentDto(department.Parent) : null,
            Path = department.Path,
            Depth = department.Depth,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt,
            UpdatedAt = department.UpdatedAt,
            ChildrenCount = department.ChildrenCount,
        };
    }
}
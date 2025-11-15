using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Application.Database;

public interface IReadDbConext
{
    IQueryable<Location> LocationsRead { get; }

    IQueryable<Department> DepartmentsRead { get; }

    IQueryable<Position> PositionsRead { get; }
}
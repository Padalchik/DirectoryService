using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.UpdateLocations;

public record UpdateLocationsCommand(Guid DepartmentId, UpdateLocationsDto UpdateLocationsDto) : ICommand;
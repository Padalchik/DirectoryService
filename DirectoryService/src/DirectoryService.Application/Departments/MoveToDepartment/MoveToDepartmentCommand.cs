using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.MoveToDepartment;

public record MoveToDepartmentCommand(Guid DepartmentId, MoveToDepartmentDto MoveToDepartmentDto) : ICommand;
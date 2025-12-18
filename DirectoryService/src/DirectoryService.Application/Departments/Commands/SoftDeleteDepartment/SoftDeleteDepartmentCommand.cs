using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Commands.SoftDeleteDepartment;

public record SoftDeleteDepartmentCommand(Guid DepartmentId) : ICommand;
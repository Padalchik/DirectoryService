using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Commands.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentDto CreateDepartmentDto) : ICommand;
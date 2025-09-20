using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments;

public record CreateDepartmentCommand(CreateDepartmentDto CreateDepartmentDto) : ICommand;
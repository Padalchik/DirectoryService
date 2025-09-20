namespace DirectoryService.Contracts.Departments;

public record CreateDepartmentDto(string Name, string Identifier, Guid? ParentId, IEnumerable<Guid> LocationIds);
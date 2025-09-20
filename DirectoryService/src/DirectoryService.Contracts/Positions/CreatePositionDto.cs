namespace DirectoryService.Contracts.Positions;

public record CreatePositionDto(string Name, string? Description, IEnumerable<Guid> DepartmentIds);
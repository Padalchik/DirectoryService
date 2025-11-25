using DirectoryService.Contracts.Locations;

namespace DirectoryService.Contracts.Departments;

public record DepartmentInfoDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public Guid? ParentId { get; init; }
    public string Path { get; init; } = string.Empty;
    public short Depth { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int ChildrenCount { get; init; }
    public bool HasMoreChildren { get; init; }
}
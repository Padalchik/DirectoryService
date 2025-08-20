using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department;

namespace DirectoryService.Domain.Position;

public class Position
{
    public Guid Id { get; private set; }

    public PositionName Name { get; private set; }

    public PositionDescription Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public Position(Guid id, PositionName name, PositionDescription description)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // EF C0RE
    private Position()
    {
    }
}
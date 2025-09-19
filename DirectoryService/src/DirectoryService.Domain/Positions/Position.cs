using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Positions;

public class Position
{
    private HashSet<DepartmentPosition> _departments = [];

    public Guid Id { get; private set; }

    public PositionName Name { get; private set; }

    public PositionDescription Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlySet<DepartmentPosition> Departments => _departments;

    public Position(Guid? id, PositionName name, PositionDescription description)
    {
        Id = id ?? Guid.NewGuid();
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

    public UnitResult<Error> UpdatePositions(IEnumerable<DepartmentPosition> newPositions)
    {
        if (newPositions.Count() == 0)
            return Error.Validation("position.department", "Department positions must contain at least one position");

        _departments = newPositions.ToHashSet();
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }
}
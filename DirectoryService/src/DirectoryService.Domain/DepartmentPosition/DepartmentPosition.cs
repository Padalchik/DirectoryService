using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.DepartmentPosition;

public class DepartmentPosition
{
    public Guid Id { get; }

    public Guid DepartmentId { get; }

    public Guid PositionId { get; }

    public DateTime CreatedAt { get; private set; }

    public DepartmentPosition(Guid positionId, Guid departmentId)
    {
        Id = Guid.NewGuid();
        PositionId = positionId;
        DepartmentId = departmentId;
        CreatedAt = DateTime.UtcNow;
    }

    // EF C0RE
    private DepartmentPosition()
    {
    }
}
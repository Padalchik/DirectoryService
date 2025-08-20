using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.DepartmentPosition;

public class DepartmentPosition
{
    public Guid Id { get; }

    public Guid DepartmentId { get; }

    public Guid PositionId { get; }

    public DateTime CreatedAt { get; private set; }

    private DepartmentPosition(Guid positionId, Guid departmentId)
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

    public static Result<DepartmentPosition> Create(Guid idPosition, Guid departmentId)
    {
        var departmentPosition = new DepartmentPosition(idPosition, departmentId);
        return Result.Success(departmentPosition);
    }
}
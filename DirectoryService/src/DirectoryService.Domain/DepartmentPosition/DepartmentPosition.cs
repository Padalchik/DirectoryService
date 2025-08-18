using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.DepartmentPosition;

public class DepartmentPosition
{
    public Guid IdPosition { get; }

    private DepartmentPosition(Guid idPosition)
    {
        IdPosition = idPosition;
    }

    public static Result<DepartmentPosition> Create(Guid idPosition)
    {
        var departmentPosition = new DepartmentPosition(idPosition);
        return Result.Success(departmentPosition);
    }
}
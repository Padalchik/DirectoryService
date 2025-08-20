using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.DepartmentLocation;

public class DepartmentLocation
{
    public Guid Id { get; }

    public Guid DepartmentId { get; }

    public Guid LocationId { get; }

    public DateTime CreatedAt { get; private set; }

    private DepartmentLocation(Guid locationId, Guid departmentId)
    {
        Id = Guid.NewGuid();
        DepartmentId = departmentId;
        LocationId = locationId;
        CreatedAt = DateTime.UtcNow;
    }

    // EF C0RE
    private DepartmentLocation()
    {
    }

    public static Result<DepartmentLocation> Create(Guid idLocation, Guid idDepartment)
    {
        var departmentLocation = new DepartmentLocation(idLocation, idDepartment);
        return Result.Success(departmentLocation);
    }
}
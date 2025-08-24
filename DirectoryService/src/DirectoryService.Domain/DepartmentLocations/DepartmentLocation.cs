namespace DirectoryService.Domain.DepartmentLocations;

public class DepartmentLocation
{
    public Guid Id { get; }

    public Guid DepartmentId { get; }

    public Guid LocationId { get; }

    public DateTime CreatedAt { get; private set; }

    public DepartmentLocation(Guid locationId, Guid departmentId)
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
}
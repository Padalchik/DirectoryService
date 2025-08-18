using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.DepartmentLocation;

public class DepartmentLocation
{
    public Guid IdLocation { get; }

    private DepartmentLocation(Guid idLocation)
    {
        IdLocation = idLocation;
    }

    public static Result<DepartmentLocation> Create(Guid idLocation)
    {
        var departmentLocation = new DepartmentLocation(idLocation);
        return Result.Success(departmentLocation);
    }
}
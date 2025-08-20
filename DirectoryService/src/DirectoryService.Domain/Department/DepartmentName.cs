using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department;

namespace DirectoryService.Domain;

public record DepartmentName
{
    public string Name { get; init; }

    private DepartmentName(string name)
    {
        Name = name;
    }
    
    public static Result<DepartmentName> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<DepartmentName>("Department name cannot be null or empty");
        
        if (name.Length < Constants.MIN_DEPARTMENT_NAME_LENGTH)
            return Result.Failure<DepartmentName>("Department name is too short");
        
        if (name.Length > Constants.MAX_DEPARTMENT_NAME_LENGTH)
            return Result.Failure<DepartmentName>("Department name is too long");

        var departmentName = new DepartmentName(name);
        return Result.Success(departmentName);
    }
}
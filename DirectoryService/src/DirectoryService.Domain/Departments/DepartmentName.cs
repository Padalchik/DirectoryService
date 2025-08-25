using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    public string Name { get; init; }

    private DepartmentName(string name)
    {
        Name = name;
    }

    public static Result<DepartmentName, Error> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return GeneralErrors.ValueIsRequired("name");

        if (name.Length < Constants.MIN_DEPARTMENT_NAME_LENGTH)
            return GeneralErrors.IncorrectValueLength("name");

        if (name.Length > Constants.MAX_DEPARTMENT_NAME_LENGTH)
            return GeneralErrors.IncorrectValueLength("name");

        var departmentName = new DepartmentName(name);
        return Result.Success<DepartmentName, Error>(departmentName);
    }
}
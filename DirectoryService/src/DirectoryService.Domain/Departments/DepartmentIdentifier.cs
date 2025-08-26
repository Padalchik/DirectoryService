using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public record DepartmentIdentifier
{
    public string Identifier { get; init; }

    private DepartmentIdentifier(string identifier)
    {
        Identifier = identifier;
    }

    public static Result<DepartmentIdentifier, Error> Create(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return GeneralErrors.ValueIsRequired("identifier");

        if (identifier.Length < Constants.MIN_DEPARTMENT_NAME_LENGTH)
            return GeneralErrors.IncorrectValueLength("identifier");

        if (identifier.Length > Constants.MAX_DEPARTMENT_NAME_LENGTH)
            return GeneralErrors.IncorrectValueLength("identifier");

        if (!Regex.IsMatch(identifier, @"^[a-zA-Z]+$"))
            return Error.Validation("invalid.regex.format", $"Некорректный формат identifier");

        var departmentsName = new DepartmentIdentifier(identifier);
        return Result.Success<DepartmentIdentifier, Error>(departmentsName);
    }
}
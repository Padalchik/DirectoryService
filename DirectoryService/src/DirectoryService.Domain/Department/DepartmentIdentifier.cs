using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain;

public record DepartmentIdentifier
{
    public string Identifier { get; init; }

    private DepartmentIdentifier(string identifier)
    {
        Identifier = identifier;
    }
    
    public static Result<DepartmentIdentifier> Create(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return Result.Failure<DepartmentIdentifier>("Department identifier cannot be null or empty");
        
        if (identifier.Length < Constants.MIN_DEPARTMENT_NAME_LENGTH)
            return Result.Failure<DepartmentIdentifier>("Department identifier is too short");
        
        if (identifier.Length > Constants.MAX_DEPARTMENT_NAME_LENGTH)
            return Result.Failure<DepartmentIdentifier>("Department identifier is too long");

        if (!Regex.IsMatch(identifier, @"^[a-zA-Z]+$"))
            return Result.Failure<DepartmentIdentifier>("Department identifier must contain only Latin characters");
        
        var departmentsName = new DepartmentIdentifier(identifier);
        return Result.Success(departmentsName);
    }
}
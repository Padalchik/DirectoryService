using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Position;

public record PositionName
{
    public string Name { get; init; }

    private PositionName(string name)
    {
        Name = name;
    }
    
    public static Result<PositionName> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<PositionName>("Position name cannot be null or empty");
        
        if (name.Length < Constants.MIN_POSITION_NAME_LENGTH)
            return Result.Failure<PositionName>("Position name is too short");
        
        if (name.Length > Constants.MAX_POSITION_NAME_LENGTH)
            return Result.Failure<PositionName>("Position name is too long");

        var locationName = new PositionName(name);
        return Result.Success(locationName);
    }
}
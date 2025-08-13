using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Location;

public record LocationName
{
    public string Name { get; init; }

    private LocationName(string name)
    {
        Name = name;
    }
    
    public static Result<LocationName> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<LocationName>("Location name cannot be null or empty");
        
        if (name.Length < Constants.MIN_LOCATION_NAME_LENGTH)
            return Result.Failure<LocationName>("Location name is too short");
        
        if (name.Length > Constants.MAX_LOCATION_NAME_LENGTH)
            return Result.Failure<LocationName>("Location name is too long");

        var locationName = new LocationName(name);
        return Result.Success(locationName);
    }
}
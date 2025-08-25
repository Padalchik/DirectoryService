using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Locations;

public record LocationName
{
    public string Name { get; init; }

    private LocationName(string name)
    {
        Name = name;
    }

    public static Result<LocationName, Error> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return GeneralErrors.ValueIsRequired("name");

        if (name.Length < Constants.MIN_LOCATION_NAME_LENGTH)
            return GeneralErrors.IncorrectValueLength("name");

        if (name.Length > Constants.MAX_LOCATION_NAME_LENGTH)
            return GeneralErrors.IncorrectValueLength("name");

        var locationName = new LocationName(name);
        return Result.Success<LocationName, Error>(locationName);
    }
}
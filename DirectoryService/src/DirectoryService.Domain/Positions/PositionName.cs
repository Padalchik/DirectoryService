using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Positions;

public record PositionName
{
    public string Name { get; init; }

    private PositionName(string name)
    {
        Name = name;
    }

    public static Result<PositionName, Error> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return GeneralErrors.ValueIsRequired("name");

        if (name.Length < Constants.MIN_POSITION_NAME_LENGTH)
            return GeneralErrors.IncorrectValueLength("name");

        if (name.Length > Constants.MAX_POSITION_NAME_LENGTH)
            return GeneralErrors.IncorrectValueLength("name");

        var locationName = new PositionName(name);
        return Result.Success<PositionName, Error>(locationName);
    }
}
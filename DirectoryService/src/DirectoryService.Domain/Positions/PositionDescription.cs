using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Positions;

public record PositionDescription
{
    public string Value { get; init; }

    private PositionDescription(string? value)
    {
        Value = value ?? string.Empty;
    }

    public static Result<PositionDescription, Error> Create(string? value)
    {
        if (value != null)
        {
            if (value.Length > Constants.MAX_POSITION_DESCRIPTION_LENGTH)
                return GeneralErrors.IncorrectValueLength("position description");
        }

        var locationName = new PositionDescription(value);
        return Result.Success<PositionDescription, Error>(locationName);
    }
}
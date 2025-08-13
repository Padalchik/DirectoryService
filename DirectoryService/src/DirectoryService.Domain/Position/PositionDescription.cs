using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Position;

public record PositionDescription
{
    public string Value { get; init; }

    private PositionDescription(string value)
    {
        Value = value;
    }
    
    public static Result<PositionDescription> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<PositionDescription>("Position description cannot be null or empty");

        if (value.Length > Constants.MAX_POSITION_DESCRIPTION_LENGTH)
            return Result.Failure<PositionDescription>("Position description is too long");

        var locationName = new PositionDescription(value);
        return Result.Success(locationName);
    }
}
using CSharpFunctionalExtensions;
using TimeZoneConverter;

namespace DirectoryService.Domain.Locations;

public record Timezone
{
    public string Value { get; init; }

    private Timezone(string value)
    {
        Value = value;
    }
    
    public static Result<Timezone> Create(string value)
    {
        if (!TZConvert.TryGetTimeZoneInfo(value, out var _))
            return Result.Failure<Timezone>("Invalid timezone value");

        var timezone = new Timezone(value);
        return Result.Success(timezone);
    }
}
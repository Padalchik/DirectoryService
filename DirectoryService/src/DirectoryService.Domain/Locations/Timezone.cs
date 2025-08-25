using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using TimeZoneConverter;

namespace DirectoryService.Domain.Locations;

public record Timezone
{
    public string Value { get; init; }

    private Timezone(string value)
    {
        Value = value;
    }

    public static Result<Timezone, Error> Create(string value)
    {
        if (!TZConvert.TryGetTimeZoneInfo(value, out var _))
            return GeneralErrors.ValueIsInvalid("timezone");

        var timezone = new Timezone(value);
        return Result.Success<Timezone, Error>(timezone);
    }
}
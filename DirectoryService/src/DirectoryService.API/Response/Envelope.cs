using System.Text.Json.Serialization;
using DirectoryService.Domain.Shared;

namespace DirectoryService.API.Response;

public record Envelope
{
    public object? Result { get; }

    public Errors? ErrorList { get; }

    public DateTime TimeGenerated { get; }

    [JsonConstructor]
    private Envelope(object? result, Errors? errorList)
    {
        Result = result;
        ErrorList = errorList;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope Ok(object? result = null) =>
        new(result, null);

    public static Envelope Error(Errors errors) =>
        new(null, errors);
}

using System.Text;

namespace DirectoryService.Application.Shared;

public static class CacheKeyBuilder
{
    public static string Build(
        string prefix,
        params (string Name, object Value)[] parameters)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            throw new ArgumentException("Prefix cannot be null or empty", nameof(prefix));

        var key = new StringBuilder(prefix);

        foreach ((string name, object value) in parameters)
            key.Append($":{name}={value}");

        return key.ToString();
    }
}
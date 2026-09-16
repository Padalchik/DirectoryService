namespace DirectoryService.Consumer;

public sealed class RabbitMqOptions
{
    public const string SECTION_NAME = "RabbitMq";

    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 5672;

    public string UserName { get; init; } = "directory";

    public string Password { get; init; } = "directory";

    public string VirtualHost { get; init; } = "/";
}

using DirectoryService.Consumer;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SECTION_NAME))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Host), "RabbitMq:Host is required")
    .Validate(options => options.Port is > 0 and <= 65535, "RabbitMq:Port is invalid")
    .Validate(options => !string.IsNullOrWhiteSpace(options.UserName), "RabbitMq:UserName is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "RabbitMq:Password is required")
    .ValidateOnStart();

builder.Services.AddHostedService<DepartmentCreatedConsumer>();

await builder.Build().RunAsync();

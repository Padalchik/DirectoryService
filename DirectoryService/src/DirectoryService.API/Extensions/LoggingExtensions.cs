using Serilog;

namespace DirectoryService.API.Extensions;

public static class LoggingExtensions
{
    public static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Debug()
            .WriteTo.Seq(
                configuration.GetConnectionString("Seq") ?? throw new Exception("Seq connection string not found"))
            .CreateLogger();

        services.AddSerilog();

        return services;
    }
}
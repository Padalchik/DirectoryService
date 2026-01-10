using DirectoryService.API.Middlewares;
using Serilog;

namespace DirectoryService.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApi(this IApplicationBuilder app)
    {
        app.UseCustomExeptionMiddleware();
        app.UseSerilogRequestLogging();

        return app;
    }
}
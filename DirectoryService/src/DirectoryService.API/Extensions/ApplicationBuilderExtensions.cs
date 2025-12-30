using DirectoryService.API.Middlewares;
using DirectoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DirectoryService.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApplication(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging();
        app.UseCustomExeptionMiddleware();

        return app;
    }

    public static IApplicationBuilder UseDatabaseMigration(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        // если БД пустая — просто мигрируем
        if (!db.Database.GetAppliedMigrations().Any())
        {
            db.Database.Migrate();
        }

        return app;
    }
}
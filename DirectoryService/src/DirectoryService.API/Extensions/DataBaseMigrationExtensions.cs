using DirectoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.API.Extensions;

public static class DataBaseMigrationExtensions
{
    public static void MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        db.Database.Migrate();
    }
}
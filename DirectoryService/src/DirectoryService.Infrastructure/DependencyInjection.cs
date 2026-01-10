using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ILocationsRepository, LocationRepository>();
        services.AddScoped<IPositionsRepository, PositionRepository>();
        services.AddScoped<IDepartmentsRepository, DepartmentRepository>();

        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddScoped<IDbConnectionFactory, NpgSlqConnectionFactory>();

        services.AddHostedService<InactiveDepartmentsCleanerBackgroundService>();

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        return services;
    }

    public static IServiceCollection ManageApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ApplicationDBContext>(_ =>
            new ApplicationDBContext(configuration.GetConnectionString("DataBase")!));

        services.AddScoped<IReadDbConext>(_ =>
            new ApplicationDBContext(configuration.GetConnectionString("DataBase")!));

        return services;
    }
}
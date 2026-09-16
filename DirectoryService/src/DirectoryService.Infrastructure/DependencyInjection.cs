using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.Infrastructure.Cache;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Messaging;
using DirectoryService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
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

    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            string connection = configuration.GetConnectionString("Redis") ?? throw new ArgumentNullException(nameof(configuration));

            options.Configuration = connection;
        });

        services.AddHybridCache();

        return services;
    }

    public static IServiceCollection AddDepartmentsCacheOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DepartmentsCacheOptions>(
            configuration.GetSection("Cache:Departments"));

        services.AddSingleton<IDepartmentsCachePolicy, DepartmentsCachePolicy>();

        return services;
    }

    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SECTION_NAME))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Host), "RabbitMq:Host is required")
            .Validate(options => options.Port is > 0 and <= 65535, "RabbitMq:Port is invalid")
            .Validate(options => !string.IsNullOrWhiteSpace(options.UserName), "RabbitMq:UserName is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "RabbitMq:Password is required")
            .ValidateOnStart();

        services.AddSingleton<IDepartmentCreatedIntegrationEventPublisher,
            RabbitMqDepartmentCreatedIntegrationEventPublisher>();

        return services;
    }
}

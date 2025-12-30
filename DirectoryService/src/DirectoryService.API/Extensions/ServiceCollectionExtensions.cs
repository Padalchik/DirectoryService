using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DirectoryService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddOpenApi();

        // ---> ВАЖНО: здесь теперь настраивается DbContext
        services.AddDbContext<ApplicationDBContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DataBase")));

        // Read-контекст из того же DbContext
        services.AddScoped<IReadDbConext, ApplicationDBContext>();

        var applicationAssembly = typeof(CreateLocationHandler).Assembly;

        services.Scan(scan => scan
            .FromAssemblies(applicationAssembly)
            .AddClasses(classes => classes.AssignableToAny(typeof(ICommandHandler<,>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        services.AddValidatorsFromAssembly(applicationAssembly);

        services.AddScoped<ILocationsRepository, LocationRepository>();
        services.AddScoped<IPositionsRepository, PositionRepository>();
        services.AddScoped<IDepartmentsRepository, DepartmentRepository>();

        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddScoped<IDbConnectionFactory, NpgSlqConnectionFactory>();

        return services;
    }

    public static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Debug()
            .WriteTo.Seq(configuration.GetConnectionString("Seq")!)
            .CreateLogger();

        services.AddSerilog();
        return services;
    }
}

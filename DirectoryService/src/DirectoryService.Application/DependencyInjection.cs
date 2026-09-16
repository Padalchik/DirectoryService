using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Commands.CreateLocation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(CreateLocationHandler).Assembly;

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(c => c.AssignableToAny(
                typeof(ICommandHandler<,>),
                typeof(IQueryHandler<,>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}

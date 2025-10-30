using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.MoveToDepartment;
using DirectoryService.Application.Departments.UpdateLocations;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public class TestDataSeeder : DirectoryServiceBaseTests
{
    public TestDataSeeder(DirectoryServiceTestWebFactory factory)
        : base(factory)
    {
    }

    public async Task<Guid> CreateLocationAsync(
        string name = "Головной офис",
        string city = "Москва",
        string street = "ул. Мира",
        string houseNumber = "9Ак4",
        string timezone = "Europe/Moscow")
    {
        return await ExecuteDb(async db =>
        {
            var location = new Location(
                LocationName.Create(name).Value,
                Address.Create(city, street, houseNumber).Value,
                Timezone.Create(timezone).Value);

            db.Locations.Add(location);
            await db.SaveChangesAsync();

            return location.Id;
        });
    }

    public async Task<Result<Domain.Departments.Department, Errors>> CreateDepartmentAsync(
        string name,
        string identifier,
        Guid? parent,
        IEnumerable<Guid>? locationIds,
        CancellationToken cancellationToken)
    {
        var result = await ExecuteCreateDepartmentHandler(async sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentDto(name, identifier, parent, locationIds ?? new List<Guid>()));

            return await sut.Handle(command, cancellationToken);
        });

        return result;
    }

    public async Task<Result<Domain.Departments.Department, Errors>> UpdateLocationsAsync(
        Guid departmentId,
        IEnumerable<Guid> locationIds,
        CancellationToken cancellationToken)
    {
        var result = await ExecuteUpdateLocationsHandler(sut =>
        {
            var command = new UpdateLocationsCommand(departmentId, new UpdateLocationsDto(locationIds));
            return sut.Handle(command, cancellationToken);
        });

        return result;
    }

    public async Task<Result<Domain.Departments.Department, Errors>> MoveToDepartmentAsync(
        Guid departmentId,
        Guid? parentId,
        CancellationToken cancellationToken)
    {
        var result = await ExecuteMoveToDepartmentHandler(sut =>
        {
            var command = new MoveToDepartmentCommand(departmentId, new MoveToDepartmentDto(parentId));
            return sut.Handle(command, cancellationToken);
        });

        return result;
    }

    private async Task<T> ExecuteCreateDepartmentHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();
        return await action(sut);
    }

    private async Task<T> ExecuteMoveToDepartmentHandler<T>(Func<MoveToDepartmentHandler, Task<T>> action)
    {
        var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<MoveToDepartmentHandler>();
        return await action(sut);
    }

    private async Task<T> ExecuteUpdateLocationsHandler<T>(Func<UpdateLocationsHandler, Task<T>> action)
    {
        var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<UpdateLocationsHandler>();

        return await action(sut);
    }
}

using DirectoryService.Infrastructure;
using DirectoryService.IntegrationTests.Seeders;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public class DirectoryServiceBaseTests : IClassFixture<DirectoryServiceTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;

    protected DepartmentSeeder DepartmentSeeder { get; }

    protected LocationSeeder LocationSeeder { get; }

    protected IServiceProvider Services { get; }

    protected DirectoryServiceBaseTests(DirectoryServiceTestWebFactory factory)
    {
        _resetDatabase = factory.ResetDatabaseAsync;

        Services = factory.Services;
        var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        DepartmentSeeder = new DepartmentSeeder(dbContext);
        LocationSeeder = new LocationSeeder(dbContext);
    }

    protected async Task<T> ExecuteDb<T>(Func<ApplicationDBContext, Task<T>> action)
    {
        var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        return await action(dbContext);
    }

    protected async Task ExecuteDb(Func<ApplicationDBContext, Task> action)
    {
        var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        await action(dbContext);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }
}
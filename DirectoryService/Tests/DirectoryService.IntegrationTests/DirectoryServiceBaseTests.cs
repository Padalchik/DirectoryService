using DirectoryService.API;
using DirectoryService.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public class DirectoryServiceBaseTests : IClassFixture<DirectoryServiceTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;

    protected IServiceProvider Services { get; }

    protected DirectoryServiceBaseTests(DirectoryServiceTestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
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
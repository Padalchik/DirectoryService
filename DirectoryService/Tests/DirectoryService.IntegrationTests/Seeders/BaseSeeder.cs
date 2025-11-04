using DirectoryService.Infrastructure;

namespace DirectoryService.IntegrationTests.Seeders;

public abstract class BaseSeeder
{
    private readonly ApplicationDBContext _dbContext;

    protected BaseSeeder(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected async Task<T> ExecuteDb<T>(Func<ApplicationDBContext, Task<T>> action)
    {
        return await action(_dbContext);
    }

    protected async Task ExecuteDb(Func<ApplicationDBContext, Task> action)
    {
        await action(_dbContext);
    }
}
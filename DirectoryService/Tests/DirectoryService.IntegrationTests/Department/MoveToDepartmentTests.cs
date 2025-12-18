using DirectoryService.Application.Departments.Commands.MoveToDepartment;
using DirectoryService.Contracts.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Department;

public class MoveToDepartmentTests : DirectoryServiceBaseTests
{
    public MoveToDepartmentTests(DirectoryServiceTestWebFactory factory)
        : base(factory)
    {
    }

    // Позитивные сценарии
    [Fact]
    public async Task CreateDepartment_ValidDataMoveToDepartment_ShouldSucceed()
    {
        var cancellationToken = CancellationToken.None;
        var locationId = await LocationSeeder.CreateLocationAsync();

        const string oldBuhDepartmentPath = "BUH";
        var adminDepartmentId = (await DepartmentSeeder.CreateDepartmentAsync("Администрация", "ADMIN", null, [locationId], cancellationToken)).Id;
        var buhDepartmentId = (await DepartmentSeeder.CreateDepartmentAsync("Бухгалтерия", oldBuhDepartmentPath, null, [locationId], cancellationToken)).Id;

        var result = await ExecuteHandler(sut =>
        {
            var command = new MoveToDepartmentCommand(buhDepartmentId, new MoveToDepartmentDto(adminDepartmentId));
            return sut.Handle(command, cancellationToken);
        });

        await ExecuteDb(async dbContext =>
        {
            var adminDepartment = await dbContext.Departments.FirstOrDefaultAsync(d => d.Id == adminDepartmentId, cancellationToken);
            var buhDepartment = await dbContext.Departments.Include(department => department.Parent).FirstOrDefaultAsync(d => d.Id == buhDepartmentId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.True(adminDepartment.Children.Contains(buhDepartment));
            Assert.Equal(1, buhDepartment.Depth);
            Assert.Equal(buhDepartment.Path, $"{adminDepartment.Path}.{oldBuhDepartmentPath}".ToLower());
            Assert.Equal(buhDepartment.Parent, adminDepartment);
        });
    }

    [Fact]
    public async Task CreateDepartment_ValidDataMoveToRoot_ShouldSucceed()
    {
        var cancellationToken = CancellationToken.None;
        var locationId = await LocationSeeder.CreateLocationAsync();

        const string oldBuhDepartmentPath = "BUH";
        var adminDepartmentId = (await DepartmentSeeder.CreateDepartmentAsync("Администрация", "ADMIN", null, [locationId], cancellationToken)).Id;
        var buhDepartmentId = (await DepartmentSeeder.CreateDepartmentAsync("Бухгалтерия", oldBuhDepartmentPath, adminDepartmentId, [locationId], cancellationToken)).Id;

        var result = await ExecuteHandler(sut =>
        {
            var command = new MoveToDepartmentCommand(buhDepartmentId, new MoveToDepartmentDto(null));
            return sut.Handle(command, cancellationToken);
        });

        await ExecuteDb(async dbContext =>
        {
            var adminDepartment = await dbContext.Departments.FirstOrDefaultAsync(d => d.Id == adminDepartmentId, cancellationToken);
            var buhDepartment = await dbContext.Departments.Include(department => department.Parent).FirstOrDefaultAsync(d => d.Id == buhDepartmentId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.False(adminDepartment.Children.Contains(buhDepartment));
            Assert.Equal(0, buhDepartment.Depth);
            Assert.Equal(oldBuhDepartmentPath.ToLower(), buhDepartment.Path);
            Assert.Null(buhDepartment.Parent);
        });
    }

    // Негативные сценарии
    [Fact]
    public async Task CreateDepartment_InValidDataFakeDepartment_ShouldFail()
    {
        var cancellationToken = CancellationToken.None;
        var locationId = await LocationSeeder.CreateLocationAsync();
        var parentId = (await DepartmentSeeder.CreateDepartmentAsync("Администрация", "ADMIN", null, [locationId], cancellationToken)).Id;

        var result = await ExecuteHandler(sut =>
        {
            var command = new MoveToDepartmentCommand(Guid.Empty, new MoveToDepartmentDto(parentId));
            return sut.Handle(command, cancellationToken);
        });

        await ExecuteDb(dbContext =>
        {
            Assert.True(result.IsFailure);
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task CreateDepartment_InValidDataFakeParent_ShouldFail()
    {
        var cancellationToken = CancellationToken.None;
        var locationId = await LocationSeeder.CreateLocationAsync();
        var departmentId = (await DepartmentSeeder.CreateDepartmentAsync("Администрация", "ADMIN", null, [locationId], cancellationToken)).Id;

        var result = await ExecuteHandler(sut =>
        {
            var command = new MoveToDepartmentCommand(departmentId, new MoveToDepartmentDto(Guid.Empty));
            return sut.Handle(command, cancellationToken);
        });

        await ExecuteDb(dbContext =>
        {
            Assert.True(result.IsFailure);
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task CreateDepartment_InValidDataDepartmentAndParentIsEqual_ShouldFail()
    {
        var cancellationToken = CancellationToken.None;
        var locationId = await LocationSeeder.CreateLocationAsync();
        var departmentId = (await DepartmentSeeder.CreateDepartmentAsync("Администрация", "ADMIN", null, [locationId], cancellationToken)).Id;

        var result = await ExecuteHandler(sut =>
        {
            var command = new MoveToDepartmentCommand(departmentId, new MoveToDepartmentDto(departmentId));
            return sut.Handle(command, cancellationToken);
        });

        await ExecuteDb(dbContext =>
        {
            Assert.True(result.IsFailure);
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task CreateDepartment_InValidDataParentIsChildOfDepartment_ShouldFail()
    {
        var cancellationToken = CancellationToken.None;
        var locationId = await LocationSeeder.CreateLocationAsync();
        var parentId = (await DepartmentSeeder.CreateDepartmentAsync("Администрация", "ADMIN", null, [locationId], cancellationToken)).Id;
        var departmentId = (await DepartmentSeeder.CreateDepartmentAsync("Бухгалтерия", "BUH", parentId, [locationId], cancellationToken)).Id;

        var result = await ExecuteHandler(sut =>
        {
            var command = new MoveToDepartmentCommand(parentId, new MoveToDepartmentDto(departmentId));
            return sut.Handle(command, cancellationToken);
        });

        await ExecuteDb(dbContext =>
        {
            Assert.True(result.IsFailure);
            return Task.CompletedTask;
        });
    }

    private async Task<T> ExecuteHandler<T>(Func<MoveToDepartmentHandler, Task<T>> action)
    {
        var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<MoveToDepartmentHandler>();
        return await action(sut);
    }
}
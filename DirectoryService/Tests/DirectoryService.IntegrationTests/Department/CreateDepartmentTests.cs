using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Department;

public class CreateDepartmentTests : DirectoryServiceBaseTests
{
    public CreateDepartmentTests(DirectoryServiceTestWebFactory factory)
        : base(factory)
    {

    }

    // Позитивные сценарии
    [Fact]
    public async Task CreateDepartment_ValidDataWithoutParent_ShouldSucceed()
    {
        var cancellationToken = CancellationToken.None;
        var locationId = await LocationSeeder.CreateLocationAsync();

        var result = await ExecuteHandler(async sut =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentDto("Бухгалтерия", "BUH", null, [locationId]));
            return await sut.Handle(command, cancellationToken);
        });

        await ExecuteDb(async dbContext =>
        {
            var department = await dbContext.Departments.FirstAsync(d => d.Id == result.Value.Id, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(result.Value.Id, department.Id);
            Assert.NotEqual(Guid.Empty, result.Value.Id);
        });
    }

    [Fact]
    public async Task CreateDepartment_ValidDataWithParent_ShouldSucceed()
    {
        var cancellationToken = CancellationToken.None;
        var locationId = await LocationSeeder.CreateLocationAsync();

        var parent = (await ExecuteHandler(async sut =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentDto("Администрация", "ADMIN", null, [locationId]));
            return await sut.Handle(command, cancellationToken);
        })).Value;

        var result = await ExecuteHandler(async sut =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentDto("Бухгалтерия", "BUH", parent.Id, [locationId]));
            return await sut.Handle(command, cancellationToken);
        });

        await ExecuteDb(async dbContext =>
        {
            var department = await dbContext.Departments.FirstAsync(d => d.Id == result.Value.Id, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(result.Value.Id, department.Id);
            Assert.NotEqual(Guid.Empty, result.Value.Id);
        });
    }

    [Fact]
    public async Task CreateDepartment_ValidDataWithManyLocations_ShouldSucceed()
    {
        var cancellationToken = CancellationToken.None;
        var oneLocationId = await LocationSeeder.CreateLocationAsync();
        var twoLocationId = await LocationSeeder.CreateLocationAsync("Коворкинг", "Санкт-Петербург", "Невский проспект", "1");

        var result = await ExecuteHandler(async sut =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentDto("Бухгалтерия", "BUH", null, [oneLocationId, twoLocationId]));
            return await sut.Handle(command, cancellationToken);
        });

        await ExecuteDb(async dbContext =>
        {
            var department = await dbContext.Departments.FirstAsync(d => d.Id == result.Value.Id, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(result.Value.Id, department.Id);
            Assert.NotEqual(Guid.Empty, result.Value.Id);
        });
    }

    // Негативные сценарии
    [Fact]
    public async Task CreateDepartment_InValidDataWithoutLocation_ShouldFail()
    {
        var cancellationToken = CancellationToken.None;

        var result = await ExecuteHandler(async sut =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentDto("Бухгалтерия", "BUH", null, []));
            return await sut.Handle(command, cancellationToken);
        });

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public async Task CreateDepartment_InValidDataWithFakeLocation_ShouldFail()
    {
        var cancellationToken = CancellationToken.None;

        var result = await ExecuteHandler(async sut =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentDto("Бухгалтерия", "BUH", null, [Guid.Empty]));
            return await sut.Handle(command, cancellationToken);
        });

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public async Task CreateDepartment_InValidDataWithEmptyDepartmentName_ShouldFail()
    {
        var locationId = await LocationSeeder.CreateLocationAsync();
        var cancellationToken = CancellationToken.None;

        var result = await ExecuteHandler(async sut =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentDto(string.Empty, "BUH", null, [locationId]));
            return await sut.Handle(command, cancellationToken);
        });

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public async Task CreateDepartment_InValidDataWithFakeParent_ShouldFail()
    {
        var cancellationToken = CancellationToken.None;
        var locationId = await LocationSeeder.CreateLocationAsync();

        var result = await ExecuteHandler(async sut =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentDto(string.Empty, "BUH", Guid.Empty, [locationId]));
            return await sut.Handle(command, cancellationToken);
        });

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }

    private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();
        return await action(sut);
    }
}
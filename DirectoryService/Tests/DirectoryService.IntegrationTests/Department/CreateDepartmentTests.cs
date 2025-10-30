using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Department;

public class CreateDepartmentTests : DirectoryServiceBaseTests
{
    private readonly TestDataSeeder _seeder;

    public CreateDepartmentTests(DirectoryServiceTestWebFactory factory)
        : base(factory)
    {
        _seeder = new TestDataSeeder(factory);
    }

    // Позитивные сценарии
    [Fact]
    public async Task CreateDepartment_ValidDataWithoutParent_ShouldSucceed()
    {
        var cancellationToken = CancellationToken.None;
        var locationId = await _seeder.CreateLocationAsync();
        var result = await _seeder.CreateDepartmentAsync("Бухгалтерия", "BUH", null, [locationId], cancellationToken);

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
        var locationId = await _seeder.CreateLocationAsync();
        Domain.Departments.Department parent = (await _seeder.CreateDepartmentAsync("Администрация", "ADMIN", null, [locationId], cancellationToken)).Value;
        var result = await _seeder.CreateDepartmentAsync("Бухгалтерия", "BUH", parent.Id, [locationId], cancellationToken);

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
        var oneLocationId = await _seeder.CreateLocationAsync();
        var twoLocationId = await _seeder.CreateLocationAsync("Коворкинг", "Санкт-Петербург", "Невский проспект", "1");
        var result = await _seeder.CreateDepartmentAsync("Бухгалтерия", "BUH", null, [oneLocationId, twoLocationId], cancellationToken);

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
        var result = await _seeder.CreateDepartmentAsync("Бухгалтерия", "BUH", null, [], cancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public async Task CreateDepartment_InValidDataWithFakeLocation_ShouldFail()
    {
        var cancellationToken = CancellationToken.None;
        var result = await _seeder.CreateDepartmentAsync("Бухгалтерия", "BUH", null, [Guid.Empty], cancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public async Task CreateDepartment_InValidDataWithEmptyDepartmentName_ShouldFail()
    {
        var locationId = await _seeder.CreateLocationAsync();
        var cancellationToken = CancellationToken.None;
        var result = await _seeder.CreateDepartmentAsync(string.Empty, "BUH", null, [locationId], cancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public async Task CreateDepartment_InValidDataWithFakeParent_ShouldFail()
    {
        var cancellationToken = CancellationToken.None;
        var locationId = await _seeder.CreateLocationAsync();
        var result = await _seeder.CreateDepartmentAsync("Бухгалтерия", "BUH", Guid.Empty, [locationId], cancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
}
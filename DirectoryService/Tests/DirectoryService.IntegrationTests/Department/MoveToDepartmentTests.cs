using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Department;

public class MoveToDepartmentTests : DirectoryServiceBaseTests
{
    private readonly TestDataSeeder _seeder;

    public MoveToDepartmentTests(DirectoryServiceTestWebFactory factory)
        : base(factory)
    {
        _seeder = new TestDataSeeder(factory);
    }

    // Позитивные сценарии
    [Fact]
    public async Task CreateDepartment_ValidDataMoveToDepartment_ShouldSucceed()
    {
        var cancellationToken = CancellationToken.None;
        var locationId = await _seeder.CreateLocationAsync();

        const string oldBuhDepartmentPath = "BUH";
        var adminDepartmentId = (await _seeder.CreateDepartmentAsync("Администрация", "ADMIN", null, [locationId], cancellationToken)).Value.Id;
        var buhDepartmentId = (await _seeder.CreateDepartmentAsync("Бухгалтерия", oldBuhDepartmentPath, null, [locationId], cancellationToken)).Value.Id;

        var result = await _seeder.MoveToDepartmentAsync(buhDepartmentId, adminDepartmentId, cancellationToken);

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
        var locationId = await _seeder.CreateLocationAsync();

        const string oldBuhDepartmentPath = "BUH";
        var adminDepartmentId = (await _seeder.CreateDepartmentAsync("Администрация", "ADMIN", null, [locationId], cancellationToken)).Value.Id;
        var buhDepartmentId = (await _seeder.CreateDepartmentAsync("Бухгалтерия", oldBuhDepartmentPath, adminDepartmentId, [locationId], cancellationToken)).Value.Id;

        var result = await _seeder.MoveToDepartmentAsync(buhDepartmentId, null, cancellationToken);

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
        var locationId = await _seeder.CreateLocationAsync();
        var parentId = (await _seeder.CreateDepartmentAsync("Администрация", "ADMIN", null, [locationId], cancellationToken)).Value.Id;

        var result = await _seeder.MoveToDepartmentAsync(Guid.Empty, parentId, cancellationToken);

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
        var locationId = await _seeder.CreateLocationAsync();
        var departmentId = (await _seeder.CreateDepartmentAsync("Администрация", "ADMIN", null, [locationId], cancellationToken)).Value.Id;

        var result = await _seeder.MoveToDepartmentAsync(departmentId, Guid.Empty, cancellationToken);

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
        var locationId = await _seeder.CreateLocationAsync();
        var departmentId = (await _seeder.CreateDepartmentAsync("Администрация", "ADMIN", null, [locationId], cancellationToken)).Value.Id;

        var result = await _seeder.MoveToDepartmentAsync(departmentId, departmentId, cancellationToken);

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
        var locationId = await _seeder.CreateLocationAsync();
        var parentId = (await _seeder.CreateDepartmentAsync("Администрация", "ADMIN", null, [locationId], cancellationToken)).Value.Id;
        var departmentId = (await _seeder.CreateDepartmentAsync("Бухгалтерия", "BUH", parentId, [locationId], cancellationToken)).Value.Id;

        var result = await _seeder.MoveToDepartmentAsync(parentId, departmentId, cancellationToken);

        await ExecuteDb(dbContext =>
        {
            Assert.True(result.IsFailure);
            return Task.CompletedTask;
        });
    }
}
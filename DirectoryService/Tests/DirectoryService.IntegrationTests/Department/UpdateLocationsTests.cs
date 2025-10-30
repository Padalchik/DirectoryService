using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.UpdateLocations;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Department;

public class UpdateLocationsTests : DirectoryServiceBaseTests
{
    private readonly TestDataSeeder _seeder;

    public UpdateLocationsTests(DirectoryServiceTestWebFactory factory)
        : base(factory)
    {
        _seeder = new TestDataSeeder(factory);
    }

    // Позитивные сценарии
    [Fact]
    public async Task UpdateLocations_ValidDataOneLocation_ShouldSucceed()
    {
        var cancellationToken = CancellationToken.None;
        var defaultLocationId = await _seeder.CreateLocationAsync();
        var departmentId = (await _seeder.CreateDepartmentAsync("Бухгалтерия", "BUH", null, [defaultLocationId], cancellationToken)).Value.Id;

        var updateLocations = new List<Guid> { await _seeder.CreateLocationAsync("Коворкинг", "Санкт-Петербург", "Невский проспект", "1") };
        var result = await _seeder.UpdateLocationsAsync(departmentId, updateLocations, cancellationToken);

        await ExecuteDb(async dbContext =>
        {
            var locationsFromDb = dbContext.Departments
                .Include(d => d.Locations)
                .FirstOrDefault(d => d.Id == departmentId)
                ?.Locations
                .Select(l => l.LocationId).ToList();

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Locations.Count == 1);
            Assert.Equal(locationsFromDb, updateLocations);
            Assert.Equal(result.Value.Locations.Select(l => l.LocationId), updateLocations);
        });
    }

    [Fact]
    public async Task UpdateLocations_ValidDataTwoLocations_ShouldSucceed()
    {
        var cancellationToken = CancellationToken.None;
        var defaultLocationId = await _seeder.CreateLocationAsync();
        var departmentId = (await _seeder.CreateDepartmentAsync("Бухгалтерия", "BUH", null, [defaultLocationId], cancellationToken)).Value.Id;

        var updateLocations = new List<Guid>
        {
            await _seeder.CreateLocationAsync("Коворкинг", "Санкт-Петербург", "Невский проспект", "1"),
            await _seeder.CreateLocationAsync("Головной офис", "Москва", "ул. Мира", "1А"),
        };
        var result = await _seeder.UpdateLocationsAsync(departmentId, updateLocations, cancellationToken);

        await ExecuteDb(async dbContext =>
        {
            var locationsFromDb = dbContext.Departments
                .Include(d => d.Locations)
                .FirstOrDefault(d => d.Id == departmentId)
                ?.Locations
                .Select(l => l.LocationId).ToList();

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Locations.Count);
            Assert.Equal(locationsFromDb.OrderBy(o => o), updateLocations.OrderBy(o => o));
            Assert.Equal(result.Value.Locations.Select(l => l.LocationId), updateLocations);
        });
    }

    // Негативные сценарии
    [Fact]
    public async Task UpdateLocations_InValidDataFakeLocation_ShouldFailed()
    {
        var cancellationToken = CancellationToken.None;
        var defaultLocationId = await _seeder.CreateLocationAsync();
        var departmentId = (await _seeder.CreateDepartmentAsync("Бухгалтерия", "BUH", null, [defaultLocationId], cancellationToken)).Value.Id;

        var updateLocations = new List<Guid> { Guid.Empty };
        var result = await _seeder.UpdateLocationsAsync(departmentId, updateLocations, cancellationToken);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task UpdateLocations_InValidDataNonUniqueTwoLocations_ShouldFailed()
    {
        var cancellationToken = CancellationToken.None;
        var defaultLocationId = await _seeder.CreateLocationAsync();
        var departmentId = (await _seeder.CreateDepartmentAsync("Бухгалтерия", "BUH", null, [defaultLocationId], cancellationToken)).Value.Id;
        var locationId = await _seeder.CreateLocationAsync("Коворкинг", "Санкт-Петербург", "Невский проспект", "1");

        var updateLocations = new List<Guid> { locationId, locationId };
        var result = await _seeder.UpdateLocationsAsync(departmentId, updateLocations, cancellationToken);

        Assert.True(result.IsFailure);
    }
}
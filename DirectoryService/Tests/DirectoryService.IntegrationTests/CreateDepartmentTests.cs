using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public class CreateDepartmentTests : IClassFixture<DirectoryServiceTestWebFactory>
{
    private IServiceProvider Services { get; }

    public CreateDepartmentTests(DirectoryServiceTestWebFactory factory)
    {
        Services = factory.Services;
    }

    [Fact]
    public async Task CreateDepartment_with_valid_data_should_succeed()
    {
        // arrage
        var locationId = await CreateLocation();

        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

        var cancellationToken = CancellationToken.None;
        var command = new CreateDepartmentCommand(new CreateDepartmentDto("Подразделение", "podrazd", null, [locationId]));

        // act
        var result = await sut.Handle(command, cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    private async Task<Guid> CreateLocation()
    {
        await using var initializerScope = Services.CreateAsyncScope();
        var dbContext = initializerScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var location = new Location(
            LocationName.Create("Локация").Value,
            Address.Create("Москва", "ул. Мира", "9Ак4").Value,
            Timezone.Create("Europe/Moscow").Value);

        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync();

        return location.Id;
    }
}
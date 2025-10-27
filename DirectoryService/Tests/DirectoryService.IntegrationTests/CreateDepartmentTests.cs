using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public class CreateDepartmentTests : DirectoryServiceBaseTests
{
    public CreateDepartmentTests(DirectoryServiceTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateDepartment_with_valid_data_should_succeed()
    {
        // arrage
        var locationId = await CreateLocation();
        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentDto("Подразделение", "podrazd", null, [locationId]));
            return sut.Handle(command, cancellationToken);
        });

        // assert
        await ExecuteDb(async dbContext =>
        {
            var department = await dbContext.Departments.FirstAsync(d => d.Id == result.Value.Id, cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(department.Id, result.Value.Id);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value.Id);
        });

    }

    private async Task<Guid> CreateLocation()
    {
        return await ExecuteDb(async dbContext =>
        {
            var location = new Location(
                LocationName.Create("Локация").Value,
                Address.Create("Москва", "ул. Мира", "9Ак4").Value,
                Timezone.Create("Europe/Moscow").Value);

            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            return location.Id;
        });
    }

    private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

        return await action(sut);
    }
}
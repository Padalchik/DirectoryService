using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure;

namespace DirectoryService.IntegrationTests.Seeders;

public class LocationSeeder : BaseSeeder
{
    public LocationSeeder(ApplicationDBContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Guid> CreateLocationAsync(
        string name = "Головной офис",
        string city = "Москва",
        string street = "ул. Мира",
        string houseNumber = "9Ак4",
        string timezone = "Europe/Moscow")
    {
        return await ExecuteDb(async db =>
        {
            var location = new Location(
                LocationName.Create(name).Value,
                Address.Create(city, street, houseNumber).Value,
                Timezone.Create(timezone).Value);

            db.Locations.Add(location);
            await db.SaveChangesAsync();

            return location.Id;
        });
    }
}
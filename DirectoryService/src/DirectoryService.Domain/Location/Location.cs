using CSharpFunctionalExtensions;
using DirectoryService.Domain.Position;

namespace DirectoryService.Domain.Location;

public class Location
{
    public Guid Id { get; set; }

    public LocationName Name { get; private set; }

    public Address Address { get; private set; }

    public Timezone Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private Location(LocationName name, Address address, Timezone timezone)
    {
        Id = Guid.NewGuid();
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // EF C0RE
    private Location()
    {
    }
    
    public static Result<Location> Create(LocationName name, Address address, Timezone timezone)
    {
        var location = new Location(name, address, timezone);
        return Result.Success(location);
    }
}
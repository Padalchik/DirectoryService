﻿using DirectoryService.Domain.DepartmentLocations;

namespace DirectoryService.Domain.Locations;

public partial class Location
{
    private HashSet<DepartmentLocation> _departments = [];

    public Guid Id { get; set; }

    public LocationName Name { get; private set; }

    public Address Address { get; private set; }

    public Timezone Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlySet<DepartmentLocation> Departments => _departments;

    public Location(LocationName name, Address address, Timezone timezone)
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
}
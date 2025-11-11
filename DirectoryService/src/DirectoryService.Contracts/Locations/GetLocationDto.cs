using DirectoryService.Contracts.Departments;

namespace DirectoryService.Contracts.Locations;

public record GetLocationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string HouseNumber { get; init; } = string.Empty;
    public string Timezone { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public List<GetDepartmentDto> Departments { get; init; } = [];
}
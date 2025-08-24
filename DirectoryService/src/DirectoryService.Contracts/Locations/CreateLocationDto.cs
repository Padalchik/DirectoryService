namespace DirectoryService.Contracts.Locations;

public record CreateLocationDto(string Name, string City, string Street, string HouseNumber, string Timezone);
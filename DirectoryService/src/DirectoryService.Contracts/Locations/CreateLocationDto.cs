namespace DirectoryService.Contracts.Locations;

public record CreateLocationDto(string Name, Address address, string Timezone);
namespace DirectoryService.Contracts.Locations;

public record CreateLocationDto(string Name, Address Address, string Timezone);
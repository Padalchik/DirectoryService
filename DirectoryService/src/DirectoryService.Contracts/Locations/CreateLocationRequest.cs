namespace DirectoryService.Contracts.Locations;

public record CreateLocationRequest(string Name, Address Address, string Timezone);
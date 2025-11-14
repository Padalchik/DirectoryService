namespace DirectoryService.Contracts.Locations;

public record GetLocationsResponse(IEnumerable<GetLocationResponse> LocationsDto, long TotalCount);
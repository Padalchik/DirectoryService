namespace DirectoryService.Contracts.Locations;

public record GetLocationsResponse(IEnumerable<GetLocationDto> LocationsDto, long TotalCount);
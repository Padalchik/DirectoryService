namespace DirectoryService.Contracts.Locations;

public record GetLocationsRequest(List<Guid>? DepartmentIds, string? Search, bool? IsActive, int Page = 1, int PageSize = 20);
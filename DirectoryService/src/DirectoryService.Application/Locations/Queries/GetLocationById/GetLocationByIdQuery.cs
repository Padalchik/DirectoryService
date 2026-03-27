using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.Queries.GetLocationById;

public record GetLocationByIdQuery(Guid LocationId) : IQuery;
using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.Queries.GetLocationById;

public record GetLocationByIdCommand(Guid LocationId) : ICommand;
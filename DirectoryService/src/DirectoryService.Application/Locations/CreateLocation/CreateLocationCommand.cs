using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Locations.CreateLocation;

public record CreateLocationCommand(CreateLocationDto CreateLocationDto) : ICommand;
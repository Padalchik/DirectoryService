using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Locations;

public record CreateLocationCommand(CreateLocationDto CreateLocationDto) : ICommand;
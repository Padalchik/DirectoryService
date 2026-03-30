using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Positions;

namespace DirectoryService.Application.Positions.Commands;

public record CreatePositionCommand(CreatePositionDto CreatePositionDto) : ICommand;
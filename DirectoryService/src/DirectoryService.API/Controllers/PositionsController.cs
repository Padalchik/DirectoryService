using DirectoryService.API.Response;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Positions;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    [HttpPost]
    public async Task<Envelope> Create(
        [FromServices] ICommandHandler<Position, CreatePositionCommand> handler,
        [FromBody] CreatePositionDto createPositionDto,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(createPositionDto);
        var createPositionResult = await handler.Handle(command, cancellationToken);

        if (createPositionResult.IsFailure)
            return Envelope.Error(createPositionResult.Error);
        else
            return Envelope.Ok(createPositionResult.Value.Id);
    }
}
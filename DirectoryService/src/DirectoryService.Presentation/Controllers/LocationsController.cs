using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<Envelope> Create(
                                            [FromServices] ICommandHandler<Location, CreateLocationCommand> handler,
                                            [FromBody] CreateLocationDto createLocationDto,
                                            CancellationToken cancellationToken)
    {

        var command = new CreateLocationCommand(createLocationDto);
        var createLocationResult = handler.Handle(command, cancellationToken).Result;

        if (createLocationResult.IsFailure)
            return Envelope.Error(createLocationResult.Error);
        else
            return Envelope.Ok(createLocationResult.Value.Id);
    }
}
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Locations;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<Envelope> Create(
                                            [FromServices] LocationsService locationsService,
                                            [FromBody] CreateLocationDto createLocationDto,
                                            CancellationToken cancellationToken)
    {
        var createLocationResult = locationsService.Create(createLocationDto, cancellationToken).Result;

        if (createLocationResult.IsFailure)
            return Envelope.Error(createLocationResult.Error);
        else
            return Envelope.Ok(createLocationResult.Value.Id);
    }
}
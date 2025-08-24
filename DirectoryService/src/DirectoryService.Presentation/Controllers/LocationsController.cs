using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Locations;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
                                            [FromServices] LocationsService locationsService,
                                            [FromBody] CreateLocationDto createLocationDto,
                                            CancellationToken cancellationToken)
    {
        var createLocationResult = locationsService.Create(createLocationDto, cancellationToken).Result;

        if (createLocationResult.IsFailure)
            return BadRequest(createLocationResult.Error);
        else
            return Ok(createLocationResult.Value.Id);
    }
}
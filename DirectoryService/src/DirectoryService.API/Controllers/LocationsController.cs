using DirectoryService.API.Response;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.Commands.CreateLocation;
using DirectoryService.Application.Locations.Queries.GetLocationById;
using DirectoryService.Application.Locations.Queries.GetLocations;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<Envelope> Create(
        [FromServices] ICommandHandler<Location, CreateLocationCommand> handler,
        [FromBody] CreateLocationRequest createLocationRequest,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(createLocationRequest);
        var createLocationResult = await handler.Handle(command, cancellationToken);

        if (createLocationResult.IsFailure)
            return Envelope.Error(createLocationResult.Error);
        else
            return Envelope.Ok(createLocationResult.Value.Id);
    }

    [HttpGet("{locationId}")]
    public async Task<Envelope> GetById(
        [FromServices] IQueryHandler<GetLocationResponse, GetLocationByIdQuery> handler,
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationByIdQuery(locationId);
        var getLocationByIdResult = await handler.Handle(query, cancellationToken);

        if (getLocationByIdResult.IsFailure)
            return Envelope.Error(getLocationByIdResult.Error);
        else
            return Envelope.Ok(getLocationByIdResult.Value);
    }

    [HttpGet]
    public async Task<Envelope> Get(
        [FromQuery] GetLocationsRequest getLocationsRequest,
        [FromServices] IQueryHandler<GetLocationsResponse, GetLocationsQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationsQuery(getLocationsRequest);
        var getLocationsResult = await handler.Handle(query, cancellationToken);

        if (getLocationsResult.IsFailure)
            return Envelope.Error(getLocationsResult.Error);
        else
            return Envelope.Ok(getLocationsResult.Value);
    }
}
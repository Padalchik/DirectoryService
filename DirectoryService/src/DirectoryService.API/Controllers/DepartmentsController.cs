using DirectoryService.API.Response;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.MoveToDepartment;
using DirectoryService.Application.Departments.UpdateLocations;
using DirectoryService.Application.Positions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    [HttpPost]
    public async Task<Envelope> Create(
        [FromServices] ICommandHandler<Department, CreateDepartmentCommand> handler,
        [FromBody] CreateDepartmentDto createDepartmentDto,
        CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand(createDepartmentDto);
        var createDepartmentResult = await handler.Handle(command, cancellationToken);

        if (createDepartmentResult.IsFailure)
            return Envelope.Error(createDepartmentResult.Error);
        else
            return Envelope.Ok(createDepartmentResult.Value.Id);
    }

    [HttpPatch]
    [Route("/api/departments/{departmentId}/locations")]
    public async Task<Envelope> UpdateLocations(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<Department, UpdateLocationsCommand> handler,
        [FromBody] UpdateLocationsDto updateLocationsDto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLocationsCommand(departmentId, updateLocationsDto);
        var updateLocationsResult = await handler.Handle(command, cancellationToken);

        if (updateLocationsResult.IsFailure)
            return Envelope.Error(updateLocationsResult.Error);
        else
            return Envelope.Ok(updateLocationsResult.Value.Id);
    }

    [HttpPatch]
    [Route("/api/departments/{departmentId}/parent")]
    public async Task<Envelope> MoveToDepartment(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<Department, MoveToDepartmentCommand> handler,
        [FromBody] MoveToDepartmentDto moveToDepartmentDto,
        CancellationToken cancellationToken)
    {
        var command = new MoveToDepartmentCommand(departmentId, moveToDepartmentDto);
        var moveToDepartmentResult = await handler.Handle(command, cancellationToken);

        if (moveToDepartmentResult.IsFailure)
            return Envelope.Error(moveToDepartmentResult.Error);
        else
            return Envelope.Ok(moveToDepartmentResult.Value.Id);
    }
}
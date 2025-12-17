using DirectoryService.API.Response;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Commands.CreateDepartment;
using DirectoryService.Application.Departments.Commands.MoveToDepartment;
using DirectoryService.Application.Departments.Commands.SoftDeleteDepartment;
using DirectoryService.Application.Departments.Commands.UpdateLocations;
using DirectoryService.Application.Departments.Queries.GetChildrenByParent;
using DirectoryService.Application.Departments.Queries.GetDepartmentsTopPositions;
using DirectoryService.Application.Departments.Queries.GetRootDepartmentsWithChilden;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.GetChildrenByParent;
using DirectoryService.Contracts.Departments.GetRootDepartmentsWithChilden;
using DirectoryService.Domain.Departments;
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

    [HttpGet]
    [Route("/api/departments/top-positions")]
    public async Task<Envelope> GetDepartmentsTopPositions(
        [FromServices] ICommandHandler<GetTopDepartmentsResponse, GetDepartmentsTopPositionsCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new GetDepartmentsTopPositionsCommand();
        var getDepartmentsTopPositionsResult = await handler.Handle(command, cancellationToken);

        if (getDepartmentsTopPositionsResult.IsFailure)
            return Envelope.Error(getDepartmentsTopPositionsResult.Error);
        else
            return Envelope.Ok(getDepartmentsTopPositionsResult.Value);
    }

    [HttpGet]
    [Route("/api/departments/roots")]
    public async Task<Envelope> GetRootDepartmentsWithChilden(
        [FromQuery] GetRootDepartmentsWithChildenRequest request,
        [FromServices] ICommandHandler<GetRootDepartmentsWithChildenResponse, GetRootDepartmentsWithChildenCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new GetRootDepartmentsWithChildenCommand(request);
        var getRootDepartmentsWithChildenResult = await handler.Handle(command, cancellationToken);

        if (getRootDepartmentsWithChildenResult.IsFailure)
            return Envelope.Error(getRootDepartmentsWithChildenResult.Error);
        else
            return Envelope.Ok(getRootDepartmentsWithChildenResult.Value);
    }

    [HttpGet]
    [Route("/api/departments/{departmentId}")]
    public async Task<Envelope> GetChildrenByParent(
        [FromRoute] Guid departmentId,
        [FromQuery] GetChildrenByParentRequest request,
        [FromServices] ICommandHandler<GetChildrenByParentResponse, GetChildrentByParentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new GetChildrentByParentCommand(departmentId, request);
        var getChildrentByParentResult = await handler.Handle(command, cancellationToken);

        if (getChildrentByParentResult.IsFailure)
            return Envelope.Error(getChildrentByParentResult.Error);
        else
            return Envelope.Ok(getChildrentByParentResult.Value);
    }

    [HttpDelete]
    [Route("/api/departments/{departmentId}")]
    public async Task<Envelope> SoftDelete(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<bool, SoftDeleteDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new SoftDeleteDepartmentCommand(departmentId);
        var softDeleteResult = await handler.Handle(command, cancellationToken);

        if (softDeleteResult.IsFailure)
            return Envelope.Error(softDeleteResult.Error);
        else
            return Envelope.Ok(softDeleteResult.Value);
    }
}
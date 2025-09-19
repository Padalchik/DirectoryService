using DirectoryService.API.Response;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
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
        var createDepartmentResult = handler.Handle(command, cancellationToken).Result;

        if (createDepartmentResult.IsFailure)
            return Envelope.Error(createDepartmentResult.Error);
        else
            return Envelope.Ok(createDepartmentResult.Value.Id);
    }
}
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Shared.Validation;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.UpdateLocations;

public class UpdateLocationsHandler : ICommandHandler<Department, UpdateLocationsCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<UpdateLocationsHandler> _logger;
    private readonly IValidator<UpdateLocationsCommand> _validator;

    public UpdateLocationsHandler(
        IDepartmentsRepository departmentsRepository,
        ILogger<UpdateLocationsHandler> logger,
        IValidator<UpdateLocationsCommand> validator)
    {
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Department, Errors>> Handle(UpdateLocationsCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        var departmentResult = await _departmentsRepository.GetDepartmentByIdAsync(command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            _logger.LogInformation(departmentResult.Error.ToString());
            return departmentResult.Error;
        }

        var department = departmentResult.Value;
        if (department.IsActive == false)
        {
            _logger.LogInformation("Department {departmentId} is not active", department.Id);
            return Error.Validation("department.is.not.active", "Подразделение не активно").ToErrors();
        }

        var departmentLocations = command.UpdateLocationsDto.LocationIds.Select(ids => new DepartmentLocation(ids, department.Id)).ToList();
        var updateLocations = department.UpdateLocations(departmentLocations);
        if (updateLocations.IsFailure)
        {
            _logger.LogInformation(updateLocations.Error.ToString());
            return updateLocations.Error.ToErrors();
        }

        await _departmentsRepository.Save();

        return Result.Success<Department, Errors>(department);
    }
}
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Shared.Validation;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentHandler : ICommandHandler<Department, CreateDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<CreateDepartmentHandler> _logger;
    private readonly IValidator<CreateDepartmentCommand> _validator;

    public CreateDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository, //это я добавил новый репозиторий. МБ надо сделать [FormService]
        ILogger<CreateDepartmentHandler> logger,
        IValidator<CreateDepartmentCommand> validator)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Department, Errors>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        if (!await _locationsRepository.IsLocationsIsExistAsync(
                command.CreateDepartmentDto.LocationIds,
                cancellationToken))
        {
            _logger.LogInformation("Error with LocationIds");
            return GeneralErrors.ValueIsInvalid("LocationIds").ToErrors();
        }

        var departmentNameResult = DepartmentName.Create(command.CreateDepartmentDto.Name);
        var departmentIdentifierResult = DepartmentIdentifier.Create(command.CreateDepartmentDto.Identifier);

        Department? parentDepartment = null;

        if (command.CreateDepartmentDto.ParentId != null)
        {
            var parentDepartmentResult = await _departmentsRepository.GetDepartmentByIdAsync(command.CreateDepartmentDto.ParentId.Value, cancellationToken);
            if (parentDepartmentResult.IsFailure)
            {
                _logger.LogInformation(parentDepartmentResult.Error.ToString());
                return parentDepartmentResult.Error;
            }

            parentDepartment = parentDepartmentResult.Value;
        }

        var createDepartmentResult = Department.Create(departmentNameResult.Value, departmentIdentifierResult.Value, parentDepartment);
        var department = createDepartmentResult.Value;

        var departmentLocations = command.CreateDepartmentDto.LocationIds.Select(ids => new DepartmentLocation(ids, department.Id)).ToList();
        var updateLocations = department.UpdateLocations(departmentLocations);
        if (updateLocations.IsFailure)
        {
            _logger.LogInformation(updateLocations.Error.ToString());
            return updateLocations.Error.ToErrors();
        }

        var addDepartmentResult = await _departmentsRepository.AddAsync(department, cancellationToken);
        if (addDepartmentResult.IsFailure)
        {
            _logger.LogInformation(addDepartmentResult.Error.ToString());
            return addDepartmentResult.Error;
        }

        _logger.LogInformation("Department created with id {departmentId}", department.Id);

        return Result.Success<Department, Errors>(department);
    }
}
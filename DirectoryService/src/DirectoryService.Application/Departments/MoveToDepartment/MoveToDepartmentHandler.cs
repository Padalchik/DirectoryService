using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Shared.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.MoveToDepartment;

public class MoveToDepartmentHandler : ICommandHandler<Department, MoveToDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<MoveToDepartmentHandler> _logger;
    private readonly IValidator<MoveToDepartmentCommand> _validator;

    public MoveToDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ILogger<MoveToDepartmentHandler> logger,
        IValidator<MoveToDepartmentCommand> validator)
    {
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Department, Errors>> Handle(MoveToDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        Department department = null;
        Department parent = null;

        var getDepartmentResult = await _departmentsRepository.GetDepartmentByIdAsync(command.DepartmentId, cancellationToken);
        if (getDepartmentResult.IsFailure)
        {
            _logger.LogInformation("Сouldn't get department {departmentId}", command.DepartmentId);
            return GeneralErrors.NotFound(command.DepartmentId).ToErrors();
        }

        department = getDepartmentResult.Value;

        if (command.MoveToDepartmentDto.ParentId is Guid parentId)
        {
            if (!await _departmentsRepository.IsDepartmentExistAsync(parentId, cancellationToken))
            {
                _logger.LogInformation("Department {departmentId} doesn't exist", parentId);
                return GeneralErrors.ValueIsInvalid("ParentId").ToErrors();
            }

            var getParentResult = await _departmentsRepository.GetDepartmentByIdAsync(parentId, cancellationToken);
            if (getParentResult.IsFailure)
            {
                _logger.LogInformation("Сouldn't get department {departmentId}", parentId);
                return GeneralErrors.NotFound(parentId).ToErrors();
            }

            parent = getParentResult.Value;

            if (!parent.IsActive)
            {
                _logger.LogInformation("Department {departmentId} is not active", parentId);
                return GeneralErrors.Failure().ToErrors();
            }

            var isInChildHierarchyResult = await _departmentsRepository.HasInChildHierarchyAsync(department.Id, parentId, cancellationToken);
            if (isInChildHierarchyResult.IsFailure)
            {
                _logger.LogInformation("Error in HasInChildHierarchyAsync.");
                return GeneralErrors.Failure().ToErrors();
            }

            if (isInChildHierarchyResult.Value == true)
            {
                _logger.LogInformation("A child department cannot be a parent department.");
                return GeneralErrors.Failure().ToErrors();
            }
        }

        if (department == parent)
        {
            _logger.LogInformation("Department couldn't be equal to a parent");
            return GeneralErrors.Failure().ToErrors();
        }

        return Result.Success<Department, Errors>(department);
    }
}
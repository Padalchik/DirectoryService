using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
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
    private readonly ITransactionManager _transactionManager;

    public MoveToDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ILogger<MoveToDepartmentHandler> logger,
        IValidator<MoveToDepartmentCommand> validator,
        ITransactionManager transactionManager)
    {
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _logger = logger;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Department, Errors>> Handle(MoveToDepartmentCommand command, CancellationToken cancellationToken)
    {
        var transactionScopreResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopreResult.IsFailure)
            return transactionScopreResult.Error.ToErrors();

        using var transactionScope = transactionScopreResult.Value;

        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
        {
            transactionScope.Rollback();
            return validationResult.ToList();
        }

        Department department = null;
        Department parent = null;

        await _departmentsRepository.LockDepartmentWithChildHierarchyAsync(command.DepartmentId, cancellationToken);
        var getDepartmentResult = await _departmentsRepository.GetDepartmentByIdAsync(command.DepartmentId, cancellationToken);
        if (getDepartmentResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogInformation("Сouldn't get department {departmentId}", command.DepartmentId);
            return GeneralErrors.NotFound(command.DepartmentId).ToErrors();
        }

        department = getDepartmentResult.Value;

        if (!department.IsActive)
        {
            transactionScope.Rollback();
            _logger.LogInformation("Department {departmentId} is not active", department);
            return GeneralErrors.Failure().ToErrors();
        }

        if (command.MoveToDepartmentDto.ParentId is Guid parentId)
        {
            if (!await _departmentsRepository.IsDepartmentExistAsync(parentId, cancellationToken))
            {
                transactionScope.Rollback();
                _logger.LogInformation("Department {departmentId} doesn't exist", parentId);
                return GeneralErrors.ValueIsInvalid("ParentId").ToErrors();
            }

            var getParentResult = await _departmentsRepository.GetDepartmentByIdAsync(parentId, cancellationToken);
            if (getParentResult.IsFailure)
            {
                transactionScope.Rollback();
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
                transactionScope.Rollback();
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

        string oldPath = department.Path;

        department.MoveToParent(parent);
        await _transactionManager.SaveChangesAsync(cancellationToken);

        string newPath = department.Path;

        await _departmentsRepository.RefreshDepartmentChildPaths(oldPath, newPath, cancellationToken);

        var commiteResult = transactionScope.Commit();
        if (commiteResult.IsFailure)
        {
            transactionScope.Rollback();
            return commiteResult.Error.ToErrors();
        }

        return Result.Success<Department, Errors>(department);
    }
}
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Application.Shared.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Commands.SoftDeleteDepartment;

public class SoftDeleteDepartmentHandler : ICommandHandler<bool, SoftDeleteDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IPositionsRepository _positionsRepository;
    private readonly IValidator<SoftDeleteDepartmentCommand> _validator;
    private readonly ILogger<SoftDeleteDepartmentHandler> _logger;
    private readonly ITransactionManager _transactionManager;
    private readonly ICacheService _cacheService;

    public SoftDeleteDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IPositionsRepository positionsRepository,
        IValidator<SoftDeleteDepartmentCommand> validator,
        ILogger<SoftDeleteDepartmentHandler> logger,
        ITransactionManager transactionManager,
        ICacheService cacheService)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _positionsRepository = positionsRepository;
        _validator = validator;
        _logger = logger;
        _transactionManager = transactionManager;
        _cacheService = cacheService;
    }

    public async Task<Result<bool, Errors>> Handle(SoftDeleteDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
        {
            _logger.LogInformation("The data was not validated");
            return validationResult.ToList();
        }

        var getDepartmentResult = await _departmentsRepository.GetDepartmentByIdAsync(command.DepartmentId, cancellationToken);
        if (getDepartmentResult.IsFailure)
        {
            _logger.LogInformation("Could not get department {departmentId}", command.DepartmentId);
            return GeneralErrors.NotFound(command.DepartmentId).ToErrors();
        }

        if (!getDepartmentResult.Value.IsActive)
        {
            _logger.LogInformation("Department {departmentId} is not active", getDepartmentResult.Value);
            return GeneralErrors.Failure().ToErrors();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            _logger.LogInformation("Couldn't create a transaction");
            return transactionScopeResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopeResult.Value;

        var departmentSoftDeleteResult = await _departmentsRepository.SoftDelete(command.DepartmentId, cancellationToken);
        if (departmentSoftDeleteResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogInformation("Error at 'Department SoftDelete'");
            return departmentSoftDeleteResult.Error;
        }

        var locationSoftDeleteResult = await _locationsRepository.SoftDeleteByDepartmentId(command.DepartmentId, cancellationToken);
        if (locationSoftDeleteResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogInformation("Error at 'Location SoftDelete'");
            return locationSoftDeleteResult.Error;
        }

        var positionSoftDeleteResult = await _positionsRepository.SoftDeleteByDepartmentId(command.DepartmentId, cancellationToken);
        if (positionSoftDeleteResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogInformation("Error at 'Position SoftDelete'");
            return positionSoftDeleteResult.Error;
        }

        var departmentUpdateDeletedPathResult = await _departmentsRepository.UpdateDeletedPath(command.DepartmentId, cancellationToken);
        if (departmentUpdateDeletedPathResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogInformation("Error at 'Department UpdateDeletedPath'");
            return departmentUpdateDeletedPathResult.Error;
        }

        var commiteResult = transactionScope.Commit();
        if (commiteResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogInformation("Error at 'Commit TransactionScope'");
            return commiteResult.Error.ToErrors();
        }

        // ИНВАЛИДАЦИЯ КЭША
        await _cacheService.RemoveByPrefixAsync(
            "departments",
            cancellationToken);

        return Result.Success<bool, Errors>(true);
    }
}
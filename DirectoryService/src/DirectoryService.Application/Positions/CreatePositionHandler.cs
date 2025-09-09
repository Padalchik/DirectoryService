using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Shared.Validation;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions;

public class CreatePositionHandler : ICommandHandler<Position, CreatePositionCommand>
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly ILogger<CreatePositionHandler> _logger;
    private readonly IValidator<CreatePositionCommand> _validator;

    public CreatePositionHandler(
        IPositionsRepository positionsRepository,
        ILogger<CreatePositionHandler> logger,
        IValidator<CreatePositionCommand> validator)
    {
        _positionsRepository = positionsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Position, Errors>> Handle(CreatePositionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        var applicationLogicResult = await IsApplicationLogicCompleteAsync(command, cancellationToken);
        if (applicationLogicResult.IsFailure)
            return applicationLogicResult.Error;

        var positionNameResult = PositionName.Create(command.CreatePositionDto.Name);
        if (positionNameResult.IsFailure)
        {
            _logger.LogInformation(positionNameResult.Error.ToString());
            return positionNameResult.Error.ToErrors();
        }

        var positionDescriptionResult = PositionDescription.Create(command.CreatePositionDto.Description);
        if (positionDescriptionResult.IsFailure)
        {
            _logger.LogInformation(positionDescriptionResult.Error.ToString());
            return positionDescriptionResult.Error.ToErrors();
        }

        var position = new Position(null, positionNameResult.Value, positionDescriptionResult.Value);
        var addPositionResult = await _positionsRepository.AddAsync(position, cancellationToken);
        if (addPositionResult.IsFailure)
        {
            _logger.LogInformation(addPositionResult.Error.ToString());
            return addPositionResult.Error;
        }

        _logger.LogInformation("Position created with id {positionId}", position.Id);

        return Result.Success<Position, Errors>(position);
    }

    private async Task<Result<bool, Errors>> IsApplicationLogicCompleteAsync(CreatePositionCommand command, CancellationToken cancellationToken)
    {
        if (await _positionsRepository.IsNameUsedAsync(command.CreatePositionDto.Name, cancellationToken))
        {
            _logger.LogInformation("PositionName '{positionName}' already exists", command.CreatePositionDto.Name);
            return GeneralErrors.ValueIsInvalid("Name", "Name").ToErrors();
        }

        if (!await _positionsRepository.IsDepartmentsIsActiveAsync(
                command.CreatePositionDto.DepartmentIds,
                cancellationToken))
        {
            _logger.LogInformation("Error with  DepartmentIds");
            return GeneralErrors.ValueIsInvalid("DepartmentIds").ToErrors();
        }

        return Result.Success<bool, Errors>(true);
    }
}
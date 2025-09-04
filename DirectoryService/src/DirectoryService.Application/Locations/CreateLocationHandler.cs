using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Shared.Validation;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Address = DirectoryService.Domain.Locations.Address;

namespace DirectoryService.Application.Locations;

public class CreateLocationHandler : ICommandHandler<Location, CreateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<CreateLocationHandler> _logger;
    private readonly IValidator<CreateLocationCommand> _validator;

    public CreateLocationHandler(
        ILocationsRepository locationsRepository,
        IValidator<CreateLocationCommand> validator,
        ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Location, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        var addressDto = command.CreateLocationDto.Address;
        var locationNameResult = LocationName.Create(command.CreateLocationDto.Name);
        var locationAddressResult = Address.Create(addressDto.City, addressDto.Street, addressDto.HouseNumber);
        var locationTimezoneResult = Timezone.Create(command.CreateLocationDto.Timezone);

        var location = new Location(locationNameResult.Value, locationAddressResult.Value, locationTimezoneResult.Value);
        var addLocationResult = await _locationsRepository.AddAsync(location, cancellationToken);
        if (addLocationResult.IsFailure)
        {
            _logger.LogInformation(addLocationResult.Error.ToString());
            return addLocationResult.Error;
        }

        _logger.LogInformation("Location created with id {locationId}", location.Id);

        return Result.Success<Location, Errors>(location);
    }
}
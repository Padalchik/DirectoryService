using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;
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

    public async Task<Result<Location, Error>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = _validator.ValidateAsync(command, cancellationToken);

        var locationNameResult = LocationName.Create(command.CreateLocationDto.Name);
        if (locationNameResult.IsFailure)
        {
            _logger.LogInformation(locationNameResult.Error.Message);
            return locationNameResult.Error;
        }

        var addressDto = command.CreateLocationDto.address;
        var locationAddressResult = Address.Create(addressDto.City, addressDto.Street, addressDto.HouseNumber);
        if (locationAddressResult.IsFailure)
        {
            _logger.LogInformation(locationAddressResult.Error.Message);
            return locationAddressResult.Error;
        }

        var locationTimezoneResult = Timezone.Create(command.CreateLocationDto.Timezone);
        if (locationTimezoneResult.IsFailure)
        {
            _logger.LogInformation(locationTimezoneResult.Error.Message);
            return locationTimezoneResult.Error;
        }

        var location = new Location(locationNameResult.Value, locationAddressResult.Value, locationTimezoneResult.Value);
        var addLocationResult = await _locationsRepository.AddAsync(location, cancellationToken);
        if (addLocationResult.IsFailure)
        {
            _logger.LogInformation(addLocationResult.Error.Message);
            return addLocationResult.Error;
        }

        _logger.LogInformation("Location created with id {locationId}", location.Id);

        return Result.Success<Location, Error>(location);
    }
}
using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;
using Address = DirectoryService.Domain.Locations.Address;

namespace DirectoryService.Application.Locations;

public class LocationsService
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<LocationsService> _logger;

    public LocationsService(ILocationsRepository locationsRepository, ILogger<LocationsService> logger)
    {
        _locationsRepository = locationsRepository;
        _logger = logger;
    }

    public async Task<Result<Location>> Create(CreateLocationDto createLocationDto, CancellationToken cancellationToken)
    {
        var locationNameResult = LocationName.Create(createLocationDto.Name);
        if (locationNameResult.IsFailure)
        {
            _logger.LogInformation(locationNameResult.Error);
            return Result.Failure<Location>(locationNameResult.Error);
        }

        var addressDto = createLocationDto.address;
        var locationAddressResult = Address.Create(addressDto.City, addressDto.Street, addressDto.HouseNumber);
        if (locationAddressResult.IsFailure)
        {
            _logger.LogInformation(locationAddressResult.Error);
            return Result.Failure<Location>(locationAddressResult.Error);
        }

        var locationTimezoneResult = Timezone.Create(createLocationDto.Timezone);
        if (locationTimezoneResult.IsFailure)
        {
            _logger.LogInformation(locationTimezoneResult.Error);
            return Result.Failure<Location>(locationTimezoneResult.Error);
        }

        var location = new Location(locationNameResult.Value, locationAddressResult.Value, locationTimezoneResult.Value);
        var addLocationResult = await _locationsRepository.AddAsync(location, cancellationToken);
        if (addLocationResult.IsFailure)
        {
            _logger.LogInformation(addLocationResult.Error);
            return Result.Failure<Location>(addLocationResult.Error);
        }

        _logger.LogInformation("Location created with id {locationId}", location.Id);

        return Result.Success(location);
    }
}
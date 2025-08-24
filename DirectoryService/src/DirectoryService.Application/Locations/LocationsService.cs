using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;

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

        var locationAddressResult = Address.Create(createLocationDto.City, createLocationDto.Street, createLocationDto.HouseNumber);
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
        await _locationsRepository.AddAsync(location, cancellationToken);
        _logger.LogInformation("Location created with id {locationId}", location.Id);

        return Result.Success(location);
    }
}
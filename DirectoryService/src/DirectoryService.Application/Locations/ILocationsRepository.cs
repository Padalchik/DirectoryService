using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    Task<Result<Guid, Errors>> AddAsync(Location location, CancellationToken cancellationToken);

    Task<IEnumerable<string>> GetLocationNamesAsync(CancellationToken cancellationToken);

    Task<IEnumerable<Address>> GetAddressesAsync(CancellationToken cancellationToken);
}
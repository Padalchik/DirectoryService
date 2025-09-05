using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    Task<Result<Guid, Errors>> AddAsync(Location location, CancellationToken cancellationToken);

    Task<bool> IsNameUsedAsync(string name, CancellationToken cancellationToken);

    Task<bool> IsAddressUsedAsync(Address address, CancellationToken cancellationToken);
}
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    Task<Result<Guid, Errors>> AddAsync(Location location, CancellationToken cancellationToken);

    Task<bool> IsNameUsedAsync(string name, CancellationToken cancellationToken);

    Task<bool> IsAddressUsedAsync(Address address, CancellationToken cancellationToken);

    Task<bool> IsLocationsIsExistAsync(IEnumerable<Guid> locationIds, CancellationToken cancellationToken);

    Task<UnitResult<Errors>> SoftDeleteByDepartmentId(Guid departmentId, CancellationToken cancellationToken);
}
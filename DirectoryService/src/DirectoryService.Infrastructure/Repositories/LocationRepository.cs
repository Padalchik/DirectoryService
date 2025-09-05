using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationRepository : ILocationsRepository
{
    private readonly ApplicationDBContext _dbContext;

    public LocationRepository(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid, Errors>> AddAsync(Location location, CancellationToken cancellationToken)
    {
        await _dbContext.Locations.AddAsync(location);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success<Guid, Errors>(location.Id);
        }
        catch (Exception ex)
        {
            return GeneralErrors.Failure("Problem with SaveChangesAsync Location").ToErrors();
        }
    }

    public async Task<bool> IsNameUsedAsync(string name, CancellationToken cancellationToken)
    {
        return await _dbContext.Locations.AnyAsync(l => l.Name.Name == name, cancellationToken);
    }

    public async Task<bool> IsAddressUsedAsync(Address address, CancellationToken cancellationToken)
    {
        return await _dbContext.Locations.AnyAsync(l => l.Address == address, cancellationToken);
    }
}
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

    public async Task<IEnumerable<string>> GetLocationNamesAsync(CancellationToken cancellationToken)
    {
        var names = await _dbContext.Locations.Select(l => l.Name.Name).ToListAsync(cancellationToken);
        return names;
    }

    public async Task<IEnumerable<Address>> GetAddressesAsync(CancellationToken cancellationToken)
    {
        var addresses = await _dbContext.Locations.Select(l => l.Address).ToListAsync(cancellationToken);
        return addresses;
    }
}
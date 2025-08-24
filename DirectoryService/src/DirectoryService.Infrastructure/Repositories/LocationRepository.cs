using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationRepository : ILocationsRepository
{
    private readonly ApplicationDBContext _dbContext;

    public LocationRepository(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> AddAsync(Location location, CancellationToken cancellationToken)
    {
        await _dbContext.Locations.AddAsync(location);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(location.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>("Problem with SaveChangesAsync Location");
        }
    }
}
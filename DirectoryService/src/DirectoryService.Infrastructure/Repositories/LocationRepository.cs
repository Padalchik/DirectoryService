using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        await _dbContext.Locations.AddAsync(location, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success<Guid, Errors>(location.Id);
        }
        catch (Exception)
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

    public async Task<bool> IsLocationsIsExistAsync(IEnumerable<Guid> locationIds, CancellationToken cancellationToken)
    {
        int count = await _dbContext.Locations.CountAsync(l => locationIds.Contains(l.Id), cancellationToken);
        return count == locationIds.Count();
    }

    public async Task<UnitResult<Errors>> SoftDeleteByDepartmentId(Guid departmentId, CancellationToken cancellationToken)
    {
        string sql = $"""
                      -- =====================================================
                      -- Soft delete локаций
                      -- =====================================================
                      
                      UPDATE public.locations l
                      SET
                          is_active  = false,
                          deleted_at = @date,
                          updated_at = @date
                      WHERE l.id IN (
                          SELECT DISTINCT dl.location_id
                          FROM public.department_locations dl
                          WHERE dl.department_id = @departmentId
                            AND NOT EXISTS (
                                SELECT 1
                                FROM public.department_locations dl2
                                JOIN public.departments d2
                                    ON d2.id = dl2.department_id
                                WHERE dl2.location_id = dl.location_id
                                  AND d2.is_active = true
                            )
                      );
                      """;

        var parameters = new[]
        {
            new NpgsqlParameter("departmentId", departmentId),
            new NpgsqlParameter("date", DateTime.UtcNow),
        };

        await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);

        return UnitResult.Success<Errors>();
    }
}
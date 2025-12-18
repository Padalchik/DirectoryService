using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DirectoryService.Infrastructure.Repositories;

public class PositionRepository : IPositionsRepository
{
    private readonly ApplicationDBContext _dbContext;

    public PositionRepository(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid, Errors>> AddAsync(Position position, CancellationToken cancellationToken)
    {
        await _dbContext.Positions.AddAsync(position, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success<Guid, Errors>(position.Id);
        }
        catch (Exception ex)
        {
            return GeneralErrors.Failure("Problem with SaveChangesAsync Position").ToErrors();
        }
    }

    public async Task<bool> IsNameUsedAsync(string name, CancellationToken cancellationToken)
    {
        return await _dbContext.Positions.AnyAsync(p => p.IsActive && p.Name.Name == name, cancellationToken);
    }

    public async Task<bool> IsDepartmentsIsActiveAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        int count = await _dbContext.Departments.CountAsync(d => ids.Contains(d.Id) && d.IsActive, cancellationToken);
        return count == ids.Count();
    }

    public async Task<UnitResult<Errors>> SoftDeleteByDepartmentId(Guid departmentId, CancellationToken cancellationToken)
    {
        string sql = $"""
                      -- =====================================================
                      -- Soft delete позиций
                      -- =====================================================
                      
                      UPDATE public.positions p
                      SET
                          is_active  = false,
                          deleted_at = @date,
                          updated_at = @date
                      WHERE p.id IN (
                          SELECT DISTINCT dp.position_id
                          FROM public.department_positions dp
                          WHERE dp.department_id = @departmentId
                            AND NOT EXISTS (
                                SELECT 1
                                FROM public.department_positions dp2
                                JOIN public.departments d2
                                    ON d2.id = dp2.department_id
                                WHERE dp2.position_id = dp.position_id
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
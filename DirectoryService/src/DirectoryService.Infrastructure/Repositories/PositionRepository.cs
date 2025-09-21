using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;

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
}
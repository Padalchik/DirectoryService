using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentsRepository
{
    private readonly ApplicationDBContext _dbContext;

    public DepartmentRepository(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid, Errors>> AddAsync(Department department, CancellationToken cancellationToken)
    {
        await _dbContext.Departments.AddAsync(department, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success<Guid, Errors>(department.Id);
        }
        catch (Exception ex)
        {
            return GeneralErrors.Failure("Problem with SaveChangesAsync Department").ToErrors();
        }
    }

    public async Task<bool> IsLocationsIsExistAsync(IEnumerable<Guid> locationIds, CancellationToken cancellationToken)
    {
        int count = await _dbContext.Locations.CountAsync(l => locationIds.Contains(l.Id), cancellationToken);
        return count == locationIds.Count();
    }

    public async Task<Result<Department, Errors>> GetDepartmentByIdAsync(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments
            .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

        if (department is null)
            return GeneralErrors.NotFound(departmentId).ToErrors();

        return department;
    }
}
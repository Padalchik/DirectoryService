using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentsRepository
{
    private readonly ApplicationDBContext _dbContext;
    private readonly ILogger<DepartmentRepository> _logger;

    public DepartmentRepository(ApplicationDBContext dbContext, ILogger<DepartmentRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
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

    public async Task<Result<Department, Errors>> GetDepartmentByIdAsync(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments
            .Include(d => d.Locations)
            .Include(d => d.Children)
            .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

        if (department is null)
            return GeneralErrors.NotFound(departmentId).ToErrors();

        return department;
    }

    public async Task<Result<Department, Errors>> GetDepartmentByIdWithLockAsync(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                                select *
                                from departments 
                                where path <@ (
                                	select path 
                                	from departments 
                                	where id = @departmentId)
                                FOR UPDATE
                                """;

            var param = new NpgsqlParameter("departmentId", departmentId);

            await _dbContext.Database.ExecuteSqlRawAsync(sql, new[] { param }, cancellationToken);

            var rootDepartment = await _dbContext.Departments
                .Include(d => d.Locations)
                .Include(d => d.Children)
                .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

            if (rootDepartment is null)
                return GeneralErrors.NotFound(departmentId).ToErrors();

            return rootDepartment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting and locking department subtree");
            return UnitResult.Failure<Errors>(GeneralErrors.Failure("Error locking departments")).Error;
        }
    }

    public async Task<UnitResult<Errors>> SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes");
            return UnitResult.Failure<Errors>(GeneralErrors.Failure());
        }
    }

    public async Task<bool> IsDepartmentExistAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        return await _dbContext.Departments.AnyAsync(d => d.Id == departmentId, cancellationToken);
    }

    public async Task<Result<bool, Errors>> HasInChildHierarchyAsync(Guid parentId, Guid possibleChildId, CancellationToken cancellationToken)
    {
        var departmentResult = await GetDepartmentByIdAsync(parentId, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error;

        var department = departmentResult.Value;

        return department.Children.Select(c => c.Id).Contains(possibleChildId);
    }

    public async Task<UnitResult<Errors>> RefreshDepartmentChildPaths(
        string oldPath,
        string newPath,
        CancellationToken cancellationToken)
    {
        string sql = $"""
        UPDATE 
            departments
        SET 
            path = @newPath::ltree || subpath(path, nlevel(@oldPath::ltree)),
            depth = nlevel(@newPath::ltree || subpath(path, nlevel(@oldPath::ltree))) - 1
        WHERE
            path <@ @oldPath::ltree 
            AND path != @oldPath::ltree;
        """;

        var parameters = new[]
        {
            new Npgsql.NpgsqlParameter("newPath", newPath),
            new Npgsql.NpgsqlParameter("oldPath", oldPath),
        };

        await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);

        return UnitResult.Success<Errors>();
    }
}
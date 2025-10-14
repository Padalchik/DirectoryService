using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Errors>> AddAsync(Department department, CancellationToken cancellationToken);

    Task<Result<Department, Errors>> GetDepartmentByIdAsync(Guid departmentId, CancellationToken cancellationToken);

    Task<UnitResult<Errors>> SaveAsync(CancellationToken cancellationToken);

    Task<bool> IsDepartmentExistAsync(Guid departmentId, CancellationToken cancellationToken);

    Task<Result<bool, Errors>> HasInChildHierarchyAsync(Guid parentId, Guid possibleChildId, CancellationToken cancellationToken);
}
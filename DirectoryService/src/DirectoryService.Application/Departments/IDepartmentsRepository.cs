using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Errors>> AddAsync(Department department, CancellationToken cancellationToken);

    Task<bool> IsLocationsIsExistAsync(IEnumerable<Guid> locationIds, CancellationToken cancellationToken);
    
    Task<Result<Department, Errors>> GetDepartmentByIdAsync(Guid departmentId, CancellationToken cancellationToken);
}
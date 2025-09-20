using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Positions;

public interface IPositionsRepository
{
    Task<Result<Guid, Errors>> AddAsync(Position position, CancellationToken cancellationToken);

    Task<bool> IsNameUsedAsync(string name, CancellationToken cancellationToken);

    Task<bool> IsDepartmentsIsActiveAsync(IEnumerable<Guid> departmentIds, CancellationToken cancellationToken);
}
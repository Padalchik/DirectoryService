using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentsTopPositions;

public class GetDepartmentsTopPositionsHandler : ICommandHandler<GetTopDepartmentsResponse, GetDepartmentsTopPositionsCommand>
{
    private readonly IReadDbConext _readDbConext;

    public GetDepartmentsTopPositionsHandler(IReadDbConext readDbConext)
    {
        _readDbConext = readDbConext;
    }

    public async Task<Result<GetTopDepartmentsResponse, Errors>> Handle(
        GetDepartmentsTopPositionsCommand command,
        CancellationToken cancellationToken)
    {
        IQueryable<Department> departmentsQuery = _readDbConext.DepartmentsRead
            .Include(d => d.Positions);

        departmentsQuery = departmentsQuery.OrderByDescending(d => d.Positions.Count).ThenBy(d => d.Name.Name);
        departmentsQuery = departmentsQuery.Take(5);

        var departments = await departmentsQuery
            .Select(d => new DepartmentInfoDto
            {
                Id = d.Id,
                Name = d.Name.Name,
                Identifier = d.Identifier.Identifier,
                ParentId = d.ParentId,
                Path = d.Path,
                Depth = d.Depth,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                ChildrenCount = d.ChildrenCount,
            })
            .ToListAsync(cancellationToken);

        var getTopDepartmentsResponse = new GetTopDepartmentsResponse(departments);

        return getTopDepartmentsResponse;
    }
}
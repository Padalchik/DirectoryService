using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments.Queries.GetRootDepartmentsWithChilden;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.GetChildrenByParent;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Departments.Queries.GetChildrenByParent;

public class GetChildrentByParentHandler : ICommandHandler<GetChildrenByParentResponse, GetChildrentByParentCommand>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetChildrentByParentHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<GetChildrenByParentResponse, Errors>> Handle(
        GetChildrentByParentCommand command,
        CancellationToken cancellationToken)
    {
        string sql =
            $"""
             WITH roots AS (SELECT * FROM public.departments d WHERE d.id = @DepartmentId ORDER BY created_at)
             SELECT
                 *,
                 (EXISTS (SELECT 1 FROM public.departments WHERE parent_id = roots.id OFFSET @Offset LIMIT @Limit)) AS has_more_children
             FROM roots
             
             UNION ALL
             
             SELECT
                 c.*,
                 (EXISTS (SELECT 1 FROM public.departments WHERE parent_id = c.id)) AS has_more_children
             FROM roots r CROSS JOIN LATERAL (
                 SELECT * FROM public.departments d WHERE d.parent_id = r.id
                 ORDER BY created_at
                 LIMIT @Limit
             ) c
             """;

        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<DepartmentInfoDto>(sql, new
        {
            DepartmentId = command.DepartmentId,
            Offset = (command.Request.Page - 1) * command.Request.Size,
            Limit = command.Request.Size,
        });

        var response = new GetChildrenByParentResponse(result.ToList());
        return Result.Success<GetChildrenByParentResponse, Errors>(response);
    }
}
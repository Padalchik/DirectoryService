using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Departments.Queries.GetRootDepartmentsWithChilden;

public class GetRootDepartmentsWithChildenHandle : ICommandHandler<GetRootDepartmentsWithChildenResponse, GetRootDepartmentsWithChildenCommand>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetRootDepartmentsWithChildenHandle(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<GetRootDepartmentsWithChildenResponse, Errors>> Handle(
        GetRootDepartmentsWithChildenCommand command, CancellationToken cancellationToken)
    {
        string sql =
            $"""
              with roots as (
                  SELECT * FROM public.departments d WHERE d.parent_id is null
                  ORDER by created_at
                  OFFSET @Offset LIMIT @RootLimit
              )
              select
                  *,
                  (EXISTS (SELECT 1 FROM public.departments WHERE parent_id = roots.id OFFSET @ChildLimit LIMIT 1)) as has_more_children
              from roots
              
              UNION ALL
              
              SELECT 
                  c.*,
                  (EXISTS(select 1 from public.departments WHERE parent_id = c.id)) as has_more_children
              FROM roots r cross join lateral(
                   SELECT * FROM public.departments d WHERE d.parent_id = r.id
                   ORDER BY created_at
                    LIMIT @ChildLimit
              ) c
              """;

        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<DepartmentInfoDto>(sql, new
        {
            Offset = (command.Request.Page - 1) * command.Request.Size,
            RootLimit = command.Request.Size,
            ChildLimit = command.Request.Prefetch,
        });

        var response = new GetRootDepartmentsWithChildenResponse(result.ToList());
        return Result.Success<GetRootDepartmentsWithChildenResponse, Errors>(response);
    }
}
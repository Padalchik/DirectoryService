using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.GetChildrenByParent;

namespace DirectoryService.Application.Departments.Queries.GetChildrenByParent;

public record GetChildrentByParentQuery(Guid DepartmentId, GetChildrenByParentRequest Request) : IQuery;
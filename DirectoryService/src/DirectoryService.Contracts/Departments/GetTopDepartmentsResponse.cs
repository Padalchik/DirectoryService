namespace DirectoryService.Contracts.Departments;

public record GetTopDepartmentsResponse(IEnumerable<DepartmentInfoDto> Departments);
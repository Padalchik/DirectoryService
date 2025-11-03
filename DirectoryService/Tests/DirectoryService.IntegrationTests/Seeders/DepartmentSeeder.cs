using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Seeders;

public class DepartmentSeeder : BaseSeeder
{
    public DepartmentSeeder(ApplicationDBContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Domain.Departments.Department> CreateDepartmentAsync(
        string name,
        string identifier,
        Guid? parentId,
        IEnumerable<Guid>? locationIds,
        CancellationToken cancellationToken)
    {
        var nameVo = DepartmentName.Create(name).Value;
        var identifierVo = DepartmentIdentifier.Create(identifier).Value;

        Domain.Departments.Department? parentDepartment = await ExecuteDb(async dbConext =>
        {
            return await dbConext.Departments.FirstOrDefaultAsync(d => d.Id == parentId, cancellationToken);
        });

        return await ExecuteDb(async db =>
        {
            var department = Domain.Departments.Department.Create(nameVo, identifierVo, parentDepartment).Value;

            if (locationIds != null)
            {
                var locations = locationIds.Select(ids => new DepartmentLocation(ids, department.Id)).ToList();
                department.UpdateLocations(locations);
            }

            db.Departments.Add(department);
            await db.SaveChangesAsync(cancellationToken);
            return department;
        });
    }
}
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Department;

public class Department
{
    private HashSet<Department> _children = [];
    private HashSet<Location.Location> _locations = [];
    private HashSet<Position.Position> _positions = [];

    public Guid Id { get; private set; }

    public DepartmentName Name { get; private set; }

    public DepartmentIdentifier Identifier { get; private set; }

    public Guid? ParentId { get; private set; }

    public string Path { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public int ChildrenCount => _children.Count();

    public IReadOnlySet<Department> Children => _children;

    public IReadOnlySet<Location.Location> Locations => _locations;

    public IReadOnlySet<Position.Position> Positions => _positions;

    private Department(DepartmentName name, DepartmentIdentifier identifier, Guid? parentId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Identifier = identifier;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        //if (parentId != null)
            //SetParent(parent)
    }

    public static Result<Department> Create(DepartmentName name, DepartmentIdentifier identifier, Guid? parentId)
    {
        var department = new Department(name, identifier, parentId);
        return Result.Success(department);
    }

    public void SetParent(Department? parentDepartment)
    {
        if (parentDepartment == null)
        {
            ParentId = null;
            Depth = 0;
            Path = $"{Name}".ToLowerInvariant();
        }
        else
        {
            if (parentDepartment.Id == Id)
                throw new InvalidOperationException("The parent department can't be itself.");

            ParentId = parentDepartment.Id;
            Path = $"{parentDepartment.Path}.{Name}".ToLower();
            Depth = (short)(parentDepartment.Depth + 1);
            parentDepartment.AddChild(this);
        }

        Touch();
    }

    private void AddChild(Department childDepartment)
    {
        if (childDepartment == null)
            throw new InvalidOperationException("Child department cannot be null");

        if (childDepartment.Id == Id)
            throw new InvalidOperationException("The parent department can't be itself.");

        _children.Add(childDepartment);
        Touch();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}
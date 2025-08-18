using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Department;
public class Department
{
    private HashSet<Department> _children = [];
    private HashSet<DepartmentLocation.DepartmentLocation> _locations = [];
    private HashSet<DepartmentPosition.DepartmentPosition> _positions = [];

    public Guid Id { get; private set; }

    public DepartmentName Name { get; private set; }

    public DepartmentIdentifier Identifier { get; private set; }

    public Department Parent { get; private set; }
    public Guid? ParentId { get; private set; }

    public string Path { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public int ChildrenCount => _children.Count();

    public IReadOnlySet<Department> Children => _children;

    public IReadOnlySet<DepartmentLocation.DepartmentLocation> Locations => _locations;

    public IReadOnlySet<DepartmentPosition.DepartmentPosition> Positions => _positions;

    private Department(DepartmentName name, DepartmentIdentifier identifier, Department? parent)
    {
        Id = Guid.NewGuid();
        Name = name;
        Identifier = identifier;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        if (parent != null)
        {
            ParentId = parent.Id;
            SetParent(parent);
        }
    }

    public static Result<Department> Create(DepartmentName name, DepartmentIdentifier identifier, Department? parent = null)
    {
        var department = new Department(name, identifier, parent);
        return Result.Success(department);
    }

    public void MoveToParent(Department? newParent)
    {
        if (newParent == this)
            throw new InvalidOperationException("The department cannot be its own parent.");

        if (newParent != null && newParent.IsDescendantOf(this))
            throw new InvalidOperationException("Cannot set a descendant as parent.");

        Parent?.RemoveChild(this);
        SetParent(newParent);
        newParent?.AddChild(this);
    }

    private void RemoveChild(Department child)
    {
        _children.Remove(child);
        Touch();
    }

    private void SetParent(Department? parentDepartment)
    {
        if (parentDepartment == null)
        {
            Parent = null;
            ParentId = null;
            Depth = 0;
            Path = $"{Name}".ToLowerInvariant();
        }
        else
        {
            if (parentDepartment.Id == Id)
                throw new InvalidOperationException("The parent department can't be itself.");

            Parent = parentDepartment;
            ParentId = parentDepartment.Id;
            Path = $"{parentDepartment.Path}.{Name}".ToLowerInvariant();
            Depth = (short)(parentDepartment.Depth + 1);
        }

        Touch();
    }

    private void AddChild(Department child)
    {
        if (child == null)
            throw new ArgumentNullException(nameof(child));

        if (child.Id == Id)
            throw new InvalidOperationException("The department cannot be its own child.");

        _children.Add(child);
        Touch();
    }

    private bool IsDescendantOf(Department potentialChild)
    {
        return _children.Contains(potentialChild)
               || _children.Any(child => child.IsDescendantOf(potentialChild));
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}
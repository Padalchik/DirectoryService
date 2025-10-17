using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;
public class Department
{
    private HashSet<Department> _children = [];
    private HashSet<DepartmentLocation> _locations = [];
    private HashSet<DepartmentPosition> _positions = [];

    public Guid Id { get; }

    public DepartmentName Name { get; private set; }

    public DepartmentIdentifier Identifier { get; private set; }

    public Department? Parent { get; private set; }

    public Guid? ParentId { get; private set; }

    public string Path { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public int ChildrenCount => _children.Count;

    public IReadOnlySet<Department> Children => _children;

    public IReadOnlySet<DepartmentLocation> Locations => _locations;

    public IReadOnlySet<DepartmentPosition> Positions => _positions;

    private Department(DepartmentName name, DepartmentIdentifier identifier, Department? parent = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Identifier = identifier;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        SetParent(parent);
    }

    // EF C0RE
    private Department()
    {
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

        if (newParent != null && this.IsAncestorOf(newParent))
            throw new InvalidOperationException("Cannot set a descendant as parent.");

        Parent?.RemoveChild(this);
        SetParent(newParent);
        newParent?.AddChild(this);
    }

    public UnitResult<Error> UpdateLocations(IEnumerable<DepartmentLocation> newLocations)
    {
        if (newLocations.Count() == 0)
            return Error.Validation("department.location", "Department locations must contain at least one location");

        _locations = newLocations.ToHashSet();
        Touch();

        return UnitResult.Success<Error>();
    }

    private void RemoveChild(Department child)
    {
        _children.Remove(child);
        Touch();
    }

    private void SetParent(Department? parentDepartment)
    {
        var selfPath = $"{Identifier.Identifier}".ToLowerInvariant();

        if (parentDepartment == null)
        {
            Parent = null;
            ParentId = null;
            Depth = 0;
            Path = selfPath;
        }
        else
        {
            if (parentDepartment.Id == Id)
                throw new InvalidOperationException("The parent department can't be itself.");

            Parent = parentDepartment;
            ParentId = parentDepartment.Id;
            Path = $"{parentDepartment.Path}.{selfPath}";
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

    private bool IsAncestorOf(Department department)
    {
        if (_children.Contains(department))
            return true;

        return _children.Any(child => child.IsAncestorOf(department));
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}
using DirectoryService.Domain.Department_context;
using DirectoryService.Domain.Location_context;
using DirectoryService.Domain.Position_context;
using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain;

// Один экземпляр представляет весь справочник, в пределах которого проверяется уникальность.
public sealed class DirectoryCatalog
{
    private readonly Dictionary<Guid, Location> _locations = new();
    private readonly Dictionary<Guid, Position> _positions = new();
    private readonly Dictionary<Guid, Department> _departments = new();
    private readonly List<DepartmentLocation> _departmentLocations = new();
    private readonly List<DepartmentPosition> _departmentPositions = new();

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations.AsReadOnly();
    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions.AsReadOnly();

    public Location CreateLocation(Guid id, string name, string address, string timeZone)
    {
        var location = Location.Create(id, name, address, timeZone);
        _locations.Add(id, location);
        return location;
    }

    public void UpdateLocation(Guid id, string name, string address, string timeZone)
    {
        var location = Find(_locations, id);
        location.EnsureActive();

        var validName = LocationName.Create(name);
        var validAddress = LocationAddress.Create(address);
        var validTimeZone = LocationTimeZone.Create(timeZone);

        // При смене адреса проверяем каждое подразделение, где уже есть эта локация.
        foreach (var link in _departmentLocations.Where(link => link.LocationId == id))
        {
            EnsureLocationAddressIsUnique(link.DepartmentId, id, validAddress);
        }

        location.Update(validName, validAddress, validTimeZone);
    }

    public Position CreatePosition(Guid id, string name, string description)
    {
        var position = Position.Create(id, name, description);
        EnsurePositionNameIsUnique(position.Name, id);
        _positions.Add(id, position);
        return position;
    }

    public void RenamePosition(Guid id, string name)
    {
        var position = Find(_positions, id);
        position.EnsureActive();
        var validName = PositionName.Create(name);
        EnsurePositionNameIsUnique(validName, id);
        position.Rename(validName);
    }

    public Department CreateDepartment(Guid id, string name, string identifier, Guid? parentId = null)
    {
        Department? parent = parentId.HasValue ? Find(_departments, parentId.Value) : null;
        parent?.EnsureActive();
        var department = Department.Create(id, name, identifier, parent);
        EnsureDepartmentNameIsUnique(department.Name, id);
        _departments.Add(id, department);
        return department;
    }

    public void UpdateDepartment(Guid id, string name, string identifier)
    {
        var department = Find(_departments, id);
        department.EnsureActive();
        var validName = DepartmentName.Create(name);
        var validIdentifier = DepartmentIdentifier.Create(identifier);
        EnsureDepartmentNameIsUnique(validName, id);

        if (department.Identifier == validIdentifier)
        {
            department.Update(validName, validIdentifier);
            return;
        }

        var path = department.ParentId.HasValue
            ? Find(_departments, department.ParentId.Value).Path.Append(validIdentifier)
            : DepartmentPath.CreateRoot(validIdentifier);
        var updates = BuildHierarchyUpdates(department, department.ParentId, path);

        department.Update(validName, validIdentifier);
        ApplyHierarchyUpdates(updates);
    }

    public DepartmentLocation AttachLocation(Guid departmentId, Guid locationId)
    {
        var department = Find(_departments, departmentId);
        var location = Find(_locations, locationId);
        department.EnsureActive();
        location.EnsureActive();

        if (_departmentLocations.Any(link => link.DepartmentId == departmentId && link.LocationId == locationId))
            throw new InvalidOperationException("Локация уже связана с этим подразделением.");

        EnsureLocationAddressIsUnique(departmentId, locationId, location.Address);

        var link = DepartmentLocation.Create(department, location);
        _departmentLocations.Add(link);
        department.MarkChanged();
        return link;
    }

    public DepartmentPosition AttachPosition(Guid departmentId, Guid positionId)
    {
        var department = Find(_departments, departmentId);
        var position = Find(_positions, positionId);
        department.EnsureActive();
        position.EnsureActive();

        // Имена должностей уникальны во всём справочнике; здесь запрещаем повтор той же связи.
        if (_departmentPositions.Any(link => link.DepartmentId == departmentId && link.PositionId == positionId))
            throw new InvalidOperationException("Должность уже связана с этим подразделением.");

        var link = DepartmentPosition.Create(department, position);
        _departmentPositions.Add(link);
        department.MarkChanged();
        return link;
    }

    public void AttachDepartment(Guid departmentId, Guid parentId)
    {
        var department = Find(_departments, departmentId);
        var parent = Find(_departments, parentId);
        department.EnsureActive();
        parent.EnsureActive();

        if (department.ParentId == parentId)
            throw new InvalidOperationException("Подразделение уже присоединено к этому родителю.");

        // Циклы проверяем по ID сущностей: одинаковые псевдонимы допустимы в разных ветках.
        Department? ancestor = parent;
        while (ancestor is not null)
        {
            if (ancestor.Id == departmentId)
                throw new InvalidOperationException("Подразделение нельзя присоединить к себе или к своему потомку.");

            ancestor = ancestor.ParentId.HasValue
                ? Find(_departments, ancestor.ParentId.Value)
                : null;
        }

        var updates = BuildHierarchyUpdates(department, parentId, parent.Path.Append(department.Identifier));
        ApplyHierarchyUpdates(updates);
    }

    private List<(Department Department, Guid? ParentId, DepartmentPath Path)> BuildHierarchyUpdates(
        Department department, Guid? parentId, DepartmentPath path)
    {
        var childrenByParent = _departments.Values.ToLookup(child => child.ParentId);
        var updates = new List<(Department Department, Guid? ParentId, DepartmentPath Path)>
        {
            (department, parentId, path)
        };

        // Сначала проверяем всю ветку и строим новые пути, затем меняем состояние.
        for (int i = 0; i < updates.Count; i++)
        {
            var update = updates[i];
            foreach (var child in childrenByParent[update.Department.Id])
            {
                child.EnsureActive();
                updates.Add((child, update.Department.Id, update.Path.Append(child.Identifier)));
            }
        }

        return updates;
    }

    private static void ApplyHierarchyUpdates(
        List<(Department Department, Guid? ParentId, DepartmentPath Path)> updates)
    {
        foreach (var update in updates)
        {
            update.Department.SetHierarchy(update.ParentId, update.Path);
        }
    }

    private void EnsurePositionNameIsUnique(PositionName name, Guid id)
    {
        if (_positions.Values.Any(position => position.Id != id && Equal(position.Name.Value, name.Value)))
            throw new InvalidOperationException("Должность с таким названием уже существует.");
    }

    private void EnsureDepartmentNameIsUnique(DepartmentName name, Guid id)
    {
        if (_departments.Values.Any(department => department.Id != id && Equal(department.Name.Value, name.Value)))
            throw new InvalidOperationException("Подразделение с таким названием уже существует.");
    }

    private void EnsureLocationAddressIsUnique(Guid departmentId, Guid locationId, LocationAddress address)
    {
        if (_departmentLocations.Any(link =>
            link.DepartmentId == departmentId && link.LocationId != locationId &&
            Equal(_locations[link.LocationId].Address.Value, address.Value)))
        {
            throw new InvalidOperationException("В подразделении уже есть локация с таким адресом.");
        }
    }

    private static bool Equal(string left, string right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static T Find<T>(Dictionary<Guid, T> items, Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Идентификатор не должен быть пустым.", nameof(id));

        return items.TryGetValue(id, out var item)
            ? item
            : throw new InvalidOperationException("Сущность не найдена в справочнике.");
    }
}

using DirectoryService.Domain.Location_context;

namespace DirectoryService.Domain.Department_context;

public sealed class DepartmentLocation
{
    public Guid DepartmentId { get; }
    public Guid LocationId { get; }

    private DepartmentLocation(Guid departmentId, Guid locationId)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    public static DepartmentLocation Create(Department department, Location location)
    {
        ArgumentNullException.ThrowIfNull(department);
        ArgumentNullException.ThrowIfNull(location);
        return new DepartmentLocation(department.Id, location.Id);
    }
}

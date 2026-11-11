using DirectoryService.Domain.Position_context;

namespace DirectoryService.Domain.Department_context;

public sealed class DepartmentPosition
{
    public Guid DepartmentId { get; }
    public Guid PositionId { get; }

    private DepartmentPosition(Guid departmentId, Guid positionId)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    public static DepartmentPosition Create(Department department, Position position)
    {
        ArgumentNullException.ThrowIfNull(department);
        ArgumentNullException.ThrowIfNull(position);
        return new DepartmentPosition(department.Id, position.Id);
    }
}

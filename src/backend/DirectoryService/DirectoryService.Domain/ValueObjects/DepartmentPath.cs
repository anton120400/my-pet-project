namespace DirectoryService.Domain.ValueObjects;

public sealed record DepartmentPath
{
    public string Value { get; }
    public short Depth { get; }

    private DepartmentPath(string value, short depth)
    {
        Value = value;
        Depth = depth;
    }

    public static DepartmentPath CreateRoot(Guid departmentId)
    {
        return new DepartmentPath(FormatId(departmentId), 0);
    }

    public DepartmentPath Append(Guid departmentId)
    {
        string segment = FormatId(departmentId);
        if (Value.Split('.').Contains(segment))
        {
            throw new ArgumentException("Идентификатор уже встречается в пути.", nameof(departmentId));
        }

        if (Depth == short.MaxValue)
        {
            throw new InvalidOperationException("Достигнута максимальная глубина подразделений.");
        }

        return new DepartmentPath($"{Value}.{segment}", (short)(Depth + 1));
    }

    private static string FormatId(Guid departmentId)
    {
        if (departmentId == Guid.Empty)
        {
            throw new ArgumentException("Идентификатор подразделения не должен быть пустым.", nameof(departmentId));
        }

        return departmentId.ToString("N");
    }
}

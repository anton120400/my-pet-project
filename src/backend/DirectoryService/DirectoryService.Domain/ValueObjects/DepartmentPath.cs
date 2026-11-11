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

    public static DepartmentPath CreateRoot(DepartmentIdentifier identifier)
    {
        return new DepartmentPath(identifier.Value, 0);
    }

    public DepartmentPath Append(DepartmentIdentifier identifier)
    {
        string segment = identifier.Value;
        if (Value.Split('.').Contains(segment))
        {
            throw new ArgumentException("Псевдоним подразделения уже встречается в пути.", nameof(identifier));
        }

        if (Depth == short.MaxValue)
        {
            throw new InvalidOperationException("Достигнута максимальная глубина подразделений.");
        }

        return new DepartmentPath($"{Value}.{segment}", (short)(Depth + 1));
    }

}

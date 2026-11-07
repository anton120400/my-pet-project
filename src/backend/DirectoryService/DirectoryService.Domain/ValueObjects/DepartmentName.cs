namespace DirectoryService.Domain.ValueObjects;

public sealed record DepartmentName
{
    public string Value { get; }
    
    private DepartmentName(string value)
    {
        Value = value;
    }
    
    public static DepartmentName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Название подразделения не должно быть пустым.", 
            nameof(value));
        }
        return new DepartmentName(value.Trim());
    }

}



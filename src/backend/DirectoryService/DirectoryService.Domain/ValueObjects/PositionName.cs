namespace DirectoryService.Domain.ValueObjects;

public sealed record PositionName
{
    public string Value { get; }
    
    private PositionName(string value)
    {
        Value = value;
    }
    
    public static PositionName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Название должности не должно быть пустым.", 
            nameof(value));
        }
        return new PositionName(value.Trim());
    }

}



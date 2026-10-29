namespace DirectoryService.Domain.ValueObjects;

public sealed record LocationName
{
    public string Value { get; }
    
    private LocationName(string value)
    {
        Value = value;
    }
    
    public static LocationName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Название локации не должно быть пустым.", 
            nameof(value));
        }
        return new LocationName(value.Trim());
    }

}

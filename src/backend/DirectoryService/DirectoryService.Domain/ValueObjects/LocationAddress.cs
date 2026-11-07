namespace DirectoryService.Domain.ValueObjects;

public sealed record LocationAddress
{
    public string Value { get; }
    
    private LocationAddress(string value)
    {
        Value = value;
    }
    
    public static LocationAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Адрес локации не должен быть пустым.", 
            nameof(value));
        }
        return new LocationAddress(value.Trim());
    }

}

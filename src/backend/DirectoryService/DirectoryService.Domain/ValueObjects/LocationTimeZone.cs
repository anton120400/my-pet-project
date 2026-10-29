namespace DirectoryService.Domain.ValueObjects;

public sealed record LocationTimeZone
{
    public string Value { get; }
    private LocationTimeZone(string value)
    {
        Value = value;
    }
    public static LocationTimeZone Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Часовой пояс не должен быть пустым.",
            nameof(value));
        }
        
        string validValue = value.Trim();

        if (!TimeZoneInfo.TryConvertIanaIdToWindowsId(validValue, out _))
        {
            throw new ArgumentException("Укажите существующий часовой пояс IANA.",
            nameof(value));
        }
        return new LocationTimeZone(validValue);
    }
}

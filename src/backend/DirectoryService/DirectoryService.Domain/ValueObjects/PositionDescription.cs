namespace DirectoryService.Domain.ValueObjects;


    public sealed record PositionDescription
{
    public string Value { get; }

    private PositionDescription(string value)
    {
        Value = value;
    }

    public static PositionDescription Create(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        return new PositionDescription(value.Trim());
    }
}   

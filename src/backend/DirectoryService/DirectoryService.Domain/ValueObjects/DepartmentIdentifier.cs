using System.Text.RegularExpressions;

namespace DirectoryService.Domain.ValueObjects;

public sealed record DepartmentIdentifier
{
    public string Value { get; }

    private DepartmentIdentifier(string value)
    {
        Value = value;
    }
    public static DepartmentIdentifier Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Псевдоним подразделения не должен быть пустым.", nameof(value));
        }
        string validValue = value.Trim();

        if (!Regex.IsMatch(validValue, @"\A[a-z][a-z0-9]*(?:-[a-z0-9]+)*\z"))
        {
            throw new ArgumentException("Некорректный формат псевдонима подразделения.", 
            nameof(value));
        }
        return new DepartmentIdentifier(validValue);
    }
}
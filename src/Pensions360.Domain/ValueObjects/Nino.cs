namespace Pensions360.Domain.ValueObjects;

public sealed record Nino
{
    public string Value { get; }

    private Nino(string value) => Value = value;

    public static Nino Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("NINO cannot be null or empty.", nameof(input));

        var trimmed = input.Trim().ToUpperInvariant();

        if (!System.Text.RegularExpressions.Regex.IsMatch(trimmed, "^[A-CEGHJ-PR-TW-Z]{2}[0-9]{6}[A-D]$"))
            throw new ArgumentException("Invalid NINO format.", nameof(input));

        return new Nino(trimmed);
    }

    public override string ToString() => Value;
}

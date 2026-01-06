namespace Pensions360.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "GBP")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        Currency = string.IsNullOrWhiteSpace(currency) ? "GBP" : currency.ToUpperInvariant();
        Amount = amount;
    }

    public override string ToString() => $"{Currency} {Amount:F2}";
}

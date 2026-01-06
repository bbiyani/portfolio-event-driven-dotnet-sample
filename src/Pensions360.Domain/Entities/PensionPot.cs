using Pensions360.Domain.ValueObjects;

namespace Pensions360.Domain.Entities;

public sealed class PensionPot
{
    public Guid Id { get; }
    public string ProviderPotId { get; private set; }
    public string ProviderCode { get; private set; }
    public Nino Nino { get; private set; }
    public Money CurrentBalance { get; private set; }
    public DateTime ValuationDate { get; private set; }

    public PensionPot(
        Guid id,
        string providerPotId,
        string providerCode,
        Nino nino,
        Money currentBalance,
        DateTime valuationDate)
    {
        Id = id;
        ProviderPotId = providerPotId;
        ProviderCode = providerCode;
        Nino = nino;
        CurrentBalance = currentBalance;
        ValuationDate = valuationDate;
    }

    public void UpdateValuation(Money newBalance, DateTime newValuationDate)
    {
        if (newValuationDate < ValuationDate)
            return;

        CurrentBalance = newBalance;
        ValuationDate = newValuationDate;
    }
}

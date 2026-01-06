using Pensions360.Domain.ValueObjects;

namespace Pensions360.Domain.Events;

public sealed record PensionPotUpdated(
    Guid PensionPotId,
    Nino Nino,
    string ProviderCode,
    string ProviderPotId,
    Money NewBalance,
    DateTime NewValuationDate,
    DateTime UpdatedAtUtc);

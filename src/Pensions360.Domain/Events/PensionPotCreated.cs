using Pensions360.Domain.ValueObjects;

namespace Pensions360.Domain.Events;

public sealed record PensionPotCreated(
    Guid PensionPotId,
    Nino Nino,
    string ProviderCode,
    string ProviderPotId,
    Money InitialBalance,
    DateTime ValuationDate,
    DateTime CreatedAtUtc);

namespace Pensions360.Shared.Messaging.Events;

public sealed record CitizenDiscoveredEvent(
    string Nino,
    DateTime DiscoveredAtUtc);

public sealed record PensionPotCreatedEvent(
    Guid PensionPotId,
    string Nino,
    string ProviderCode,
    string ProviderPotId,
    decimal Amount,
    string Currency,
    DateTime ValuationDate,
    DateTime CreatedAtUtc);

public sealed record PensionPotUpdatedEvent(
    Guid PensionPotId,
    string Nino,
    string ProviderCode,
    string ProviderPotId,
    decimal Amount,
    string Currency,
    DateTime ValuationDate,
    DateTime UpdatedAtUtc);

public sealed record PensionsSummaryViewedEvent(
    string Nino,
    string? RequestedBy,
    DateTime ViewedAtUtc);

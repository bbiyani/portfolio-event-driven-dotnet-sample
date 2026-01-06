using Pensions360.Domain.ValueObjects;

namespace Pensions360.Domain.Events;

public sealed record PensionsSummaryViewed(
    Nino Nino,
    string? RequestedBy,
    DateTime ViewedAtUtc);

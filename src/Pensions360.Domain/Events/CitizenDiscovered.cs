using Pensions360.Domain.ValueObjects;

namespace Pensions360.Domain.Events;

public sealed record CitizenDiscovered(Nino Nino, DateTime DiscoveredAtUtc);

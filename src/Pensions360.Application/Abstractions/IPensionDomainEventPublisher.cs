using Pensions360.Domain.Events;

namespace Pensions360.Application.Abstractions;

public interface IPensionDomainEventPublisher
{
    Task PublishCitizenDiscoveredAsync(CitizenDiscovered @event, CancellationToken ct = default);
    Task PublishPensionPotCreatedAsync(PensionPotCreated @event, CancellationToken ct = default);
    Task PublishPensionPotUpdatedAsync(PensionPotUpdated @event, CancellationToken ct = default);
    Task PublishPensionsSummaryViewedAsync(PensionsSummaryViewed @event, CancellationToken ct = default);
}

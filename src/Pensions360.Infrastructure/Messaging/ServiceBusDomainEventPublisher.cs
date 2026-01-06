using MassTransit;
using Pensions360.Application.Abstractions;
using Pensions360.Domain.Events;
using Pensions360.Shared.Messaging.Events;

namespace Pensions360.Infrastructure.Messaging;

public sealed class ServiceBusDomainEventPublisher : IPensionDomainEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public ServiceBusDomainEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishCitizenDiscoveredAsync(CitizenDiscovered @event, CancellationToken ct = default)
        => _publishEndpoint.Publish(
            new CitizenDiscoveredEvent(@event.Nino.Value, @event.DiscoveredAtUtc), ct);

    public Task PublishPensionPotCreatedAsync(PensionPotCreated @event, CancellationToken ct = default)
        => _publishEndpoint.Publish(
            new PensionPotCreatedEvent(
                @event.PensionPotId,
                @event.Nino.Value,
                @event.ProviderCode,
                @event.ProviderPotId,
                @event.InitialBalance.Amount,
                @event.InitialBalance.Currency,
                @event.ValuationDate,
                @event.CreatedAtUtc),
            ct);

    public Task PublishPensionPotUpdatedAsync(PensionPotUpdated @event, CancellationToken ct = default)
        => _publishEndpoint.Publish(
            new PensionPotUpdatedEvent(
                @event.PensionPotId,
                @event.Nino.Value,
                @event.ProviderCode,
                @event.ProviderPotId,
                @event.NewBalance.Amount,
                @event.NewBalance.Currency,
                @event.NewValuationDate,
                @event.UpdatedAtUtc),
            ct);

    public Task PublishPensionsSummaryViewedAsync(PensionsSummaryViewed @event, CancellationToken ct = default)
        => _publishEndpoint.Publish(
            new PensionsSummaryViewedEvent(
                @event.Nino.Value,
                @event.RequestedBy,
                @event.ViewedAtUtc),
            ct);
}

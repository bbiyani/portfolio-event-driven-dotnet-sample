using Pensions360.Application.Abstractions;
using Pensions360.Domain.Entities;
using Pensions360.Domain.Events;
using Pensions360.Domain.ValueObjects;

namespace Pensions360.Application.Pensions.Services;

public sealed class PensionIngestionService : IPensionIngestionService
{
    private readonly IPensionDomainEventPublisher _eventPublisher;
    private readonly IPensionPotStateStore _stateStore;

    public PensionIngestionService(
        IPensionDomainEventPublisher eventPublisher,
        IPensionPotStateStore stateStore)
    {
        _eventPublisher = eventPublisher;
        _stateStore = stateStore;
    }

    public async Task ProcessProviderFileAsync(
        string providerCode,
        string blobContainer,
        string blobName,
        DateTime uploadedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var rows = await _stateStore.GetProviderFileRowsAsync(blobContainer, blobName, cancellationToken);

        var seenNinos = new HashSet<string>();

        foreach (var row in rows)
        {
            var nino = Nino.Create(row.Nino);
            var money = new Money(row.Balance, "GBP");
            var now = DateTime.UtcNow;

            if (!seenNinos.Contains(nino.Value))
            {
                seenNinos.Add(nino.Value);

                var citizenExists = await _stateStore.CitizenExistsAsync(nino, cancellationToken);
                if (!citizenExists)
                {
                    var ev = new CitizenDiscovered(nino, now);
                    await _stateStore.UpsertCitizenAsync(new Citizen(nino), cancellationToken);
                    await _eventPublisher.PublishCitizenDiscoveredAsync(ev, cancellationToken);
                }
            }

            var existingPot = await _stateStore.GetPensionPotAsync(
                nino,
                providerCode,
                row.ProviderPotId,
                cancellationToken);

            if (existingPot is null)
            {
                var newPot = new PensionPot(
                    Guid.NewGuid(),
                    row.ProviderPotId,
                    providerCode,
                    nino,
                    money,
                    row.ValuationDate);

                await _stateStore.UpsertPensionPotAsync(newPot, cancellationToken);

                var createdEvent = new PensionPotCreated(
                    newPot.Id,
                    nino,
                    providerCode,
                    row.ProviderPotId,
                    money,
                    row.ValuationDate,
                    now);

                await _eventPublisher.PublishPensionPotCreatedAsync(createdEvent, cancellationToken);
            }
            else
            {
                var oldBalance = existingPot.CurrentBalance;
                var oldDate = existingPot.ValuationDate;

                existingPot.UpdateValuation(money, row.ValuationDate);

                if (existingPot.CurrentBalance != oldBalance || existingPot.ValuationDate != oldDate)
                {
                    await _stateStore.UpsertPensionPotAsync(existingPot, cancellationToken);

                    var updatedEvent = new PensionPotUpdated(
                        existingPot.Id,
                        nino,
                        providerCode,
                        existingPot.ProviderPotId,
                        existingPot.CurrentBalance,
                        existingPot.ValuationDate,
                        now);

                    await _eventPublisher.PublishPensionPotUpdatedAsync(updatedEvent, cancellationToken);
                }
            }
        }
    }
}

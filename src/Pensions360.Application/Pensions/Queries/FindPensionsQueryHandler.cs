using MediatR;
using Pensions360.Application.Abstractions;
using Pensions360.Application.Pensions.Dtos;
using Pensions360.Domain.Events;
using Pensions360.Domain.ValueObjects;

namespace Pensions360.Application.Pensions.Queries;

public sealed class FindPensionsQueryHandler
    : IRequestHandler<FindPensionsQuery, IReadOnlyList<PensionPotDto>>
{
    private readonly IPensionPotReadRepository _readRepository;
    private readonly IPensionDomainEventPublisher _eventPublisher;

    public FindPensionsQueryHandler(
        IPensionPotReadRepository readRepository,
        IPensionDomainEventPublisher eventPublisher)
    {
        _readRepository = readRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<IReadOnlyList<PensionPotDto>> Handle(
        FindPensionsQuery request,
        CancellationToken cancellationToken)
    {
        var nino = Nino.Create(request.Nino);
        var pots = await _readRepository.GetPotsByNinoAsync(nino, cancellationToken);

        var auditEvent = new PensionsSummaryViewed(
            nino,
            RequestedBy: null,
            ViewedAtUtc: DateTime.UtcNow);

        await _eventPublisher.PublishPensionsSummaryViewedAsync(auditEvent, cancellationToken);

        return pots;
    }
}

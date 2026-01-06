using MassTransit;
using Pensions360.Application.Abstractions;
using Pensions360.Shared.Messaging.Commands;

namespace Pensions360.Ingestion.Worker.Consumers;

public sealed class ProcessProviderFileConsumer : IConsumer<ProcessProviderFileCommand>
{
    private readonly IPensionIngestionService _ingestionService;

    public ProcessProviderFileConsumer(IPensionIngestionService ingestionService)
    {
        _ingestionService = ingestionService;
    }

    public async Task Consume(ConsumeContext<ProcessProviderFileCommand> context)
    {
        var msg = context.Message;

        await _ingestionService.ProcessProviderFileAsync(
            msg.ProviderCode,
            msg.BlobContainerName,
            msg.BlobName,
            msg.UploadedAtUtc,
            context.CancellationToken);
    }
}

using MassTransit;
using Microsoft.Extensions.Logging;
using Pensions360.Application.Abstractions;
using Pensions360.Shared.Messaging.Commands;
using Serilog.Context;

namespace Pensions360.Ingestion.Worker.Consumers;

public sealed class ProcessProviderFileConsumer : IConsumer<ProcessProviderFileCommand>
{
    private readonly IPensionIngestionService _ingestionService;
    private readonly ILogger<ProcessProviderFileConsumer> _logger;

    public ProcessProviderFileConsumer(
        IPensionIngestionService ingestionService,
        ILogger<ProcessProviderFileConsumer> logger)
    {
        _ingestionService = ingestionService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessProviderFileCommand> context)
    {
        var msg = context.Message;

        using (LogContext.PushProperty("handler", nameof(ProcessProviderFileConsumer)))
        using (LogContext.PushProperty("messageId", context.MessageId?.ToString()))
        using (LogContext.PushProperty("providerCode", msg.ProviderCode))
        using (LogContext.PushProperty("blobName", msg.BlobName))
        {
            _logger.LogInformation(
                "Processing provider ingestion file {ProviderCode}/{BlobName}.",
                msg.ProviderCode,
                msg.BlobName);

            await _ingestionService.ProcessProviderFileAsync(
                msg.ProviderCode,
                msg.BlobContainerName,
                msg.BlobName,
                msg.UploadedAtUtc,
                context.CancellationToken);

            _logger.LogInformation(
                "Finished provider ingestion file {ProviderCode}/{BlobName}.",
                msg.ProviderCode,
                msg.BlobName);
        }
    }
}

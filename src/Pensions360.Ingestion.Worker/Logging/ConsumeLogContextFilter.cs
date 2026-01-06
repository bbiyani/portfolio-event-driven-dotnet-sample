using MassTransit;
using Serilog.Context;

namespace Pensions360.Ingestion.Worker.Logging;

public sealed class ConsumeLogContextFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var correlationId = context.CorrelationId?.ToString()
                            ?? context.Headers.Get<string>("X-Correlation-Id");

        using (LogContext.PushProperty("correlationId", correlationId))
        using (LogContext.PushProperty("messageId", context.MessageId?.ToString()))
        using (LogContext.PushProperty("eventType", typeof(T).Name))
        {
            await next.Send(context);
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope(nameof(ConsumeLogContextFilter<T>));
    }
}

using GreenWorld.Application.Contracts;
using GreenWorld.Application.Messaging;
using MassTransit;

namespace GreenWorld.Infrastructure.Messaging;

/// <summary>
/// MassTransit consumer. MassTransit resolves this in a per-message DI scope, so
/// the ingestion service (and its DbContext) are fresh per delivery. Appends the
/// event and updates the asset projection.
/// </summary>
public sealed class MeterReadingConsumer : IConsumer<MeterReadingMessage>
{
    private readonly IMeterReadingIngestionService _ingestion;
    public MeterReadingConsumer(IMeterReadingIngestionService ingestion) => _ingestion = ingestion;

    public Task Consume(ConsumeContext<MeterReadingMessage> context)
        => _ingestion.IngestAsync(context.Message, context.CancellationToken);
}

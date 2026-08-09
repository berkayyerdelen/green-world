using GreenWorld.Application.Messaging;

namespace GreenWorld.Application.Contracts;

/// <summary>
/// Ingests a meter reading (as delivered by the RabbitMQ consumer): appends the
/// event to the store and folds it into the asset's cumulative projection.
/// </summary>
public interface IMeterReadingIngestionService
{
    Task IngestAsync(MeterReadingMessage message, CancellationToken ct = default);
}

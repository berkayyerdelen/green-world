using GreenWorld.Application.Contracts;
using GreenWorld.Application.Messaging;
using GreenWorld.Domain.Models.Events;
using GreenWorld.Domain.Repositories;

namespace GreenWorld.Application.Services;

/// <summary>
/// Consumes a meter reading and updates state: appends the immutable event to the
/// store (source of truth) and folds it into the asset's cumulative projection.
/// Runs per message delivered by the RabbitMQ consumer.
/// </summary>
public sealed class MeterReadingIngestionService : IMeterReadingIngestionService
{
    private readonly IAssetRepository _assets;
    private readonly IMeterReadingEventStore _events;

    public MeterReadingIngestionService(IAssetRepository assets, IMeterReadingEventStore events)
    {
        _assets = assets;
        _events = events;
    }

    public async Task IngestAsync(MeterReadingMessage message, CancellationToken ct = default)
    {
        var asset = await _assets.GetAsync(message.AssetId, ct)
            ?? throw new InvalidOperationException($"Unknown asset {message.AssetId}.");

        var reading = new MeterReadingEvent(
            message.ReadingId, message.AssetId, message.OccurredAt,
            message.EnergyKwh, message.PowerKw, message.Direction, DateTimeOffset.UtcNow);

        await _events.AppendAsync(reading, ct);   // append to event store
        asset.ApplyReading(reading);              // update projection
        await _assets.SaveChangesAsync(ct);       // persist projection (+ event) in one UoW
    }
}

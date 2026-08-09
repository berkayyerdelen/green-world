using GreenWorld.Domain.Models.Events;

namespace GreenWorld.Domain.Repositories;

/// <summary>Append-only store of meter readings (event-sourcing source of truth).</summary>
public interface IMeterReadingEventStore
{
    Task AppendAsync(MeterReadingEvent reading, CancellationToken ct = default);

    Task<IReadOnlyList<MeterReadingEvent>> GetForAssetAsync(
        Guid assetId, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default);
}

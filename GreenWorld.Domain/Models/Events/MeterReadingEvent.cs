namespace GreenWorld.Domain.Models.Events;

/// <summary>
/// An immutable meter reading for one asset over one interval. This is the
/// event-sourced source of truth: readings are appended to the store and also
/// folded into each asset's cumulative projection. Conceptually these arrive
/// from meters via RabbitMQ and are persisted by the consumer.
/// </summary>
public sealed class MeterReadingEvent
{
    public Guid Id { get; private set; }
    public Guid AssetId { get; private set; }

    /// <summary>Simulated instant the interval started (meter time).</summary>
    public DateTimeOffset OccurredAt { get; private set; }

    /// <summary>Energy accrued during the interval (kWh, always non-negative).</summary>
    public double EnergyKwh { get; private set; }

    /// <summary>Average power over the interval (kW).</summary>
    public double PowerKw { get; private set; }

    public FlowDirection Direction { get; private set; }

    /// <summary>Wall-clock instant the reading was ingested.</summary>
    public DateTimeOffset ReceivedAt { get; private set; }

    private MeterReadingEvent() { } // EF

    public MeterReadingEvent(Guid id, Guid assetId, DateTimeOffset occurredAt,
        double energyKwh, double powerKw, FlowDirection direction, DateTimeOffset receivedAt)
    {
        if (energyKwh < 0) throw new ArgumentOutOfRangeException(nameof(energyKwh));
        Id = id;
        AssetId = assetId;
        OccurredAt = occurredAt;
        EnergyKwh = energyKwh;
        PowerKw = powerKw;
        Direction = direction;
        ReceivedAt = receivedAt;
    }
}

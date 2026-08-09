using GreenWorld.Domain.Models.Events;

namespace GreenWorld.Domain.Models.Assets;

/// <summary>
/// A metered device belonging to a site. Every asset is its own meter. It carries
/// the sizing parameters the meter simulator needs, and — crucially — a
/// <b>cumulative energy projection</b> (kWh since simulation start) that is
/// updated by folding in <see cref="MeterReadingEvent"/>s as they are ingested.
/// The raw events remain the source of truth; this projection makes real-time
/// reads O(1).
/// </summary>
public sealed class Asset
{
    public Guid Id { get; private set; }
    public Guid SiteId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public AssetKind Kind { get; private set; }
    public FlowDirection Direction { get; private set; }

    // Sizing / behaviour parameters (used by the meter simulator).
    public double CapacityKwp { get; private set; }   // PV
    public double RatedPowerKw { get; private set; }  // heat pump / EV / charger
    public double ScaleFactor { get; private set; }   // base-load scale
    public int Seed { get; private set; }

    // Event-sourced projection.
    public double CumulativeConsumedKwh { get; private set; }
    public double CumulativeGeneratedKwh { get; private set; }
    public double LastPowerKw { get; private set; }
    public DateTimeOffset? LastReadingAt { get; private set; }

    private Asset() { } // EF

    public Asset(Guid id, Guid siteId, string name, AssetKind kind, FlowDirection direction,
        int seed, double capacityKwp = 0, double ratedPowerKw = 0, double scaleFactor = 1)
    {
        Id = id;
        SiteId = siteId;
        Name = name;
        Kind = kind;
        Direction = direction;
        Seed = seed;
        CapacityKwp = capacityKwp;
        RatedPowerKw = ratedPowerKw;
        ScaleFactor = scaleFactor;
    }

    /// <summary>Net cumulative energy (generation positive, consumption negative).</summary>
    public double CumulativeNetKwh => CumulativeGeneratedKwh - CumulativeConsumedKwh;

    /// <summary>Fold a reading into the projection. Idempotency is enforced upstream.</summary>
    public void ApplyReading(MeterReadingEvent reading)
    {
        if (reading.AssetId != Id)
            throw new InvalidOperationException("Reading does not belong to this asset.");

        if (reading.Direction == FlowDirection.Generation)
            CumulativeGeneratedKwh += reading.EnergyKwh;
        else
            CumulativeConsumedKwh += reading.EnergyKwh;

        LastPowerKw = reading.PowerKw;
        LastReadingAt = reading.OccurredAt;
    }
}

namespace GreenWorld.Domain.Models;

/// <summary>
/// A point-in-time aggregate of the whole neighbourhood: the simulated weather &
/// season context, instantaneous power, and cumulative energy. Written per
/// ingested tick to give "aggregate power/energy over time" for charts and
/// historical queries.
/// </summary>
public sealed class NeighbourhoodAggregateSnapshot
{
    public Guid Id { get; private set; }
    public Guid NeighbourhoodId { get; private set; }
    public DateTimeOffset At { get; private set; }

    public Season Season { get; private set; }
    public double TemperatureCelsius { get; private set; }
    public double CloudCover { get; private set; }
    public double IrradianceFactor { get; private set; }

    public double TotalConsumptionKw { get; private set; }
    public double TotalGenerationKw { get; private set; }
    public double CumulativeConsumedKwh { get; private set; }
    public double CumulativeGeneratedKwh { get; private set; }

    private NeighbourhoodAggregateSnapshot() { } // EF

    public NeighbourhoodAggregateSnapshot(Guid id, Guid neighbourhoodId, DateTimeOffset at,
        Season season, double temperatureCelsius, double cloudCover, double irradianceFactor,
        double totalConsumptionKw, double totalGenerationKw,
        double cumulativeConsumedKwh, double cumulativeGeneratedKwh)
    {
        Id = id;
        NeighbourhoodId = neighbourhoodId;
        At = at;
        Season = season;
        TemperatureCelsius = temperatureCelsius;
        CloudCover = cloudCover;
        IrradianceFactor = irradianceFactor;
        TotalConsumptionKw = totalConsumptionKw;
        TotalGenerationKw = totalGenerationKw;
        CumulativeConsumedKwh = cumulativeConsumedKwh;
        CumulativeGeneratedKwh = cumulativeGeneratedKwh;
    }

    public double NetKw => TotalGenerationKw - TotalConsumptionKw;
}

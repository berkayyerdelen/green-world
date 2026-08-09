namespace GreenWorld.Application.Responses;

public sealed record AggregateStateResponse(
    Guid NeighbourhoodId,
    DateTimeOffset? At,
    string? Season,
    double TemperatureCelsius,
    double CloudCover,
    double IrradianceFactor,
    double TotalConsumptionKw,
    double TotalGenerationKw,
    double NetKw,
    double CumulativeConsumedKwh,
    double CumulativeGeneratedKwh);

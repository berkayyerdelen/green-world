namespace GreenWorld.Application.Responses;

public sealed record AggregateStateResponse(
    Guid NeighbourhoodId,
    DateTimeOffset? At,
    double TotalConsumptionKw,
    double TotalGenerationKw,
    double NetKw,
    double CumulativeConsumedKwh,
    double CumulativeGeneratedKwh);

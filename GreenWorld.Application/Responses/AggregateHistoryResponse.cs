namespace GreenWorld.Application.Responses;

public sealed record AggregatePointDto(
    DateTimeOffset At,
    double TotalConsumptionKw,
    double TotalGenerationKw,
    double NetKw,
    double CumulativeConsumedKwh,
    double CumulativeGeneratedKwh);

public sealed record AggregateHistoryResponse(int Count, IReadOnlyList<AggregatePointDto> Points);

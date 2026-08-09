namespace GreenWorld.Application.Responses;

public sealed record AggregatePointDto(
    DateTimeOffset At,
    string Season,
    double TemperatureCelsius,
    double TotalConsumptionKw,
    double TotalGenerationKw,
    double NetKw,
    double CumulativeConsumedKwh,
    double CumulativeGeneratedKwh);

public sealed record AggregateHistoryResponse(int Count, IReadOnlyList<AggregatePointDto> Points);

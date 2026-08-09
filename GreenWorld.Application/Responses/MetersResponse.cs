namespace GreenWorld.Application.Responses;

public sealed record MeterDto(
    Guid AssetId, string AssetName, string Kind, string Direction,
    Guid SiteId, double CumulativeConsumedKwh, double CumulativeGeneratedKwh,
    DateTimeOffset? LastReadingAt);

public sealed record MetersResponse(
    double TotalConsumedKwh, double TotalGeneratedKwh, IReadOnlyList<MeterDto> Meters);

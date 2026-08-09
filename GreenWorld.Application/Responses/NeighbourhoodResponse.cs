namespace GreenWorld.Application.Responses;

public sealed record AssetDto(
    Guid Id, string Name, string Kind, string Direction,
    double CumulativeConsumedKwh, double CumulativeGeneratedKwh,
    double LastPowerKw, DateTimeOffset? LastReadingAt);

public sealed record SiteDto(
    Guid Id, string Name, string SiteType,
    double CumulativeConsumedKwh, double CumulativeGeneratedKwh,
    IReadOnlyList<AssetDto> Assets);

public sealed record NeighbourhoodResponse(
    Guid Id, string Name, DateTimeOffset SimulationStart,
    int HouseholdCount, int PublicFacilityCount,
    IReadOnlyList<SiteDto> Sites);

using GreenWorld.Application.Contracts;
using GreenWorld.Application.Responses;
using GreenWorld.Domain.Repositories;
using GreenWorld.SharedKernel.Configurations;

namespace GreenWorld.Application.Services;

/// <summary>
/// Read-side of the system. Real-time figures come from the fast cumulative
/// projections on each asset; historical figures come from the aggregate
/// snapshot series and the raw event store.
/// </summary>
public sealed class NeighbourhoodQueryService : INeighbourhoodQueryService
{
    private readonly INeighbourhoodRepository _neighbourhoods;
    private readonly INeighbourhoodAggregateStore _aggregates;
    private readonly IMeterReadingEventStore _events;
    private readonly NeighbourhoodConfiguration _config;

    public NeighbourhoodQueryService(
        INeighbourhoodRepository neighbourhoods,
        INeighbourhoodAggregateStore aggregates,
        IMeterReadingEventStore events,
        NeighbourhoodConfiguration config)
    {
        _neighbourhoods = neighbourhoods;
        _aggregates = aggregates;
        _events = events;
        _config = config;
    }

    public async Task<NeighbourhoodResponse> GetNeighbourhoodAsync(CancellationToken ct = default)
    {
        var neighbourhood = await Load(ct);
        var sites = neighbourhood.Sites.Select(s => new SiteDto(
            s.Id, s.Name, s.SiteType.ToString(),
            s.CumulativeConsumedKwh, s.CumulativeGeneratedKwh,
            s.Assets.Select(ToAssetDto).ToList())).ToList();

        return new NeighbourhoodResponse(
            neighbourhood.Id, neighbourhood.Name, neighbourhood.SimulationStart,
            neighbourhood.Households.Count(), neighbourhood.PublicFacilities.Count(), sites);
    }

    public async Task<MetersResponse> GetMetersAsync(CancellationToken ct = default)
    {
        var neighbourhood = await Load(ct);
        var meters = neighbourhood.Sites
            .SelectMany(s => s.Assets.Select(a => new MeterDto(
                a.Id, a.Name, a.Kind.ToString(), a.Direction.ToString(), s.Id,
                a.CumulativeConsumedKwh, a.CumulativeGeneratedKwh, a.LastReadingAt)))
            .ToList();
        return new MetersResponse(
            meters.Sum(m => m.CumulativeConsumedKwh),
            meters.Sum(m => m.CumulativeGeneratedKwh), meters);
    }

    public async Task<AggregateStateResponse> GetAggregateStateAsync(CancellationToken ct = default)
    {
        var neighbourhood = await Load(ct);
        var latest = await _aggregates.GetLatestAsync(neighbourhood.Id, ct);
        var capacity = _config.BatteryCapacityKwh;
        var soc = latest?.BatterySocKwh ?? 0;
        return new AggregateStateResponse(
            neighbourhood.Id, latest?.At,
            latest?.Season.ToString(),
            latest?.TemperatureCelsius ?? 0,
            latest?.CloudCover ?? 0,
            latest?.IrradianceFactor ?? 0,
            latest?.TotalConsumptionKw ?? 0,
            latest?.TotalGenerationKw ?? 0,
            latest?.NetKw ?? 0,
            neighbourhood.AllAssets().Sum(a => a.CumulativeConsumedKwh),
            neighbourhood.AllAssets().Sum(a => a.CumulativeGeneratedKwh),
            latest?.NetLoadWithoutBatteryKw ?? 0,
            latest?.NetLoadWithBatteryKw ?? 0,
            latest?.BatteryPowerKw ?? 0,
            soc,
            capacity,
            capacity > 0 ? soc / capacity * 100 : 0,
            _config.BatteryDischargeThresholdKw,
            _config.BatteryChargeThresholdKw);
    }

    public async Task<AggregateHistoryResponse> GetAggregateHistoryAsync(
        DateTimeOffset? from, DateTimeOffset? to, int? lastN, CancellationToken ct = default)
    {
        var n = await Load(ct);
        var points = await _aggregates.GetRangeAsync(n.Id, from, to, lastN, ct);
        var dtos = points.Select(p => new AggregatePointDto(
            p.At, p.Season.ToString(), p.TemperatureCelsius,
            p.TotalConsumptionKw, p.TotalGenerationKw, p.NetKw,
            p.CumulativeConsumedKwh, p.CumulativeGeneratedKwh,
            p.NetLoadWithoutBatteryKw, p.NetLoadWithBatteryKw,
            p.BatteryPowerKw, p.BatterySocKwh)).ToList();
        return new AggregateHistoryResponse(dtos.Count, dtos);
    }

    public async Task<AssetHistoryResponse> GetAssetHistoryAsync(
        Guid assetId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
    {
        var readings = await _events.GetForAssetAsync(assetId, from, to, ct);
        var dtos = readings.Select(r => new ReadingDto(
            r.OccurredAt, r.EnergyKwh, r.PowerKw, r.Direction.ToString())).ToList();
        return new AssetHistoryResponse(assetId, dtos.Count, dtos);
    }

    private async Task<Domain.Models.Neighbourhood> Load(CancellationToken ct)
        => await _neighbourhoods.GetGraphAsync(ct)
           ?? throw new InvalidOperationException("Neighbourhood has not been seeded.");

    private static AssetDto ToAssetDto(Domain.Models.Assets.Asset a) => new(
        a.Id, a.Name, a.Kind.ToString(), a.Direction.ToString(),
        a.CumulativeConsumedKwh, a.CumulativeGeneratedKwh, a.LastPowerKw, a.LastReadingAt);
}

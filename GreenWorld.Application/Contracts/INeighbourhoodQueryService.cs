using GreenWorld.Application.Responses;

namespace GreenWorld.Application.Contracts;

/// <summary>Real-time and historical reads over the neighbourhood's energy state.</summary>
public interface INeighbourhoodQueryService
{
    /// <summary>Structure of the neighbourhood: sites and assets with cumulative energy.</summary>
    Task<NeighbourhoodResponse> GetNeighbourhoodAsync(CancellationToken ct = default);

    /// <summary>Cumulative energy per asset since simulation start.</summary>
    Task<MetersResponse> GetMetersAsync(CancellationToken ct = default);

    /// <summary>Latest aggregate power + cumulative energy for the whole neighbourhood.</summary>
    Task<AggregateStateResponse> GetAggregateStateAsync(CancellationToken ct = default);

    /// <summary>Neighbourhood aggregate power/energy over time.</summary>
    Task<AggregateHistoryResponse> GetAggregateHistoryAsync(
        DateTimeOffset? from, DateTimeOffset? to, int? lastN, CancellationToken ct = default);

    /// <summary>Raw reading history for a single asset.</summary>
    Task<AssetHistoryResponse> GetAssetHistoryAsync(
        Guid assetId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default);
}

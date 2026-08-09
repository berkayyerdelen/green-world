using GreenWorld.Domain.Models;

namespace GreenWorld.Domain.Repositories;

/// <summary>Persists and queries neighbourhood aggregate snapshots over time.</summary>
public interface INeighbourhoodAggregateStore
{
    Task AddAsync(NeighbourhoodAggregateSnapshot snapshot, CancellationToken ct = default);
    Task<NeighbourhoodAggregateSnapshot?> GetLatestAsync(Guid neighbourhoodId, CancellationToken ct = default);
    Task<IReadOnlyList<NeighbourhoodAggregateSnapshot>> GetRangeAsync(
        Guid neighbourhoodId, DateTimeOffset? from, DateTimeOffset? to, int? lastN, CancellationToken ct = default);
}

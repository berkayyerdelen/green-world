using GreenWorld.Domain.Models;

namespace GreenWorld.Domain.Repositories;

/// <summary>Loads/persists the neighbourhood aggregate (sites + assets).</summary>
public interface INeighbourhoodRepository
{
    Task<bool> ExistsAnyAsync(CancellationToken ct = default);
    Task AddAsync(Neighbourhood neighbourhood, CancellationToken ct = default);

    /// <summary>The single neighbourhood with its sites and assets loaded.</summary>
    Task<Neighbourhood?> GetGraphAsync(CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}

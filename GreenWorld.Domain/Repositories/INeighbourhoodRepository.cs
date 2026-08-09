using GreenWorld.Domain.Models;

namespace GreenWorld.Domain.Repositories;

public interface INeighbourhoodRepository
{
    Task<Neighbourhood?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Neighbourhood>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Neighbourhood neighbourhood, CancellationToken ct = default);
}

using GreenWorld.Domain.Models;
using GreenWorld.Domain.Repositories;
using GreenWorld.Infrastructure.Persistence;

namespace GreenWorld.Infrastructure.Repositories;

public sealed class NeighbourhoodRepository : INeighbourhoodRepository
{
    private readonly ApplicationContext _context;

    public NeighbourhoodRepository(ApplicationContext context) => _context = context;

    public Task<Neighbourhood?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_context.Neighbourhoods.FirstOrDefault(n => n.Id == id));

    public Task<IReadOnlyList<Neighbourhood>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Neighbourhood>>(_context.Neighbourhoods.ToList());

    public Task AddAsync(Neighbourhood neighbourhood, CancellationToken ct = default)
    {
        _context.Neighbourhoods.Add(neighbourhood);
        return Task.CompletedTask;
    }
}

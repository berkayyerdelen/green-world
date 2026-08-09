using GreenWorld.Domain.Models;
using GreenWorld.Domain.Repositories;
using GreenWorld.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenWorld.Infrastructure.Repositories;

public sealed class NeighbourhoodRepository : INeighbourhoodRepository
{
    private readonly GreenWorldDbContext _db;
    public NeighbourhoodRepository(GreenWorldDbContext db) => _db = db;

    public Task<bool> ExistsAnyAsync(CancellationToken ct = default)
        => _db.Neighbourhoods.AnyAsync(ct);

    public async Task AddAsync(Neighbourhood neighbourhood, CancellationToken ct = default)
        => await _db.Neighbourhoods.AddAsync(neighbourhood, ct);

    public Task<Neighbourhood?> GetGraphAsync(CancellationToken ct = default)
        => _db.Neighbourhoods
            .Include(n => n.Sites).ThenInclude(s => s.Assets)
            .AsSplitQuery()
            .FirstOrDefaultAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}

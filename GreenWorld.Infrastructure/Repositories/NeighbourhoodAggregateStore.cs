using GreenWorld.Domain.Models;
using GreenWorld.Domain.Repositories;
using GreenWorld.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenWorld.Infrastructure.Repositories;

public sealed class NeighbourhoodAggregateStore : INeighbourhoodAggregateStore
{
    private readonly GreenWorldDbContext _db;
    public NeighbourhoodAggregateStore(GreenWorldDbContext db) => _db = db;

    public async Task AddAsync(NeighbourhoodAggregateSnapshot snapshot, CancellationToken ct = default)
    {
        await _db.AggregateSnapshots.AddAsync(snapshot, ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task<NeighbourhoodAggregateSnapshot?> GetLatestAsync(Guid neighbourhoodId, CancellationToken ct = default)
        => _db.AggregateSnapshots.AsNoTracking()
            .Where(s => s.NeighbourhoodId == neighbourhoodId)
            .OrderByDescending(s => s.At)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<NeighbourhoodAggregateSnapshot>> GetRangeAsync(
        Guid neighbourhoodId, DateTimeOffset? from, DateTimeOffset? to, int? lastN, CancellationToken ct = default)
    {
        var q = _db.AggregateSnapshots.AsNoTracking().Where(s => s.NeighbourhoodId == neighbourhoodId);
        if (from is not null) q = q.Where(s => s.At >= from);
        if (to is not null) q = q.Where(s => s.At <= to);

        if (lastN is > 0)
            return (await q.OrderByDescending(s => s.At).Take(lastN.Value).ToListAsync(ct))
                   .OrderBy(s => s.At).ToList();

        return await q.OrderBy(s => s.At).ToListAsync(ct);
    }
}

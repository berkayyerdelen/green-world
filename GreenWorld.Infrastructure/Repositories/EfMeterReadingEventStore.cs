using GreenWorld.Domain.Models.Events;
using GreenWorld.Domain.Repositories;
using GreenWorld.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenWorld.Infrastructure.Repositories;

public sealed class EfMeterReadingEventStore : IMeterReadingEventStore
{
    private readonly GreenWorldDbContext _db;
    public EfMeterReadingEventStore(GreenWorldDbContext db) => _db = db;

    /// <summary>Adds to the context; persisted by the caller's unit of work.</summary>
    public async Task AppendAsync(MeterReadingEvent reading, CancellationToken ct = default)
        => await _db.MeterReadings.AddAsync(reading, ct);

    public async Task<IReadOnlyList<MeterReadingEvent>> GetForAssetAsync(
        Guid assetId, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default)
    {
        var q = _db.MeterReadings.AsNoTracking().Where(r => r.AssetId == assetId);
        if (from is not null) q = q.Where(r => r.OccurredAt >= from);
        if (to is not null) q = q.Where(r => r.OccurredAt <= to);
        return await q.OrderBy(r => r.OccurredAt).ToListAsync(ct);
    }
}

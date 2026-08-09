using GreenWorld.Domain.Models.Assets;
using GreenWorld.Domain.Repositories;
using GreenWorld.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenWorld.Infrastructure.Repositories;

public sealed class EfAssetRepository : IAssetRepository
{
    private readonly GreenWorldDbContext _db;
    public EfAssetRepository(GreenWorldDbContext db) => _db = db;

    public Task<Asset?> GetAsync(Guid assetId, CancellationToken ct = default)
        => _db.Assets.FirstOrDefaultAsync(a => a.Id == assetId, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}

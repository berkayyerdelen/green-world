using GreenWorld.Domain.Models.Assets;

namespace GreenWorld.Domain.Repositories;

/// <summary>Tracked access to individual assets for projection updates.</summary>
public interface IAssetRepository
{
    Task<Asset?> GetAsync(Guid assetId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

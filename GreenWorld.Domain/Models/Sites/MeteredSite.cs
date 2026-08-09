using GreenWorld.Domain.Models.Assets;

namespace GreenWorld.Domain.Models.Sites;

/// <summary>
/// A metered location in the neighbourhood that owns a set of assets. Two kinds
/// exist (TPH-mapped): <see cref="Household"/> and <see cref="PublicFacility"/>.
/// </summary>
public abstract class MeteredSite
{
    private readonly List<Asset> _assets = new();

    public Guid Id { get; private set; }
    public Guid NeighbourhoodId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public IReadOnlyList<Asset> Assets => _assets;

    public abstract SiteType SiteType { get; }

    protected MeteredSite() { } // EF

    protected MeteredSite(Guid id, Guid neighbourhoodId, string name)
    {
        Id = id;
        NeighbourhoodId = neighbourhoodId;
        Name = name;
    }

    public void AddAsset(Asset asset) => _assets.Add(asset);

    public bool Has(AssetKind kind) => _assets.Any(a => a.Kind == kind);
    public double CumulativeConsumedKwh => _assets.Sum(a => a.CumulativeConsumedKwh);
    public double CumulativeGeneratedKwh => _assets.Sum(a => a.CumulativeGeneratedKwh);
}

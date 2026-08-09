using GreenWorld.Domain.Models.Assets;
using GreenWorld.Domain.Models.Sites;

namespace GreenWorld.Domain.Models;

/// <summary>
/// Aggregate root: the neighbourhood and all the sites (households + public
/// facilities) it contains.
/// </summary>
public sealed class Neighbourhood
{
    private readonly List<MeteredSite> _sites = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    /// <summary>Simulation start instant; cumulative energy is measured from here.</summary>
    public DateTimeOffset SimulationStart { get; private set; }

    public IReadOnlyList<MeteredSite> Sites => _sites;

    private Neighbourhood() { } // EF

    public Neighbourhood(Guid id, string name, DateTimeOffset simulationStart)
    {
        Id = id;
        Name = name;
        SimulationStart = simulationStart;
    }

    public void AddSite(MeteredSite site) => _sites.Add(site);

    public IEnumerable<Household> Households => _sites.OfType<Household>();
    public IEnumerable<PublicFacility> PublicFacilities => _sites.OfType<PublicFacility>();
    public IEnumerable<Asset> AllAssets() => _sites.SelectMany(s => s.Assets);
}

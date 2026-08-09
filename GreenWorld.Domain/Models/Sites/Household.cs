namespace GreenWorld.Domain.Models.Sites;

/// <summary>A residence. May own a base load plus optional PV, heat pump, home EV charger.</summary>
public sealed class Household : MeteredSite
{
    public override SiteType SiteType => SiteType.Household;

    private Household() { } // EF

    public Household(Guid id, Guid neighbourhoodId, string name)
        : base(id, neighbourhoodId, name) { }
}

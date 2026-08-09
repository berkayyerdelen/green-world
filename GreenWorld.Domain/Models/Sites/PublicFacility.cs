namespace GreenWorld.Domain.Models.Sites;

/// <summary>Shared neighbourhood infrastructure, e.g. a public EV charger.</summary>
public sealed class PublicFacility : MeteredSite
{
    public override SiteType SiteType => SiteType.PublicFacility;

    private PublicFacility() { } // EF

    public PublicFacility(Guid id, Guid neighbourhoodId, string name)
        : base(id, neighbourhoodId, name) { }
}

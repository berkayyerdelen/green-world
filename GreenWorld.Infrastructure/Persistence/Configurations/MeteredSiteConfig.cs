using GreenWorld.Domain.Models.Sites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenWorld.Infrastructure.Persistence.Configurations;

public sealed class MeteredSiteConfig : IEntityTypeConfiguration<MeteredSite>
{
    public void Configure(EntityTypeBuilder<MeteredSite> b)
    {
        b.ToTable("sites");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.NeighbourhoodId);

        // Table-per-hierarchy: one table, discriminator column.
        b.HasDiscriminator<string>("site_type")
            .HasValue<Household>("Household")
            .HasValue<PublicFacility>("PublicFacility");

        b.HasMany(x => x.Assets).WithOne()
            .HasForeignKey(a => a.SiteId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Assets).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.Ignore(x => x.SiteType);          // computed from the concrete type
        b.Ignore(x => x.CumulativeConsumedKwh);
        b.Ignore(x => x.CumulativeGeneratedKwh);
    }
}

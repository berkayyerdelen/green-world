using GreenWorld.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenWorld.Infrastructure.Persistence.Configurations;

public sealed class NeighbourhoodConfig : IEntityTypeConfiguration<Neighbourhood>
{
    public void Configure(EntityTypeBuilder<Neighbourhood> b)
    {
        b.ToTable("neighbourhoods");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.SimulationStart);

        b.HasMany(x => x.Sites).WithOne()
            .HasForeignKey(s => s.NeighbourhoodId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Sites).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.Ignore(x => x.Households);
        b.Ignore(x => x.PublicFacilities);
    }
}

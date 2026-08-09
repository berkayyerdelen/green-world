using GreenWorld.Domain.Models.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenWorld.Infrastructure.Persistence.Configurations;

public sealed class AssetConfig : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> b)
    {
        b.ToTable("assets");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.Direction).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CapacityKwp);
        b.Property(x => x.RatedPowerKw);
        b.Property(x => x.ScaleFactor);
        b.Property(x => x.Seed);
        b.Property(x => x.CumulativeConsumedKwh);
        b.Property(x => x.CumulativeGeneratedKwh);
        b.Property(x => x.LastPowerKw);
        b.Property(x => x.LastReadingAt);
        b.HasIndex(x => x.SiteId);
        b.Ignore(x => x.CumulativeNetKwh);
    }
}

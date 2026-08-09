using GreenWorld.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenWorld.Infrastructure.Persistence.Configurations;

public sealed class AggregateSnapshotConfig : IEntityTypeConfiguration<NeighbourhoodAggregateSnapshot>
{
    public void Configure(EntityTypeBuilder<NeighbourhoodAggregateSnapshot> b)
    {
        b.ToTable("neighbourhood_aggregate_snapshots");
        b.HasKey(x => x.Id);
        b.Property(x => x.Season).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(x => new { x.NeighbourhoodId, x.At });
        b.Ignore(x => x.NetKw);
    }
}

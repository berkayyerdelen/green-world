using GreenWorld.Domain.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenWorld.Infrastructure.Persistence.Configurations;

public sealed class MeterReadingEventConfig : IEntityTypeConfiguration<MeterReadingEvent>
{
    public void Configure(EntityTypeBuilder<MeterReadingEvent> b)
    {
        b.ToTable("meter_readings");
        b.HasKey(x => x.Id);
        b.Property(x => x.Direction).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(x => new { x.AssetId, x.OccurredAt });
    }
}

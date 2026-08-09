using GreenWorld.Domain.Models;
using GreenWorld.Domain.Models.Assets;
using GreenWorld.Domain.Models.Events;
using GreenWorld.Domain.Models.Sites;
using Microsoft.EntityFrameworkCore;

namespace GreenWorld.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the neighbourhood, sites (TPH), assets, the append-only
/// meter-reading event store, and neighbourhood aggregate snapshots.
/// </summary>
public sealed class GreenWorldDbContext : DbContext
{
    public GreenWorldDbContext(DbContextOptions<GreenWorldDbContext> options) : base(options) { }

    public DbSet<Neighbourhood> Neighbourhoods => Set<Neighbourhood>();
    public DbSet<MeteredSite> Sites => Set<MeteredSite>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<MeterReadingEvent> MeterReadings => Set<MeterReadingEvent>();
    public DbSet<NeighbourhoodAggregateSnapshot> AggregateSnapshots => Set<NeighbourhoodAggregateSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(GreenWorldDbContext).Assembly);
}

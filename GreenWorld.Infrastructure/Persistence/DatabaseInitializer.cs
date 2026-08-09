using GreenWorld.Domain.Repositories;
using GreenWorld.Infrastructure.Seed;
using GreenWorld.SharedKernel.Configurations;
using Microsoft.EntityFrameworkCore;

namespace GreenWorld.Infrastructure.Persistence;

/// <summary>
/// Ensures the schema exists and the neighbourhood is seeded once. Uses
/// EnsureCreated for zero-friction startup; swap for db.Database.Migrate() once
/// EF migrations are added.
/// </summary>
public sealed class DatabaseInitializer
{
    private readonly GreenWorldDbContext _db;
    private readonly INeighbourhoodRepository _neighbourhoods;
    private readonly NeighbourhoodConfiguration _config;

    public DatabaseInitializer(
        GreenWorldDbContext db,
        INeighbourhoodRepository neighbourhoods,
        NeighbourhoodConfiguration config)
    {
        _db = db;
        _neighbourhoods = neighbourhoods;
        _config = config;
    }

    public async Task InitialiseAsync(CancellationToken ct = default)
    {
        await _db.Database.EnsureCreatedAsync(ct);

        if (await _neighbourhoods.ExistsAnyAsync(ct)) return;

        var neighbourhood = new NeighbourhoodSeeder().Build(_config);
        await _neighbourhoods.AddAsync(neighbourhood, ct);
        await _neighbourhoods.SaveChangesAsync(ct);
    }
}

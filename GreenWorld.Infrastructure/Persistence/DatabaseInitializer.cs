using GreenWorld.Domain.Repositories;
using GreenWorld.Infrastructure.Seed;
using GreenWorld.SharedKernel.Configurations;
using Microsoft.EntityFrameworkCore;

namespace GreenWorld.Infrastructure.Persistence;

/// <summary>
/// Applies EF migrations (creating the schema if needed) and seeds the
/// neighbourhood once.
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
        await _db.Database.MigrateAsync(ct);

        if (await _neighbourhoods.ExistsAnyAsync(ct)) return;

        var neighbourhood = new NeighbourhoodSeeder().Build(_config);
        await _neighbourhoods.AddAsync(neighbourhood, ct);
        await _neighbourhoods.SaveChangesAsync(ct);
    }
}

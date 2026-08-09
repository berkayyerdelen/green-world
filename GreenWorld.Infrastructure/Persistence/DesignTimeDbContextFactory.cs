using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GreenWorld.Infrastructure.Persistence;

/// <summary>
/// Lets EF tooling (dotnet ef migrations/database) build the context without the
/// web host. Uses the local Postgres connection by default; override with the
/// GREENWORLD_DB environment variable.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GreenWorldDbContext>
{
    public GreenWorldDbContext CreateDbContext(string[] args)
    {
        var conn = Environment.GetEnvironmentVariable("GREENWORLD_DB")
                   ?? "Host=localhost;Port=5432;Database=greenworld;Username=greenworld;Password=greenworld";
        var options = new DbContextOptionsBuilder<GreenWorldDbContext>()
            .UseNpgsql(conn)
            .Options;
        return new GreenWorldDbContext(options);
    }
}

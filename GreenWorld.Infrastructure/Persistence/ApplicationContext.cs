using GreenWorld.Domain.Models;

namespace GreenWorld.Infrastructure.Persistence;

/// <summary>
/// In-memory data store placeholder. Swap for EF Core DbContext / Mongo etc.
/// when persistence is implemented.
/// </summary>
public sealed class ApplicationContext
{
    public List<Neighbourhood> Neighbourhoods { get; } = new();
}

namespace GreenWorld.Domain.Models;

/// <summary>
/// The result of the simulation for a single time step: how much was consumed,
/// generated, and the resulting net balance for the neighbourhood.
/// </summary>
public sealed record SimulationTick(
    DateTimeOffset Timestamp,
    EnergyAmount Consumed,
    EnergyAmount Generated)
{
    /// <summary>Positive = surplus fed to grid, Negative = drawn from grid.</summary>
    public EnergyAmount Net => Generated - Consumed;
}

namespace GreenWorld.Domain.Models;

/// <summary>
/// Value object representing an amount of energy in kilowatt-hours (kWh).
/// Immutable; supports simple arithmetic used by the simulation.
/// </summary>
public readonly record struct EnergyAmount(double Kilowatthours)
{
    public static readonly EnergyAmount Zero = new(0);

    public static EnergyAmount operator +(EnergyAmount a, EnergyAmount b) => new(a.Kilowatthours + b.Kilowatthours);
    public static EnergyAmount operator -(EnergyAmount a, EnergyAmount b) => new(a.Kilowatthours - b.Kilowatthours);

    public override string ToString() => $"{Kilowatthours:0.###} kWh";
}

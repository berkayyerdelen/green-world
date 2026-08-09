namespace GreenWorld.Domain.Models;

/// <summary>
/// Immutable context handed to every asset for a single tick: the simulated
/// time, the season, the weather, and the step duration (so an asset can turn
/// its power into energy for this tick).
/// </summary>
public readonly record struct SimulationContext(
    DateTimeOffset Timestamp,
    Season Season,
    Weather Weather,
    TimeSpan Step)
{
    public double StepHours => Step.TotalHours;
    public int Hour => Timestamp.Hour;
    public int DayOfYear => Timestamp.DayOfYear;
}

namespace GreenWorld.Domain.Models;

/// <summary>
/// The controllable simulation clock. Knows when the simulation started, the
/// step size, and the current simulated instant. Advancing moves time forward
/// by whole steps only, so accounting stays aligned to tick boundaries.
/// </summary>
public sealed class SimulationClock
{
    public DateTimeOffset Start { get; }
    public TimeSpan Step { get; }
    public DateTimeOffset Current { get; private set; }

    public SimulationClock(DateTimeOffset start, TimeSpan step)
    {
        if (step <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(step));
        Start = start;
        Step = step;
        Current = start;
    }

    /// <summary>Number of whole steps elapsed since the start.</summary>
    public long ElapsedSteps => (long)Math.Round((Current - Start) / Step);

    /// <summary>Advance one step and return the new current time.</summary>
    public DateTimeOffset Advance()
    {
        Current += Step;
        return Current;
    }
}

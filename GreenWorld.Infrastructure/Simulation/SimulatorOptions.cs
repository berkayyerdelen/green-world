namespace GreenWorld.Infrastructure.Simulation;

/// <summary>Runtime pacing for the meter simulator (bound from configuration).</summary>
public sealed class SimulatorOptions
{
    public const string SectionName = "Simulator";

    /// <summary>Enable the background meter simulator.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Wall-clock delay between simulated ticks (ms). Lower = faster.</summary>
    public int StepDelayMs { get; set; } = 1000;

    /// <summary>Stop after this many steps (0 = run indefinitely).</summary>
    public int MaxSteps { get; set; } = 0;

    /// <summary>Seconds to wait on startup for DB/broker to be ready.</summary>
    public int StartupDelaySeconds { get; set; } = 5;
}

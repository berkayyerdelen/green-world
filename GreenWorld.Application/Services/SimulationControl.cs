using GreenWorld.Application.Contracts;

namespace GreenWorld.Application.Services;

/// <summary>Thread-safe implementation of <see cref="ISimulationControl"/>.</summary>
public sealed class SimulationControl : ISimulationControl
{
    private volatile bool _paused;
    private volatile int _stepDelayMs;

    public SimulationControl(int initialStepDelayMs = 1000)
        => _stepDelayMs = Math.Max(0, initialStepDelayMs);

    public bool IsPaused => _paused;
    public int StepDelayMs => _stepDelayMs;

    public void Pause() => _paused = true;
    public void Resume() => _paused = false;
    public void SetStepDelay(int milliseconds) => _stepDelayMs = Math.Clamp(milliseconds, 0, 60_000);
}

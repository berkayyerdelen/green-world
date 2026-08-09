namespace GreenWorld.Application.Contracts;

/// <summary>
/// Runtime control over the simulation clock. Shared singleton read by the
/// background meter simulator each tick and written by the API. Lets the clock be
/// paused/resumed and its pace changed while the app runs.
/// </summary>
public interface ISimulationControl
{
    bool IsPaused { get; }
    int StepDelayMs { get; }

    void Pause();
    void Resume();
    void SetStepDelay(int milliseconds);
}

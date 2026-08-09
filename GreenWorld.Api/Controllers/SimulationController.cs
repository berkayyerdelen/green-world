using GreenWorld.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GreenWorld.Api.Controllers;

/// <summary>Controls the simulation clock at runtime: pause, resume, and pace.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class SimulationController : ControllerBase
{
    private readonly ISimulationControl _control;
    public SimulationController(ISimulationControl control) => _control = control;

    public sealed record StatusDto(bool IsPaused, int StepDelayMs);

    /// <summary>Current pause state and pace (ms of wall-clock per simulated tick).</summary>
    [HttpGet("status")]
    public IActionResult Status() => Ok(new StatusDto(_control.IsPaused, _control.StepDelayMs));

    /// <summary>Pause the clock (meters stop advancing).</summary>
    [HttpPost("pause")]
    public IActionResult Pause() { _control.Pause(); return Ok(new StatusDto(_control.IsPaused, _control.StepDelayMs)); }

    /// <summary>Resume the clock.</summary>
    [HttpPost("resume")]
    public IActionResult Resume() { _control.Resume(); return Ok(new StatusDto(_control.IsPaused, _control.StepDelayMs)); }

    /// <summary>Set the pace: wall-clock milliseconds per simulated tick (0–60000).</summary>
    [HttpPost("speed")]
    public IActionResult Speed([FromQuery] int delayMs)
    {
        _control.SetStepDelay(delayMs);
        return Ok(new StatusDto(_control.IsPaused, _control.StepDelayMs));
    }
}

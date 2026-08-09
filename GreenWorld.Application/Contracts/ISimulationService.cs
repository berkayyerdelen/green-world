using GreenWorld.Application.Requests;
using GreenWorld.Application.Responses;

namespace GreenWorld.Application.Contracts;

/// <summary>
/// Use case: run a time-stepped simulation of a neighbourhood's electricity
/// consumption and generation, returning the per-tick series.
/// </summary>
public interface ISimulationService
{
    Task<SimulationResult> RunAsync(RunSimulationRequest request, CancellationToken ct = default);
}

using GreenWorld.Application.Contracts;
using GreenWorld.Application.Extensions.MappingExtensions;
using GreenWorld.Application.Requests;
using GreenWorld.Application.Responses;
using GreenWorld.Domain.Exceptions;
using GreenWorld.Domain.Models;
using GreenWorld.Domain.Policies.Contracts;
using GreenWorld.Domain.Repositories;

namespace GreenWorld.Application.Services;

/// <summary>
/// Orchestrates the simulation: loads the neighbourhood, steps through time and
/// aggregates consumption/generation for every household using the injected policies.
/// </summary>
public sealed class SimulationService : ISimulationService
{
    private readonly INeighbourhoodRepository _neighbourhoods;
    private readonly IConsumptionPolicy _consumption;
    private readonly IGenerationPolicy _generation;

    public SimulationService(
        INeighbourhoodRepository neighbourhoods,
        IConsumptionPolicy consumption,
        IGenerationPolicy generation)
    {
        _neighbourhoods = neighbourhoods;
        _consumption = consumption;
        _generation = generation;
    }

    public async Task<SimulationResult> RunAsync(RunSimulationRequest request, CancellationToken ct = default)
    {
        var neighbourhood = await _neighbourhoods.GetByIdAsync(request.NeighbourhoodId, ct)
            ?? throw new NeighbourhoodNotFoundException(request.NeighbourhoodId);

        var ticks = new List<SimulationTickDto>();
        for (var t = request.From; t < request.To; t += request.Step)
        {
            var consumed = EnergyAmount.Zero;
            var generated = EnergyAmount.Zero;
            foreach (var household in neighbourhood.Households)
            {
                consumed += _consumption.ConsumptionAt(household, t);
                generated += _generation.GenerationAt(household, t);
            }
            ticks.Add(new SimulationTick(t, consumed, generated).ToDto());
        }

        return new SimulationResult(neighbourhood.Id, ticks);
    }
}

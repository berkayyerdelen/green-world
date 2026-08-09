using GreenWorld.Api.Requests;
using GreenWorld.Application.Contracts;
using GreenWorld.Application.Requests;
using Microsoft.AspNetCore.Mvc;

namespace GreenWorld.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SimulationController : ControllerBase
{
    private readonly ISimulationService _simulation;

    public SimulationController(ISimulationService simulation) => _simulation = simulation;

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] RunSimulationApiRequest request, CancellationToken ct)
    {
        var result = await _simulation.RunAsync(
            new RunSimulationRequest(request.NeighbourhoodId, request.From, request.To, request.Step), ct);
        return Ok(result);
    }
}

using GreenWorld.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GreenWorld.Api.Controllers;

/// <summary>Read model: structure, meters, and aggregate power/energy over time.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class NeighbourhoodController : ControllerBase
{
    private readonly INeighbourhoodQueryService _query;
    public NeighbourhoodController(INeighbourhoodQueryService query) => _query = query;

    /// <summary>Neighbourhood structure: sites and assets with cumulative energy.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await _query.GetNeighbourhoodAsync(ct));

    /// <summary>Cumulative energy (kWh) per asset/meter since simulation start.</summary>
    [HttpGet("meters")]
    public async Task<IActionResult> Meters(CancellationToken ct)
        => Ok(await _query.GetMetersAsync(ct));

    /// <summary>Latest aggregate power + cumulative energy for the neighbourhood (real time).</summary>
    [HttpGet("aggregate")]
    public async Task<IActionResult> Aggregate(CancellationToken ct)
        => Ok(await _query.GetAggregateStateAsync(ct));

    /// <summary>Aggregate power/energy over time. Filter with ?from, ?to or ?last=N.</summary>
    [HttpGet("aggregate/history")]
    public async Task<IActionResult> AggregateHistory(
        [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to,
        [FromQuery] int? last, CancellationToken ct)
        => Ok(await _query.GetAggregateHistoryAsync(from, to, last, ct));

    /// <summary>Raw meter-reading history for one asset (from the event store).</summary>
    [HttpGet("assets/{assetId:guid}/history")]
    public async Task<IActionResult> AssetHistory(
        Guid assetId, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken ct)
        => Ok(await _query.GetAssetHistoryAsync(assetId, from, to, ct));
}

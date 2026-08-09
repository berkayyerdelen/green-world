using GreenWorld.Application.Contracts;
using GreenWorld.Application.Messaging;
using GreenWorld.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace GreenWorld.Api.Controllers;

/// <summary>
/// Manual meter-reading ingress. Lets you publish a reading onto the same
/// RabbitMQ pipeline the simulated meters use (useful for testing the flow).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class MeterReadingsController : ControllerBase
{
    private readonly IMeterReadingPublisher _publisher;
    public MeterReadingsController(IMeterReadingPublisher publisher) => _publisher = publisher;

    public sealed record PublishReadingRequest(
        Guid AssetId, DateTimeOffset OccurredAt, double EnergyKwh, double PowerKw, FlowDirection Direction);

    /// <summary>Publish a meter reading to the queue; the consumer will ingest it.</summary>
    [HttpPost]
    public async Task<IActionResult> Publish([FromBody] PublishReadingRequest request, CancellationToken ct)
    {
        var message = new MeterReadingMessage(
            Guid.NewGuid(), request.AssetId, request.OccurredAt,
            request.EnergyKwh, request.PowerKw, request.Direction);
        await _publisher.PublishAsync(message, ct);
        return Accepted(message);
    }
}

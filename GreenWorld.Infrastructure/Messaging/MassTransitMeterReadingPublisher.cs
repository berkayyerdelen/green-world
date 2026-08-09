using GreenWorld.Application.Contracts;
using GreenWorld.Application.Messaging;
using MassTransit;

namespace GreenWorld.Infrastructure.Messaging;

/// <summary>Publishes meter readings through MassTransit (RabbitMQ transport).</summary>
public sealed class MassTransitMeterReadingPublisher : IMeterReadingPublisher
{
    private readonly IBus _bus;
    public MassTransitMeterReadingPublisher(IBus bus) => _bus = bus;

    public Task PublishAsync(MeterReadingMessage message, CancellationToken ct = default)
        => _bus.Publish(message, ct);
}

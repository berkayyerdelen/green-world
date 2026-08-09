using GreenWorld.Application.Messaging;

namespace GreenWorld.Application.Contracts;

/// <summary>Publishes meter readings onto the message bus (RabbitMQ).</summary>
public interface IMeterReadingPublisher
{
    Task PublishAsync(MeterReadingMessage message, CancellationToken ct = default);
}

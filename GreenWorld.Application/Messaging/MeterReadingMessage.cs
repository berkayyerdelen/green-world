using GreenWorld.Domain.Models;

namespace GreenWorld.Application.Messaging;

/// <summary>
/// The payload a meter publishes to RabbitMQ for one interval. Deliberately flat
/// and serializer-friendly; the consumer turns it into a domain
/// <c>MeterReadingEvent</c>.
/// </summary>
public sealed record MeterReadingMessage(
    Guid ReadingId,
    Guid AssetId,
    DateTimeOffset OccurredAt,
    double EnergyKwh,
    double PowerKw,
    FlowDirection Direction);

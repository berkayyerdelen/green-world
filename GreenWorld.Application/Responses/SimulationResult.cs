namespace GreenWorld.Application.Responses;

/// <summary>Application-layer output DTO: the simulated series over time.</summary>
public sealed record SimulationResult(
    Guid NeighbourhoodId,
    IReadOnlyList<SimulationTickDto> Ticks);

public sealed record SimulationTickDto(
    DateTimeOffset Timestamp,
    double ConsumedKwh,
    double GeneratedKwh,
    double NetKwh);

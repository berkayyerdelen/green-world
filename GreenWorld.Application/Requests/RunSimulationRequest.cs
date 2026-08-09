namespace GreenWorld.Application.Requests;

/// <summary>Application-layer input DTO describing a simulation to run.</summary>
public sealed record RunSimulationRequest(
    Guid NeighbourhoodId,
    DateTimeOffset From,
    DateTimeOffset To,
    TimeSpan Step);

namespace GreenWorld.Api.Requests;

/// <summary>Api-layer input model. Mapped to the Application request in the controller.</summary>
public sealed record RunSimulationApiRequest(
    Guid NeighbourhoodId,
    DateTimeOffset From,
    DateTimeOffset To,
    TimeSpan Step);

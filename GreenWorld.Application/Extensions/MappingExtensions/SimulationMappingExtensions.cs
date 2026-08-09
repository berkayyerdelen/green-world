using GreenWorld.Application.Responses;
using GreenWorld.Domain.Models;

namespace GreenWorld.Application.Extensions.MappingExtensions;

/// <summary>Maps domain models to Application DTOs.</summary>
public static class SimulationMappingExtensions
{
    public static SimulationTickDto ToDto(this SimulationTick tick) => new(
        tick.Timestamp,
        tick.Consumed.Kilowatthours,
        tick.Generated.Kilowatthours,
        tick.Net.Kilowatthours);
}

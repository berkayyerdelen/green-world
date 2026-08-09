using GreenWorld.Domain.Models;

namespace GreenWorld.Domain.Policies.Contracts;

/// <summary>
/// Produces deterministic weather for any simulated instant. Implementations
/// must be pure functions of (seed, timestamp): the same inputs always yield the
/// same weather, so runs are fully reproducible.
/// </summary>
public interface IWeatherModel
{
    Weather WeatherAt(DateTimeOffset timestamp);
}

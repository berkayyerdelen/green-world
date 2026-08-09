using GreenWorld.Domain.Models;
using GreenWorld.Domain.Policies.Contracts;

namespace GreenWorld.Infrastructure.Policies;

/// <summary>Placeholder policy: crude solar curve peaking at midday.</summary>
public sealed class DaylightGenerationPolicy : IGenerationPolicy
{
    public EnergyAmount GenerationAt(Household household, DateTimeOffset timestamp)
    {
        var hour = timestamp.Hour + timestamp.Minute / 60.0;
        var output = Math.Max(0, Math.Sin((hour - 6) / 12.0 * Math.PI));
        return new EnergyAmount(output);
    }
}

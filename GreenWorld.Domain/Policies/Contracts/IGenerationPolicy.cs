using GreenWorld.Domain.Models;

namespace GreenWorld.Domain.Policies.Contracts;

/// <summary>Computes how much energy a household generates at a given moment.</summary>
public interface IGenerationPolicy
{
    EnergyAmount GenerationAt(Household household, DateTimeOffset timestamp);
}

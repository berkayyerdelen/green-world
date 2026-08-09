using GreenWorld.Domain.Models;

namespace GreenWorld.Domain.Policies.Contracts;

/// <summary>Computes how much energy a household consumes at a given moment.</summary>
public interface IConsumptionPolicy
{
    EnergyAmount ConsumptionAt(Household household, DateTimeOffset timestamp);
}

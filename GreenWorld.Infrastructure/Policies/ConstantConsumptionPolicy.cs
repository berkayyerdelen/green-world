using GreenWorld.Domain.Models;
using GreenWorld.Domain.Policies.Contracts;

namespace GreenWorld.Infrastructure.Policies;

/// <summary>Placeholder policy: each household consumes a flat rate per tick.</summary>
public sealed class ConstantConsumptionPolicy : IConsumptionPolicy
{
    public EnergyAmount ConsumptionAt(Household household, DateTimeOffset timestamp) => new(0.5);
}

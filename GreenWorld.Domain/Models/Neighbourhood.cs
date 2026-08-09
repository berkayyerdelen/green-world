namespace GreenWorld.Domain.Models;

/// <summary>
/// Aggregate root: a collection of households whose net energy balance is
/// simulated over time.
/// </summary>
public sealed class Neighbourhood
{
    private readonly List<Household> _households = new();

    public Guid Id { get; }
    public string Name { get; }
    public IReadOnlyList<Household> Households => _households;

    public Neighbourhood(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public void AddHousehold(Household household) => _households.Add(household);
}

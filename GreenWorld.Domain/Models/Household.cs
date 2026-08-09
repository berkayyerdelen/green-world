namespace GreenWorld.Domain.Models;

/// <summary>
/// A single household in the neighbourhood. May both consume and (optionally)
/// generate electricity via attached generation sources (e.g. rooftop solar).
/// </summary>
public sealed class Household
{
    public Guid Id { get; }
    public string Name { get; }

    public Household(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}

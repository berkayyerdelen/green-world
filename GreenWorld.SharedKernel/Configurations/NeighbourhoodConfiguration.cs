namespace GreenWorld.SharedKernel.Configurations;

/// <summary>
/// Declarative, reproducible definition of the neighbourhood. A fixed
/// <see cref="Seed"/> plus stated asset proportions fully determines the built
/// neighbourhood. Bind from appsettings/JSON or construct in code; both paths
/// end at the same <see cref="NeighbourhoodConfiguration"/>.
/// </summary>
public sealed class NeighbourhoodConfiguration
{
    public string Name { get; set; } = "GreenWorld";

    /// <summary>Master seed for all deterministic behaviour (assets + weather).</summary>
    public int Seed { get; set; } = 42;

    public int HouseCount { get; set; } = 30;
    public int PublicChargerCount { get; set; } = 6;

    /// <summary>Fraction of houses [0,1] with each optional asset.</summary>
    public double PvShare { get; set; } = 0.40;
    public double HeatPumpShare { get; set; } = 0.30;
    public double HomeEvShare { get; set; } = 0.20;

    // Asset sizing (documented defaults; overridable via config).
    public double PvCapacityKwp { get; set; } = 4.0;
    public double HomeEvPowerKw { get; set; } = 7.4;
    public double PublicChargerPowerKw { get; set; } = 22.0;

    /// <summary>Simulation start (UTC). Defaults to a winter midnight for a clear cold-start.</summary>
    public DateTimeOffset StartUtc { get; set; } = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>Step size in minutes. 60 = 1 hour (default).</summary>
    public int StepMinutes { get; set; } = 60;

    public TimeSpan Step => TimeSpan.FromMinutes(StepMinutes);
}

using GreenWorld.Domain.Models;

namespace GreenWorld.Domain.Services;

/// <summary>What a meter reports for one asset over one interval.</summary>
public readonly record struct MeterReading(FlowDirection Direction, double PowerKw, double EnergyKwh)
{
    public bool IsZero => EnergyKwh <= 0;
}

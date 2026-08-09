namespace GreenWorld.Domain.Models.Storage;

/// <summary>
/// A neighbourhood-scale battery. Holds its rating (capacity, max charge/discharge
/// power, round-trip efficiency) and its live state of charge (SoC). Power is
/// signed from the grid's point of view: <b>positive = discharging</b> (serving
/// load), <b>negative = charging</b> (drawing load). Round-trip losses are split
/// evenly across charge and discharge (one-way efficiency = sqrt(round-trip)).
/// </summary>
public sealed class Battery
{
    public double CapacityKwh { get; }
    public double MaxChargeKw { get; }
    public double MaxDischargeKw { get; }
    public double RoundTripEfficiency { get; }
    public double SocKwh { get; private set; }

    public Battery(double capacityKwh, double maxChargeKw, double maxDischargeKw,
        double roundTripEfficiency, double initialSocKwh)
    {
        if (capacityKwh <= 0) throw new ArgumentOutOfRangeException(nameof(capacityKwh));
        CapacityKwh = capacityKwh;
        MaxChargeKw = Math.Max(0, maxChargeKw);
        MaxDischargeKw = Math.Max(0, maxDischargeKw);
        RoundTripEfficiency = Math.Clamp(roundTripEfficiency, 0.1, 1.0);
        SocKwh = Math.Clamp(initialSocKwh, 0, capacityKwh);
    }

    /// <summary>One-way (charge or discharge) efficiency.</summary>
    public double OneWayEfficiency => Math.Sqrt(RoundTripEfficiency);

    public double SocFraction => CapacityKwh <= 0 ? 0 : SocKwh / CapacityKwh;

    /// <summary>Grid power deliverable by discharging over the interval, limited by SoC.</summary>
    public double DeliverablePowerKw(double hours)
        => hours <= 0 ? 0 : Math.Min(MaxDischargeKw, SocKwh * OneWayEfficiency / hours);

    /// <summary>Grid power absorbable by charging over the interval, limited by headroom.</summary>
    public double AbsorbablePowerKw(double hours)
        => hours <= 0 ? 0 : Math.Min(MaxChargeKw, (CapacityKwh - SocKwh) / (OneWayEfficiency * hours));

    /// <summary>
    /// Apply a signed grid power for <paramref name="hours"/> and update SoC.
    /// Discharging depletes SoC by the energy drawn from storage; charging adds the
    /// energy that actually reaches storage after losses.
    /// </summary>
    public void Apply(double gridPowerKw, double hours)
    {
        if (hours <= 0) return;
        if (gridPowerKw > 0)                 // discharge: grid receives gridPowerKw
            SocKwh -= gridPowerKw * hours / OneWayEfficiency;
        else if (gridPowerKw < 0)            // charge: grid supplies |gridPowerKw|
            SocKwh += -gridPowerKw * hours * OneWayEfficiency;
        SocKwh = Math.Clamp(SocKwh, 0, CapacityKwh);
    }
}

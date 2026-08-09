using GreenWorld.Domain.Models.Storage;

namespace GreenWorld.Domain.Services;

/// <summary>
/// Threshold-based peak-shaving controller. When neighbourhood grid load exceeds
/// the discharge threshold it discharges to pull load back toward the threshold;
/// when load is low (or PV is exporting) below the charge threshold it charges to
/// refill. Returns the signed grid power the battery should apply this interval
/// (+ discharge, − charge), already clamped to power, SoC and headroom limits.
/// </summary>
public sealed class PeakShavingStrategy
{
    public double Decide(Battery battery, double gridLoadKw,
        double dischargeThresholdKw, double chargeThresholdKw, double hours)
    {
        if (gridLoadKw > dischargeThresholdKw)
        {
            var want = gridLoadKw - dischargeThresholdKw;
            return Math.Max(0, Math.Min(want, battery.DeliverablePowerKw(hours)));
        }

        if (gridLoadKw < chargeThresholdKw)
        {
            var want = chargeThresholdKw - gridLoadKw;
            return -Math.Max(0, Math.Min(want, battery.AbsorbablePowerKw(hours)));
        }

        return 0; // within the dead-band: idle
    }
}

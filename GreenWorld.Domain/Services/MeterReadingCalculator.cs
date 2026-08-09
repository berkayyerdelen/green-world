using GreenWorld.Domain.Common;
using GreenWorld.Domain.Models;
using GreenWorld.Domain.Models.Assets;

namespace GreenWorld.Domain.Services;

/// <summary>
/// Pure domain service that turns an asset + weather/time context into the meter
/// reading for one interval. This is the physics of the simulation, kept in the
/// domain and switched on <see cref="AssetKind"/> so assets stay simple data.
/// Deterministic: identical inputs always yield the same reading.
/// </summary>
public sealed class MeterReadingCalculator
{
    // Residential base-load daily curve (kW), index = hour of day.
    private static readonly double[] BaseProfile =
    {
        0.20,0.18,0.17,0.17,0.18,0.25,0.45,0.65,0.55,0.40,0.35,0.35,
        0.38,0.35,0.33,0.35,0.45,0.70,0.95,0.90,0.75,0.60,0.40,0.28
    };

    // Public charger occupancy fraction by hour.
    private static readonly double[] PublicOccupancy =
    {
        0.05,0.04,0.03,0.03,0.03,0.05,0.10,0.20,0.30,0.35,0.40,0.45,
        0.50,0.45,0.40,0.42,0.50,0.65,0.70,0.60,0.45,0.30,0.18,0.10
    };

    public MeterReading Read(Asset asset, SimulationContext ctx)
    {
        var powerKw = asset.Kind switch
        {
            AssetKind.BaseLoad        => BaseLoad(asset, ctx),
            AssetKind.HeatPump        => HeatPump(ctx),
            AssetKind.Pv              => Pv(asset, ctx),
            AssetKind.HomeEvCharger   => HomeEv(asset, ctx),
            AssetKind.PublicEvCharger => PublicEv(asset, ctx),
            _ => 0.0
        };
        powerKw = Math.Max(0, powerKw);
        return new MeterReading(asset.Direction, powerKw, powerKw * ctx.StepHours);
    }

    private static double BaseLoad(Asset a, SimulationContext ctx)
    {
        var seasonal = ctx.Season == Season.Winter ? 1.15 : ctx.Season == Season.Summer ? 0.95 : 1.0;
        var jitter = 0.9 + 0.2 * Deterministic.Unit(a.Seed, ctx.DayOfYear, ctx.Hour);
        return BaseProfile[ctx.Hour] * a.ScaleFactor * seasonal * jitter;
    }

    private static double HeatPump(SimulationContext ctx)
    {
        const double setpoint = 20.0;
        var deltaT = setpoint - ctx.Weather.TemperatureCelsius;
        if (deltaT <= 0) return 0;                      // no active cooling modelled
        var thermalKw = deltaT * 0.18;                  // building heat-loss coefficient
        var cop = Math.Clamp(2.0 + 0.11 * ctx.Weather.TemperatureCelsius, 1.6, 4.5);
        return Math.Min(thermalKw / cop, 3.0);          // capped compressor draw
    }

    private static double Pv(Asset a, SimulationContext ctx)
        => a.CapacityKwp * ctx.Weather.IrradianceFactor;

    private static double HomeEv(Asset a, SimulationContext ctx)
    {
        const int startHour = 22;
        var power = a.RatedPowerKw <= 0 ? 7.4 : a.RatedPowerKw;
        var sessionDay = ctx.Hour >= startHour ? ctx.DayOfYear : ctx.DayOfYear - 1;
        if (!Deterministic.Chance(a.Seed, 0.6, sessionDay, 1)) return 0;
        var needKwh = Deterministic.Range(a.Seed, 6, 30, sessionDay, 2);
        var hoursNeeded = (int)Math.Ceiling(needKwh / power);
        var hoursSinceStart = ctx.Hour >= startHour ? ctx.Hour - startHour : ctx.Hour + (24 - startHour);
        return hoursSinceStart < hoursNeeded ? power : 0;
    }

    private static double PublicEv(Asset a, SimulationContext ctx)
    {
        var power = a.RatedPowerKw <= 0 ? 22.0 : a.RatedPowerKw;
        var weekend = ctx.Timestamp.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        var weekdayFactor = weekend ? 0.75 : 1.0;
        var jitter = 0.85 + 0.3 * Deterministic.Unit(a.Seed, ctx.DayOfYear, ctx.Hour);
        var occ = Math.Clamp(PublicOccupancy[ctx.Hour] * weekdayFactor * jitter, 0, 1);
        return power * occ;
    }
}

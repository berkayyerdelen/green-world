using GreenWorld.Domain.Common;
using GreenWorld.Domain.Models;
using GreenWorld.Domain.Policies.Contracts;

namespace GreenWorld.Infrastructure.Weather;

/// <summary>
/// Deterministic, privacy-friendly weather. Pure function of (seed, timestamp):
/// no external APIs. Season sets baseline temperature, day length and cloudiness;
/// a diurnal sine adds the day/night swing; a per-day deterministic draw adds
/// weather variability. Irradiance combines a clear-sky solar curve (whose window
/// widens in summer, narrows in winter) with the day's cloud cover.
/// </summary>
public sealed class SeasonalWeatherModel : IWeatherModel
{
    private readonly int _seed;

    public SeasonalWeatherModel(int seed) => _seed = seed;

    public Domain.Models.Weather WeatherAt(DateTimeOffset t)
    {
        var season = t.SeasonOf();
        var doy = t.DayOfYear;
        var hourFrac = t.Hour + t.Minute / 60.0;

        // --- Temperature (deg C) ---
        var (meanC, dayLength) = season switch
        {
            Season.Winter => (3.0, 8.5),
            Season.Spring => (12.0, 12.5),
            Season.Summer => (22.0, 16.0),
            _             => (11.0, 11.0) // Autumn
        };
        // Daily weather offset (+/- 4 C), stable within a day.
        var dailyOffset = Deterministic.Range(_seed, -4, 4, doy, 100);
        // Diurnal swing: coldest ~05:00, warmest ~15:00.
        var diurnal = 5.0 * Math.Sin((hourFrac - 9.0) / 24.0 * 2 * Math.PI);
        var temperature = meanC + dailyOffset + diurnal;

        // --- Cloud cover [0,1] ---
        var cloudBase = season switch
        {
            Season.Winter => 0.65,
            Season.Autumn => 0.60,
            Season.Spring => 0.45,
            _             => 0.35 // Summer
        };
        var cloud = Math.Clamp(cloudBase + Deterministic.Range(_seed, -0.3, 0.3, doy, 200), 0, 1);

        // --- Irradiance factor [0,1] ---
        var sunrise = 12.0 - dayLength / 2.0;
        var sunset = 12.0 + dayLength / 2.0;
        double clearSky = 0;
        if (hourFrac > sunrise && hourFrac < sunset)
            clearSky = Math.Sin(Math.PI * (hourFrac - sunrise) / (sunset - sunrise));
        var seasonalPeak = season == Season.Summer ? 1.0 : season == Season.Winter ? 0.55 : 0.8;
        var irradiance = Math.Clamp(clearSky * seasonalPeak * (1 - 0.75 * cloud), 0, 1);

        return new Domain.Models.Weather(temperature, cloud, irradiance);
    }
}

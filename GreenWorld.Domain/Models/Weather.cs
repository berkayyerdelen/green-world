namespace GreenWorld.Domain.Models;

/// <summary>
/// Deterministic weather snapshot for a single simulated moment.
/// <para><b>TemperatureCelsius</b> drives heat-pump demand.</para>
/// <para><b>CloudCover</b> (0 clear .. 1 overcast) and <b>IrradianceFactor</b>
/// (0 dark .. 1 full sun) drive PV generation.</para>
/// </summary>
public readonly record struct Weather(
    double TemperatureCelsius,
    double CloudCover,
    double IrradianceFactor);

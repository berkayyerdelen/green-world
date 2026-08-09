using FluentAssertions;
using GreenWorld.Domain.Models;
using GreenWorld.Domain.Models.Assets;
using GreenWorld.Domain.Services;
using Xunit;

namespace GreenWorld.Domain.Tests;

public class MeterReadingCalculatorTests
{
    private readonly MeterReadingCalculator _calc = new();

    private static SimulationContext Ctx(double temp, double irr, int hour = 12, Season season = Season.Winter)
        => new(new DateTimeOffset(2025, 1, 15, hour, 0, 0, TimeSpan.Zero), season,
               new Weather(temp, 0.2, irr), TimeSpan.FromHours(1));

    private static Asset Pv(double kwp) => new(
        Guid.NewGuid(), Guid.NewGuid(), "pv", AssetKind.Pv, FlowDirection.Generation, 1, capacityKwp: kwp);

    private static Asset HeatPump() => new(
        Guid.NewGuid(), Guid.NewGuid(), "hp", AssetKind.HeatPump, FlowDirection.Consumption, 1, ratedPowerKw: 3);

    [Fact]
    public void Pv_Scales_With_Irradiance_And_Is_Zero_At_Night()
    {
        _calc.Read(Pv(4), Ctx(5, 0.5)).PowerKw.Should().BeApproximately(2.0, 1e-9);
        _calc.Read(Pv(4), Ctx(5, 0.0)).IsZero.Should().BeTrue();
    }

    [Fact]
    public void HeatPump_Draws_More_When_Colder()
    {
        var cold = _calc.Read(HeatPump(), Ctx(-5, 0)).PowerKw;
        var mild = _calc.Read(HeatPump(), Ctx(15, 0)).PowerKw;
        cold.Should().BeGreaterThan(mild);
    }

    [Fact]
    public void Energy_Is_Power_Times_Step_Hours()
    {
        var r = _calc.Read(Pv(4), Ctx(5, 0.5));
        r.EnergyKwh.Should().BeApproximately(r.PowerKw * 1.0, 1e-9);
    }
}

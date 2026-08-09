using FluentAssertions;
using GreenWorld.Domain.Models.Storage;
using GreenWorld.Domain.Services;
using Xunit;

namespace GreenWorld.Domain.Tests;

public class PeakShavingTests
{
    private static Battery FullBattery() =>
        new(capacityKwh: 100, maxChargeKw: 50, maxDischargeKw: 50,
            roundTripEfficiency: 1.0, initialSocKwh: 100);

    private readonly PeakShavingStrategy _strategy = new();

    [Fact]
    public void Discharges_To_Pull_Peak_Down_To_Threshold()
    {
        var b = FullBattery();
        var power = _strategy.Decide(b, gridLoadKw: 80, dischargeThresholdKw: 50, chargeThresholdKw: 20, hours: 1);
        power.Should().BeApproximately(30, 1e-9); // 80 - 50, within 50 kW / SoC limits
    }

    [Fact]
    public void Discharge_Is_Capped_By_Max_Power()
    {
        var b = FullBattery();
        var power = _strategy.Decide(b, gridLoadKw: 200, dischargeThresholdKw: 50, chargeThresholdKw: 20, hours: 1);
        power.Should().Be(50); // capped at MaxDischargeKw
    }

    [Fact]
    public void Charges_When_Load_Below_Charge_Threshold()
    {
        var b = new Battery(100, 50, 50, 1.0, initialSocKwh: 10);
        var power = _strategy.Decide(b, gridLoadKw: 5, dischargeThresholdKw: 50, chargeThresholdKw: 20, hours: 1);
        power.Should().BeApproximately(-15, 1e-9); // charge to lift load toward 20 kW
    }

    [Fact]
    public void Idle_Within_Dead_Band()
    {
        var b = FullBattery();
        _strategy.Decide(b, gridLoadKw: 35, dischargeThresholdKw: 50, chargeThresholdKw: 20, hours: 1)
            .Should().Be(0);
    }

    [Fact]
    public void Empty_Battery_Cannot_Discharge()
    {
        var b = new Battery(100, 50, 50, 1.0, initialSocKwh: 0);
        _strategy.Decide(b, gridLoadKw: 80, dischargeThresholdKw: 50, chargeThresholdKw: 20, hours: 1)
            .Should().Be(0);
    }

    [Fact]
    public void Apply_Depletes_And_Refills_Soc()
    {
        var b = new Battery(100, 50, 50, 1.0, initialSocKwh: 50);
        b.Apply(10, 1); b.SocKwh.Should().BeApproximately(40, 1e-9);   // discharge 10 kWh
        b.Apply(-20, 1); b.SocKwh.Should().BeApproximately(60, 1e-9);  // charge 20 kWh
    }

    [Fact]
    public void RoundTrip_Efficiency_Loses_Energy()
    {
        var b = new Battery(100, 50, 50, roundTripEfficiency: 0.81, initialSocKwh: 50); // one-way 0.9
        b.Apply(-10, 1);                       // grid gives 10 kWh -> stores 9 kWh
        b.SocKwh.Should().BeApproximately(59, 1e-9);
    }
}

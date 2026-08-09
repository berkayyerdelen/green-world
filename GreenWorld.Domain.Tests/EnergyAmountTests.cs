using FluentAssertions;
using GreenWorld.Domain.Models;
using Xunit;

namespace GreenWorld.Domain.Tests;

public class EnergyAmountTests
{
    [Fact]
    public void Net_Is_Generation_Minus_Consumption()
    {
        var tick = new SimulationTick(DateTimeOffset.UnixEpoch, new EnergyAmount(2), new EnergyAmount(5));
        tick.Net.Kilowatthours.Should().Be(3);
    }
}

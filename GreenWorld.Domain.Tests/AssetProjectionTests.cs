using FluentAssertions;
using GreenWorld.Domain.Models;
using GreenWorld.Domain.Models.Assets;
using GreenWorld.Domain.Models.Events;
using Xunit;

namespace GreenWorld.Domain.Tests;

public class AssetProjectionTests
{
    private static Asset NewAsset(FlowDirection dir) =>
        new(Guid.NewGuid(), Guid.NewGuid(), "a", AssetKind.BaseLoad, dir, 1);

    [Fact]
    public void ApplyReading_Accrues_Cumulative_Energy()
    {
        var asset = NewAsset(FlowDirection.Consumption);
        asset.ApplyReading(new MeterReadingEvent(Guid.NewGuid(), asset.Id,
            DateTimeOffset.UnixEpoch, 1.5, 1.5, FlowDirection.Consumption, DateTimeOffset.UtcNow));
        asset.ApplyReading(new MeterReadingEvent(Guid.NewGuid(), asset.Id,
            DateTimeOffset.UnixEpoch.AddHours(1), 2.0, 2.0, FlowDirection.Consumption, DateTimeOffset.UtcNow));

        asset.CumulativeConsumedKwh.Should().Be(3.5);
        asset.CumulativeGeneratedKwh.Should().Be(0);
        asset.LastReadingAt.Should().Be(DateTimeOffset.UnixEpoch.AddHours(1));
    }

    [Fact]
    public void ApplyReading_Rejects_Foreign_Asset()
    {
        var asset = NewAsset(FlowDirection.Generation);
        var foreign = new MeterReadingEvent(Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UnixEpoch, 1, 1, FlowDirection.Generation, DateTimeOffset.UtcNow);
        var act = () => asset.ApplyReading(foreign);
        act.Should().Throw<InvalidOperationException>();
    }
}

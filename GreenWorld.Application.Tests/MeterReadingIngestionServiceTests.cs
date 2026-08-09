using FluentAssertions;
using GreenWorld.Application.Messaging;
using GreenWorld.Application.Services;
using GreenWorld.Domain.Models;
using GreenWorld.Domain.Models.Assets;
using GreenWorld.Domain.Models.Events;
using GreenWorld.Domain.Repositories;
using Xunit;

namespace GreenWorld.Application.Tests;

public class MeterReadingIngestionServiceTests
{
    private sealed class FakeAssetRepo : IAssetRepository
    {
        public required Asset Asset;
        public int Saves;
        public Task<Asset?> GetAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Asset?>(Asset);
        public Task SaveChangesAsync(CancellationToken ct = default) { Saves++; return Task.CompletedTask; }
    }

    private sealed class FakeEventStore : IMeterReadingEventStore
    {
        public readonly List<MeterReadingEvent> Events = new();
        public Task AppendAsync(MeterReadingEvent r, CancellationToken ct = default) { Events.Add(r); return Task.CompletedTask; }
        public Task<IReadOnlyList<MeterReadingEvent>> GetForAssetAsync(Guid a, DateTimeOffset? f = null, DateTimeOffset? t = null, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<MeterReadingEvent>>(Events);
    }

    [Fact]
    public async Task Ingest_Appends_Event_And_Updates_Projection()
    {
        var asset = new Asset(Guid.NewGuid(), Guid.NewGuid(), "pv", AssetKind.Pv, FlowDirection.Generation, 1);
        var repo = new FakeAssetRepo { Asset = asset };
        var store = new FakeEventStore();
        var svc = new MeterReadingIngestionService(repo, store);

        await svc.IngestAsync(new MeterReadingMessage(
            Guid.NewGuid(), asset.Id, DateTimeOffset.UnixEpoch, 2.5, 2.5, FlowDirection.Generation));

        store.Events.Should().HaveCount(1);
        asset.CumulativeGeneratedKwh.Should().Be(2.5);
        repo.Saves.Should().Be(1);
    }
}

using GreenWorld.Application.Contracts;
using GreenWorld.Application.Messaging;
using GreenWorld.Domain.Models;
using GreenWorld.Domain.Repositories;
using GreenWorld.Domain.Services;
using GreenWorld.SharedKernel.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GreenWorld.Infrastructure.Simulation;

/// <summary>
/// Background "meter farm". Drives the simulation clock and, each tick, computes
/// every asset's reading from weather + time and <b>publishes it to RabbitMQ</b>
/// (via MassTransit) exactly as real meters would. It also writes a neighbourhood
/// aggregate snapshot per tick so aggregate power/energy over time is queryable.
/// The consumer independently folds published readings into asset projections.
/// </summary>
public sealed class MeterSimulatorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMeterReadingPublisher _publisher;
    private readonly IWeatherModel _weather;
    private readonly MeterReadingCalculator _calculator;
    private readonly NeighbourhoodConfiguration _config;
    private readonly SimulatorOptions _options;
    private readonly ILogger<MeterSimulatorService> _logger;

    public MeterSimulatorService(
        IServiceScopeFactory scopeFactory,
        IMeterReadingPublisher publisher,
        IWeatherModel weather,
        MeterReadingCalculator calculator,
        NeighbourhoodConfiguration config,
        IOptions<SimulatorOptions> options,
        ILogger<MeterSimulatorService> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _weather = weather;
        _calculator = calculator;
        _config = config;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Meter simulator disabled.");
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(_options.StartupDelaySeconds), stoppingToken);

        var neighbourhood = await LoadNeighbourhoodAsync(stoppingToken);
        if (neighbourhood is null)
        {
            _logger.LogWarning("No neighbourhood found; simulator not starting.");
            return;
        }

        var assets = neighbourhood.AllAssets().ToList();
        var clock = new SimulationClock(_config.StartUtc, _config.Step);
        double cumulativeConsumed = 0, cumulativeGenerated = 0;
        var step = 0;

        _logger.LogInformation("Meter simulator started: {Assets} assets, step {Step}.",
            assets.Count, _config.Step);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = clock.Current;
            var ctx = new SimulationContext(now, now.SeasonOf(), _weather.WeatherAt(now), _config.Step);
            double tickConsumptionKw = 0, tickGenerationKw = 0;

            foreach (var asset in assets)
            {
                var reading = _calculator.Read(asset, ctx);
                if (reading.IsZero) continue;

                if (reading.Direction == FlowDirection.Generation) tickGenerationKw += reading.PowerKw;
                else tickConsumptionKw += reading.PowerKw;

                await _publisher.PublishAsync(new MeterReadingMessage(
                    Guid.NewGuid(), asset.Id, now, reading.EnergyKwh, reading.PowerKw, reading.Direction),
                    stoppingToken);
            }

            cumulativeConsumed += tickConsumptionKw * ctx.StepHours;
            cumulativeGenerated += tickGenerationKw * ctx.StepHours;
            await WriteSnapshotAsync(new NeighbourhoodAggregateSnapshot(
                Guid.NewGuid(), neighbourhood.Id, now,
                tickConsumptionKw, tickGenerationKw, cumulativeConsumed, cumulativeGenerated),
                stoppingToken);

            clock.Advance();
            step++;
            if (_options.MaxSteps > 0 && step >= _options.MaxSteps)
            {
                _logger.LogInformation("Meter simulator reached MaxSteps ({Max}).", _options.MaxSteps);
                break;
            }

            await Task.Delay(_options.StepDelayMs, stoppingToken);
        }
    }

    private async Task<Neighbourhood?> LoadNeighbourhoodAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<INeighbourhoodRepository>();
        return await repo.GetGraphAsync(ct);
    }

    private async Task WriteSnapshotAsync(NeighbourhoodAggregateSnapshot snapshot, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<INeighbourhoodAggregateStore>();
        await store.AddAsync(snapshot, ct);
    }
}

using GreenWorld.Application.Contracts;
using GreenWorld.Domain.Policies.Contracts;
using GreenWorld.Domain.Repositories;
using GreenWorld.Domain.Services;
using GreenWorld.Infrastructure.Configuration;
using GreenWorld.Infrastructure.Messaging;
using GreenWorld.Infrastructure.Persistence;
using GreenWorld.Infrastructure.Repositories;
using GreenWorld.Infrastructure.Simulation;
using GreenWorld.Infrastructure.Weather;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GreenWorld.Infrastructure;

/// <summary>
/// Wires Infrastructure: configuration, EF Core (PostgreSQL), repositories/event
/// store, deterministic weather + calculator, MassTransit (RabbitMQ) publisher &
/// consumer, and the background meter simulator.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddGreenWorldInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Neighbourhood definition: code defaults + optional JSON override.
        var neighbourhoodConfig = NeighbourhoodConfigurationLoader.Load();
        services.AddSingleton(neighbourhoodConfig);

        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<SimulatorOptions>(configuration.GetSection(SimulatorOptions.SectionName));

        // EF Core + PostgreSQL.
        services.AddDbContext<GreenWorldDbContext>(o =>
            o.UseNpgsql(configuration.GetConnectionString("Postgres"))
             // Initial migration + snapshot were authored by hand; suppress EF 10's
             // strict snapshot-vs-model check so Migrate() applies the migration.
             .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

        // Repositories / stores.
        services.AddScoped<INeighbourhoodRepository, EfNeighbourhoodRepository>();
        services.AddScoped<IAssetRepository, EfAssetRepository>();
        services.AddScoped<IMeterReadingEventStore, EfMeterReadingEventStore>();
        services.AddScoped<INeighbourhoodAggregateStore, EfNeighbourhoodAggregateStore>();
        services.AddScoped<DatabaseInitializer>();

        // Deterministic domain services.
        services.AddSingleton<MeterReadingCalculator>();
        services.AddSingleton<IWeatherModel>(_ => new SeasonalWeatherModel(neighbourhoodConfig.Seed));

        // Messaging: MassTransit over RabbitMQ.
        services.AddSingleton<IMeterReadingPublisher, MassTransitMeterReadingPublisher>();
        services.AddMassTransit(x =>
        {
            x.AddConsumer<MeterReadingConsumer>();
            x.UsingRabbitMq((ctx, cfg) =>
            {
                var opt = ctx.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                cfg.Host(opt.HostName, (ushort)opt.Port, opt.VirtualHost, h =>
                {
                    h.Username(opt.UserName);
                    h.Password(opt.Password);
                });
                cfg.ConfigureEndpoints(ctx);
            });
        });

        // Background meter farm that publishes readings.
        services.AddHostedService<MeterSimulatorService>();

        return services;
    }
}

using GreenWorld.Application.Contracts;
using GreenWorld.Application.Services;
using GreenWorld.Infrastructure;

namespace GreenWorld.Api.Extensions;

/// <summary>
/// Composition root. Registers Application use-case services and delegates the
/// heavy wiring (EF Core, MassTransit/RabbitMQ, simulator) to Infrastructure.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGreenWorld(this IServiceCollection services, IConfiguration configuration)
    {
        // Application use cases.
        services.AddScoped<IMeterReadingIngestionService, MeterReadingIngestionService>();
        services.AddScoped<INeighbourhoodQueryService, NeighbourhoodQueryService>();

        // Infrastructure: persistence + messaging + simulator.
        services.AddGreenWorldInfrastructure(configuration);

        return services;
    }
}

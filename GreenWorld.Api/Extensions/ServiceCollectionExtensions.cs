using GreenWorld.Application.Contracts;
using GreenWorld.Application.Services;
using GreenWorld.Domain.Policies.Contracts;
using GreenWorld.Domain.Repositories;
using GreenWorld.Infrastructure.Persistence;
using GreenWorld.Infrastructure.Policies;
using GreenWorld.Infrastructure.Repositories;

namespace GreenWorld.Api.Extensions;

/// <summary>Composition root: wires interfaces to implementations per the dependency rule.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGreenWorld(this IServiceCollection services)
    {
        services.AddSingleton<ApplicationContext>();
        services.AddScoped<INeighbourhoodRepository, NeighbourhoodRepository>();

        services.AddScoped<IConsumptionPolicy, ConstantConsumptionPolicy>();
        services.AddScoped<IGenerationPolicy, DaylightGenerationPolicy>();

        services.AddScoped<ISimulationService, SimulationService>();

        return services;
    }
}

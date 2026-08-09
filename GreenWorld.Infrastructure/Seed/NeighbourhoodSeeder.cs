using GreenWorld.Domain.Common;
using GreenWorld.Domain.Models;
using GreenWorld.Domain.Models.Assets;
using GreenWorld.Domain.Models.Sites;
using GreenWorld.SharedKernel.Configurations;

namespace GreenWorld.Infrastructure.Seed;

/// <summary>
/// Deterministically builds the neighbourhood graph from configuration: exactly
/// HouseCount households (with a base load plus seeded PV / heat-pump / home-EV
/// assets against the stated shares) and exactly PublicChargerCount public
/// facilities, each holding one public EV charger asset.
/// </summary>
public sealed class NeighbourhoodSeeder
{
    public Neighbourhood Build(NeighbourhoodConfiguration cfg)
    {
        var neighbourhoodId = Guid(cfg.Seed, "neighbourhood", 0);
        var neighbourhood = new Neighbourhood(neighbourhoodId, cfg.Name, cfg.StartUtc);

        for (var i = 0; i < cfg.HouseCount; i++)
            neighbourhood.AddSite(BuildHousehold(cfg, neighbourhoodId, i));

        for (var i = 0; i < cfg.PublicChargerCount; i++)
            neighbourhood.AddSite(BuildPublicFacility(cfg, neighbourhoodId, i));

        return neighbourhood;
    }

    private static Household BuildHousehold(NeighbourhoodConfiguration cfg, Guid nId, int i)
    {
        var seed = cfg.Seed ^ (i + 1);
        var siteId = Guid(cfg.Seed, "house", i);
        var name = $"House {i + 1:00}";
        var house = new Household(siteId, nId, name);

        var scale = Deterministic.Range(seed, 0.8, 1.2, i, 0);
        house.AddAsset(new Asset(Guid(cfg.Seed, "base", i), siteId, $"{name} Base Load",
            AssetKind.BaseLoad, FlowDirection.Consumption, seed, scaleFactor: scale));

        if (Deterministic.Chance(seed, cfg.PvShare, i, 10))
            house.AddAsset(new Asset(Guid(cfg.Seed, "pv", i), siteId, $"{name} PV",
                AssetKind.Pv, FlowDirection.Generation, seed,
                capacityKwp: cfg.PvCapacityKwp * Deterministic.Range(seed, 0.75, 1.25, i, 11)));

        if (Deterministic.Chance(seed, cfg.HeatPumpShare, i, 20))
            house.AddAsset(new Asset(Guid(cfg.Seed, "hp", i), siteId, $"{name} Heat Pump",
                AssetKind.HeatPump, FlowDirection.Consumption, seed, ratedPowerKw: 3.0));

        if (Deterministic.Chance(seed, cfg.HomeEvShare, i, 30))
            house.AddAsset(new Asset(Guid(cfg.Seed, "ev", i), siteId, $"{name} EV Charger",
                AssetKind.HomeEvCharger, FlowDirection.Consumption, seed, ratedPowerKw: cfg.HomeEvPowerKw));

        return house;
    }

    private static PublicFacility BuildPublicFacility(NeighbourhoodConfiguration cfg, Guid nId, int i)
    {
        var seed = cfg.Seed ^ (0x5000 + i);
        var siteId = Guid(cfg.Seed, "facility", i);
        var name = $"Public Charge Point {i + 1}";
        var facility = new PublicFacility(siteId, nId, name);
        facility.AddAsset(new Asset(Guid(cfg.Seed, "public", i), siteId, $"{name} Charger",
            AssetKind.PublicEvCharger, FlowDirection.Consumption, seed,
            ratedPowerKw: cfg.PublicChargerPowerKw));
        return facility;
    }

    private static Guid Guid(int seed, string role, int index)
    {
        var bytes = new byte[16];
        var h = unchecked((uint)(seed * 397) ^ StableHash(role) ^ (uint)(index * 40503));
        for (var i = 0; i < 16; i++)
        {
            h ^= h << 13; h ^= h >> 17; h ^= h << 5;
            bytes[i] = (byte)(h & 0xFF);
        }
        return new System.Guid(bytes);
    }

    private static uint StableHash(string s)
    {
        unchecked
        {
            uint h = 2166136261u;
            foreach (var ch in s) h = (h ^ ch) * 16777619u;
            return h;
        }
    }
}

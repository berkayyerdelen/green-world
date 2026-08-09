namespace GreenWorld.Domain.Common;

/// <summary>
/// Small deterministic hash utility. Given a seed and a set of integer
/// coordinates (e.g. day-of-year, hour, asset index) it returns a stable
/// pseudo-random value in [0,1). This keeps the whole simulation reproducible:
/// the same seed and simulated time always yield the same weather and behaviour.
/// </summary>
public static class Deterministic
{
    public static double Unit(int seed, params int[] coords)
    {
        unchecked
        {
            uint h = 2166136261u ^ (uint)seed;
            foreach (var c in coords)
            {
                h = (h ^ (uint)c) * 16777619u;
                h ^= h >> 13;
                h *= 2654435761u;
                h ^= h >> 16;
            }
            return (h & 0xFFFFFF) / (double)0x1000000; // 24-bit mantissa -> [0,1)
        }
    }

    /// <summary>Deterministic value in [min,max).</summary>
    public static double Range(int seed, double min, double max, params int[] coords)
        => min + (max - min) * Unit(seed, coords);

    /// <summary>Deterministic true with the given probability.</summary>
    public static bool Chance(int seed, double probability, params int[] coords)
        => Unit(seed, coords) < probability;
}

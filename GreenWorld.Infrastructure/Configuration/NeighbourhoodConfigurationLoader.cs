using System.Text.Json;
using GreenWorld.SharedKernel.Configurations;

namespace GreenWorld.Infrastructure.Configuration;

/// <summary>
/// Resolves the neighbourhood configuration. Starts from the code-based defaults
/// in <see cref="NeighbourhoodConfiguration"/> and, if a JSON file is present,
/// overlays it — giving "code default + optional JSON override" in one place.
/// </summary>
public static class NeighbourhoodConfigurationLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>Code defaults, optionally overridden by a JSON file if it exists.</summary>
    public static NeighbourhoodConfiguration Load(string? jsonPath = null)
    {
        jsonPath ??= Path.Combine(AppContext.BaseDirectory, "Configuration", "neighbourhood.json");
        if (!File.Exists(jsonPath))
            return new NeighbourhoodConfiguration();

        var json = File.ReadAllText(jsonPath);
        return JsonSerializer.Deserialize<NeighbourhoodConfiguration>(json, Options)
               ?? new NeighbourhoodConfiguration();
    }
}

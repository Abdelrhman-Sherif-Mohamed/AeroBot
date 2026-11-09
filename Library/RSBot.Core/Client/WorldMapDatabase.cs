using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using RSBot.Core.Objects;

namespace RSBot.Core.Client;

public class WorldTown
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("region")]
    public ushort Region { get; set; }

    [JsonPropertyName("posX")]
    public double PosX { get; set; }

    [JsonPropertyName("posY")]
    public double PosY { get; set; }

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("z")]
    public double Z { get; set; }

    public Position ToPosition() => new(Region, (float)X, (float)Y, (float)Z);
}

public class TeleportLink
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("region")]
    public int Region { get; set; }

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("z")]
    public double Z { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }

    public ushort ActualRegion => (ushort)(Region < 0 ? Region + 65536 : Region);
    public Position ToPosition() => new(ActualRegion, (float)X, (float)Y, (float)Z);
}

public class WorldNpc
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("region")]
    public int Region { get; set; }

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("z")]
    public double Z { get; set; }

    [JsonPropertyName("teleport")]
    public List<TeleportLink> Teleport { get; set; } = new();

    public ushort ActualRegion => (ushort)(Region < 0 ? Region + 65536 : Region);
    public Position ToPosition() => new(ActualRegion, (float)X, (float)Y, (float)Z);
}

public class WorldTeleport
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("region")]
    public int Region { get; set; }

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("z")]
    public double Z { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("teleport")]
    public List<TeleportLink> Teleport { get; set; } = new();

    public ushort ActualRegion => (ushort)(Region < 0 ? Region + 65536 : Region);
    public Position ToPosition() => new(ActualRegion, (float)X, (float)Y, (float)Z);
}

public class WorldMapDataset
{
    [JsonPropertyName("towns")]
    public List<WorldTown> Towns { get; set; } = new();

    [JsonPropertyName("npcs")]
    public List<WorldNpc> Npcs { get; set; } = new();

    [JsonPropertyName("teleports")]
    public List<WorldTeleport> Teleports { get; set; } = new();
}

/// <summary>
/// Provides complete knowledge of the Silkroad Online game world (NPCs, Teleports, Towns, and coordinates).
/// </summary>
public class WorldMapDatabase
{
    private static readonly Lazy<WorldMapDatabase> _instance = new(() => new WorldMapDatabase());
    public static WorldMapDatabase Instance => _instance.Value;

    private readonly List<WorldTown> _towns = new();
    private readonly List<WorldNpc> _npcs = new();
    private readonly List<WorldTeleport> _teleports = new();
    private bool _loaded;

    public IReadOnlyList<WorldTown> Towns { get { EnsureLoaded(); return _towns; } }
    public IReadOnlyList<WorldNpc> NPCs { get { EnsureLoaded(); return _npcs; } }
    public IReadOnlyList<WorldTeleport> Teleports { get { EnsureLoaded(); return _teleports; } }

    public WorldMapDatabase()
    {
    }

    public void EnsureLoaded()
    {
        if (_loaded) return;
        lock (_towns)
        {
            if (_loaded) return;
            Load();
        }
    }

    public void Load(string customPath = null)
    {
        try
        {
            var path = customPath;
            if (string.IsNullOrEmpty(path))
            {
                var candidate = Path.Combine(Application.StartupPath, "Data", "xSROMap", "world_map_data.json");
                if (File.Exists(candidate))
                {
                    path = candidate;
                }
                else
                {
                    // Fallback to relative or current directory
                    var alt = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "xSROMap", "world_map_data.json");
                    if (File.Exists(alt)) path = alt;
                }
            }

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Log.Notify($"[WorldMap] Data file not found at {path}");
                return;
            }

            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<WorldMapDataset>(json);
            if (data != null)
            {
                _towns.Clear();
                _npcs.Clear();
                _teleports.Clear();

                if (data.Towns != null) _towns.AddRange(data.Towns);
                if (data.Npcs != null) _npcs.AddRange(data.Npcs);
                if (data.Teleports != null) _teleports.AddRange(data.Teleports);

                _loaded = true;
                Log.Notify($"[WorldMap] Loaded {_towns.Count} towns, {_npcs.Count} NPCs, and {_teleports.Count} teleports.");
            }
        }
        catch (Exception ex)
        {
            Log.Notify($"[WorldMap] Failed to load world map data: {ex.Message}");
        }
    }

    /// <summary>
    /// Finds an NPC by exact or case-insensitive name match.
    /// </summary>
    public WorldNpc FindNpc(string name)
    {
        EnsureLoaded();
        if (string.IsNullOrWhiteSpace(name)) return null;

        return _npcs.FirstOrDefault(n => string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase))
            ?? _npcs.FirstOrDefault(n => n.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    /// <summary>
    /// Searches NPCs by query substring.
    /// </summary>
    public List<WorldNpc> SearchNpcs(string query)
    {
        EnsureLoaded();
        if (string.IsNullOrWhiteSpace(query)) return new List<WorldNpc>();

        return _npcs.Where(n => n.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
    }

    /// <summary>
    /// Finds a town by name.
    /// </summary>
    public WorldTown GetTown(string name)
    {
        EnsureLoaded();
        if (string.IsNullOrWhiteSpace(name)) return null;

        return _towns.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase))
            ?? _towns.FirstOrDefault(t => t.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    /// <summary>
    /// Finds the closest town to a given game position.
    /// </summary>
    public WorldTown GetClosestTown(Position pos)
    {
        EnsureLoaded();
        if (_towns.Count == 0) return null;

        WorldTown best = null;
        double bestDist = double.MaxValue;

        foreach (var town in _towns)
        {
            var townPos = town.ToPosition();
            var d = pos.DistanceTo(townPos);
            if (d < bestDist)
            {
                bestDist = d;
                best = town;
            }
        }

        return best;
    }

    /// <summary>
    /// Converts in-game coordinates (PosX, PosY) to internal Silkroad coordinates (X, Y, Z, Region).
    /// </summary>
    public static (double x, double y, ushort region) GameToSRO(double posX, double posY)
    {
        var x = Math.Round(Math.Abs(posX) % 192.0 * 10.0);
        if (posX < 0.0) x = 1920 - x;

        var y = Math.Round(Math.Abs(posY) % 192.0 * 10.0);
        if (posY < 0.0) y = 1920 - y;

        var xSector = (int)Math.Round((posX - x / 10.0) / 192.0 + 135);
        var ySector = (int)Math.Round((posY - y / 10.0) / 192.0 + 92);

        var region = (ushort)((ySector << 8) | (xSector & 0xFF));
        return (x, y, region);
    }

    /// <summary>
    /// Converts internal Silkroad coordinates (region, x, y) to in-game coordinates (PosX, PosY).
    /// </summary>
    public static (double posX, double posY) SroToGame(ushort region, double x, double y)
    {
        var xSec = region & 0xFF;
        var ySec = (region >> 8) & 0xFF;

        var posX = (xSec - 135) * 192.0 + (x / 10.0);
        var posY = (ySec - 92) * 192.0 + (y / 10.0);

        return (posX, posY);
    }
}

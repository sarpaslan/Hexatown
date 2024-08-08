using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
[Flags]
public enum BiomeType : int
{
    None = 0,
    Woodlands = 1 << 0,
    Plains = 1 << 1,
    Forest = 1 << 2,
    Highlands = 1 << 3,
    Mountains = 1 << 4,
    Hills = 1 << 5,
    Marsh = 1 << 6,
    Scrublands = 1 << 7,
    Wasteland = 1 << 8,
    Desert = 1 << 9,
    Jungle = 1 << 10,
    RedDesertForest = 1 << 11,
    RedDesertMountain = 1 << 12,
    RedDirt = 1 << 13,
    SwampJungle = 1 << 14,
    Wetlands = 1 << 15,
    YellowDesertForest = 1 << 16,
    YellowDesertForestBurned = 1 << 17,
    YellowDesertHills = 1 << 18,
    YellowDesertMountain = 1 << 19,
    ClayPit = 1 << 20,
}
public static class Biome
{
    public static Dictionary<string, BiomeType> Biomes = new Dictionary<string, BiomeType>();

    public static Action<BiomeType> OnBiomeUnlocked;

    private static Dictionary<BiomeType, bool> BiomesUnlocked = new();

    public static int UnlockedBiomeCount;

    public static bool IsBiome(Tile tile, BiomeType type)
    {
        return GetBiome(tile.name) == type;
    }
    public static BiomeType GetBiome(Tile tile)
    {
        if (tile == null)
            return BiomeType.None;
        return GetBiome(tile.name);
    }
    public static BiomeType GetBiome(string name)
    {
        if (Biomes.TryGetValue(name, out var type))
        {
            return type;
        }
        if (Enum.TryParse(name, true, out BiomeType biome))
        {
            Biomes.Add(name, biome);
            return biome;
        }
        return BiomeType.None;
        throw new NullReferenceException($"Can't find the biome with name {name}");
    }
    public static void UnlockBiome(BiomeType type)
    {
        if (IsBiomeUnlocked(type))
            return;
        OnBiomeUnlocked?.Invoke(type);
        BiomesUnlocked.Add(type, true);
        UnlockedBiomeCount++;
    }
    public static bool IsBiomeUnlocked(BiomeType type)
    {
        if (type == BiomeType.None)
            return true;
        return BiomesUnlocked.ContainsKey(type);
    }

    public static BiomeType GetBiome(Vector3Int position)
    {
        return GetBiome(TileMapController.Instance.Map.GetTile(position) as Tile);
    }

    public static string GetDescription(BiomeType type)
    {
        switch (type)
        {
            case BiomeType.None:
                return "Uncategorized";
            case BiomeType.Woodlands:
                return "A temperate region with scattered trees, often interspersed with grasslands.";
            case BiomeType.Plains:
                return "A vast, flat expanse of open grassland, often with few trees.";
            case BiomeType.Forest:
                return "A densely packed area dominated by tall trees, providing shade and diverse plant life.";
            case BiomeType.Highlands:
                return "Elevated terrain with rolling hills, offering scenic vistas and potentially harsh weather.";
            case BiomeType.Mountains:
                return "A formidable landscape characterized by steep slopes, peaks, and valleys, often with snow-capped summits.";
            case BiomeType.Hills:
                return "A series of smaller, rounded elevations, often found bordering other biomes.";
            case BiomeType.Marsh:
                return "A low-lying area with waterlogged ground, typically supporting reeds, grasses, and moisture-loving plants.";
            case BiomeType.Scrublands:
                return "A dry, arid region dominated by low-lying shrubs and bushes, adapted to survive with limited rain.";
            case BiomeType.Wasteland:
                return "A barren or wasteland biome with little vegetation, possibly due to erosion or harsh conditions.";
            case BiomeType.Desert:
                return "A hot, dry region with sparse vegetation, often featuring sand dunes and rocky outcrops.";
            default:
                return "Unknown Biome";
        }
    }
}

using System;
using KeuGames.Sound;
using TMPro;
using UnityEngine;

public enum SpriteIcon : int
{
    Null = 0,
    Food = 1,
    Wood = 2,
    Population = 3,
    Gold = 4,
    Cabbage = 5,
    Wheat = 6,
    Tomato = 7,
    Onion = 8,
    Potato = 9,
    CowMeat = 10,
    PigMeat = 11,
    ChickenMeat = 12,
    SheepMeat = 13,
    SheepProduct = 14,
    CowProduct = 15,
    ChickenProduct = 16,
    PigProduct = 17,
    Fish = 18,
    Balance = 19,
    Upgrade = 20,
    Farmer = 73,
    Worker = 74,
    Artisan = 75,
    FARM_YIELD_INCREASE_SKILL = 1256,
    FARM_SPEED_INCREASE_SKILL = 1257,
    FARM_AUTO_GATHER_SKILL = 1258,
    QuestionMarkIcon = 600,

    Error = 999,
    Cancel = 998,
    Water = 996,
    CowIcon = 511,
    SheepIcon = 512,
    ChickenIcon = 513,
    PigIcon = 514,
    Cut = 100,
    Gather = 101,
    Happy = 997,
    Villager = 995,
    Tree = 1260,
    Coal = 1267,
    Clothes = 1268,
    Bread = 1269,
    Soap = 1270,
    Tallow = 1271,
    Flour = 1272,
    IronOre = 1273,
    Steel = 1274,
    GoldOre = 1275,
    WellWater = 1276,
    GroupOfPigs = 1277,
    GroupOfCows = 1278,
    GroupOfSheep = 1279,
    GroupOfChicks = 1280,
    Cave = 1281,
    Tent = 1282,
    Pyramid = 1283,
    Clay = 1284,
    ClayBrick = 1285,
    Pickaxe = 1286,
    StiltHouse = 1287,
    Cocoa = 1288,
    Settings = 9009,
    Research = 9008,
    Pause = 9007,
    Play = 9006,

}

public enum FontType : int
{
    Null = 0,
    Small = 1,
    Normal = 2,
    Big = 3,
    Large = 4,
    VerrySmall = 5,
}
[Serializable]
public class FontResource
{
    public TMP_FontAsset Font;
    public FontType Type;
    public float Size;
}

[Serializable]
public class SpriteResource
{
    public UnityEngine.Sprite Icon;
    public SpriteIcon Type;
}
[Serializable]
public class BiomeIcons
{
    public BiomeType Type;
    public UnityEngine.Sprite Icon;
}

[CreateAssetMenu]
public class GameResources : ScriptableObject
{
    [SerializeField]
    public SpriteResource[] m_sprites;

    [SerializeField]
    private FontResource[] m_fonts;

    [SerializeField]
    public BiomeIcons[] m_biomes;

    public SpriteRenderer HexPrefab;
    public ScriptableSoundPlayer SoundPlayer;
    public void Reset()
    {
        SoundPlayer.Reset();
    }

    public UnityEngine.Sprite GetIcon(SpriteIcon type)
    {
        foreach (var v in m_sprites)
        {
            if (v.Type == type)
                return v.Icon;
        }
        return null;
    }
    public UnityEngine.Sprite GetBiomeIcon(BiomeType type)
    {
        foreach (var v in m_biomes)
        {
            if (v.Type == type)
                return v.Icon;
        }
        return null;
    }


    public FontResource GetFont(FontType type)
    {
        foreach (var v in m_fonts)
        {
            if (v.Type == type)
                return v;
        }
        return null;
    }

    public SpriteIcon ToIconType(Stats key)
    {
        switch (key)
        {
            case Stats.POP:
                return SpriteIcon.Population;
            case Stats.MAXPOP:
                return SpriteIcon.Population;
            case Stats.WOOD:
                return SpriteIcon.Wood;
            case Stats.GOLD:
                return SpriteIcon.Gold;
            case Stats.WHEAT:
                return SpriteIcon.Wheat;
            case Stats.TOMATO:
                return SpriteIcon.Tomato;
            case Stats.POTATO:
                return SpriteIcon.Potato;
            case Stats.CABBAGE:
                return SpriteIcon.Cabbage;
            case Stats.PORK:
                return SpriteIcon.PigMeat;
            case Stats.WOOL:
                return SpriteIcon.SheepProduct;
            case Stats.FISH:
                return SpriteIcon.Fish;
            case Stats.EGG:
                return SpriteIcon.ChickenProduct;
            case Stats.BALANCE:
                return SpriteIcon.Balance;
            case Stats.NONE:
                return SpriteIcon.Error;
            case Stats.COAL:
                return SpriteIcon.Coal;
            case Stats.CLOTHES:
                return SpriteIcon.Clothes;
            case Stats.BREAD:
                return SpriteIcon.Bread;
            case Stats.SOAP:
                return SpriteIcon.Soap;
            case Stats.TALLOW:
                return SpriteIcon.Tallow;
            case Stats.FLOUR:
                return SpriteIcon.Flour;
            case Stats.CLAY:
                return SpriteIcon.Clay;
            case Stats.BRICK:
                return SpriteIcon.ClayBrick;
            case Stats.MILK:
                return SpriteIcon.CowProduct;
            case Stats.IRON_ORE:
                return SpriteIcon.IronOre;
            case Stats.GOLD_ORE:
                return SpriteIcon.GoldOre;
        }
        return SpriteIcon.Null;
    }

    internal SpriteIcon GetVillagerTierIcon(WorkTier tier)
    {
        switch (tier)
        {
            case WorkTier.FARMER:
                return SpriteIcon.Farmer;
            case WorkTier.WORKER:
                return SpriteIcon.Worker;
            case WorkTier.ARTISAN:
                return SpriteIcon.Artisan;
        }
        return SpriteIcon.Null;
    }

}

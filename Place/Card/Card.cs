using System;
using UnityEngine;
using UnityEngine.Tilemaps;
[Serializable]
public class CardPrice
{
    public Stats Stats;
    public int Value;
}

[CreateAssetMenu]
public class Card : ScriptableObject
{
    public string Name;
    public string Description;
    public Tile Tile;
    public PlaceType Type;
    public bool Refundable = true;
    public bool CanFire;
    public int SortOrder = 0;
    public GameObject MenuTemplate;
    public PlaceView PlaceView;
    [NonSerialized]
    [Range(0, 100)]
    [SerializeField]
    private int m_initialFrequency;
    public int RuntimeFrequency
    {
        get => m_initialFrequency;
    }
    public bool DefaultUnlocked = false;

    [Header("This biome will be only placable in this biomes")]
    public BiomePlaceRules[] Rules;
    public BiomeType IdealBiomeTypes;
    public bool OnlyAllowedOnce;
    public Season Season;
    public CardPrice[] Prices;
    public bool Destroyable = true;

    public int RevealDepth = 0;

    public Place CreatePlace(Vector3Int position)
    {
        switch (Type)
        {
            case PlaceType.CASTLE:
                return new Castle(this, position);
            case PlaceType.FARMLAND:
                return new FarmLand(this, position);
            case PlaceType.WINDMILL:
                return new WindMill(this, position);
            case PlaceType.VILLAGE:
                return new Village(this, position);
            case PlaceType.MINES:
                return new Mines(this, position);
            case PlaceType.LUMBERJACK:
                return new Lumberjack(this, position);
            case PlaceType.EXPLORER_TOWER:
                return new ExplorerTower(this, position);
            case PlaceType.RANCH:
                return new Ranch(this, position);
            case PlaceType.WELL:
                return new Well(this, position);
            case PlaceType.CEMETERY:
                return new Cemetery(this, position);
            case PlaceType.FISHINGDOCK:
                return new FishingDock(this, position);
            case PlaceType.STOREHOUSE:
                return new Storehouse(this, position);
            case PlaceType.MARKETPLACE:
                return new Marketplace(this, position);
            case PlaceType.BURNT:
                return new BurntPlace(this, position);
            case PlaceType.LIBRARY:
                return new Library(this, position);
            case PlaceType.TAILOR:
                return new Tailor(this, position);
            case PlaceType.BAKER:
                return new Baker(this, position);
            case PlaceType.SOAPMAKER:
                return new SoapMaker(this, position);
            case PlaceType.CLAYQUARRY:
                return new ClayQuarry(this, position);
            case PlaceType.CLAYFACTORY:
                return new ClayFactory(this, position);
            case PlaceType.EXPLORATION_PLACE:
                return new ExplorationPlace(this, position);
        }
        return null;
    }
    public virtual bool CanPlace(Vector3Int position)
    {
        return true;
    }
}
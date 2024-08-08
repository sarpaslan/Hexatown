using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;



public class ExplorationAnimal : ExplorationFoundable
{
    public override void OnClicked()
    {
        var ranch = CardController.GetCard<RanchCard>();
        var selection = ranch.Selections.First(t => t.Name == Name);
        selection.Unlocked = true;
        Debug.Log(selection);
    }
}
public class ExplorationCocoaSeeds : ExplorationFoundable
{
    public override void OnClicked()
    {
    }
}

public class ExplorationSeed : ExplorationFoundable
{
    public override void OnClicked()
    {
        var farmCard = CardController.GetCard<FarmCard>();
        farmCard.Selections.First(t => t.Name == Name).Unlocked = true;
    }
}

public class ExplorationFoundablePlace : ExplorationFoundable
{
    public override void OnClicked()
    {
        if (Name == "Cave")
        {
            GameScreen.Instance.ShowInfo("Hmmm", "There is nothing to do here right now");
        }
    }
}

public abstract class ExplorationFoundable
{
    public string Name;
    public Sprite Sprite;
    public bool Collectable;
    public bool Collected;
    public string FoundText;
    public float Chance = 1f / 1000f;
    public BiomeType Biomes;
    public bool Found;
    public int TileLimit;
    public PlaceType NeededResearch;
    public abstract void OnClicked();
}


public class ExplorationController : MonoBehaviour
{
    public Card GhostCard;
    private List<ExplorationFoundable> m_foundables;

    public List<ExplorationFoundable> Foundables
    {
        get
        {
            if (m_foundables == null)
                LoadFoundables();
            return m_foundables;
        }
    }

    [SerializeField]
    private FoundItemBehaviour m_foundablePrefab;
    public static ExplorationController Instance;
    void OnDestroy()
    {
        Foundables.Clear();
        TileMapController.Instance.OnRevealed -= OnReveal;
    }
    public float time;

    void Start()
    {
        TileMapController.Instance.OnRevealed += OnReveal;
    }
    public void LoadFoundables()
    {
        m_foundables = new List<ExplorationFoundable>
        {
            new ExplorationAnimal()
            {
                Name = "Pig",
                Sprite = SpriteIcon.GroupOfPigs.ToSprite(),
                FoundText = "Ranch Animal\nPigs",
                Biomes = BiomeType.Plains | BiomeType.Marsh | BiomeType.Woodlands | BiomeType.Scrublands,
                Chance = 1f / 20f,
                TileLimit = 50,
                Collectable = true,
                NeededResearch = PlaceType.WELL
            },
            new ExplorationAnimal()
            {
                Name = "Chicken",
                Sprite = SpriteIcon.GroupOfChicks.ToSprite(),
                FoundText = "Ranch Animal\nChicken",
                Biomes = BiomeType.Plains | BiomeType.Woodlands | BiomeType.Scrublands,
                Chance = 1f / 40f,
                TileLimit = 100,
                Collectable = true,
                NeededResearch = PlaceType.RANCH
            },
            new ExplorationAnimal()
            {
                Name = "Cow",
                Sprite = SpriteIcon.GroupOfCows.ToSprite(),
                FoundText = "Ranch Animal\nCow",
                Biomes = BiomeType.Highlands | BiomeType.Hills | BiomeType.Plains,
                Chance = 1f / 30,
                TileLimit = 120,
                Collectable = true,
                NeededResearch = PlaceType.RANCH
            },
            new ExplorationAnimal()
            {
                Name = "Sheep",
                Sprite = SpriteIcon.GroupOfSheep.ToSprite(),
                FoundText = "Ranch Animal\nSheep",
                Biomes = BiomeType.Plains | BiomeType.Hills | BiomeType.Highlands | BiomeType.Woodlands,
                Chance = 1f / 2f,
                TileLimit = 25,
                Collectable = true,
                NeededResearch = PlaceType.WELL
            },
            new ExplorationSeed()
            {
                Name = "Tomato",
                Sprite = SpriteIcon.Tomato.ToSprite(),
                FoundText = "Farm\nTomato Seeds",
                Biomes = BiomeType.Wetlands,
                Chance = 1f / 20f,
                TileLimit = 140,
                Collectable = true,
                NeededResearch = PlaceType.WINDMILL
            },
            new ExplorationSeed()
            {
                Name = "Cabbage",
                Sprite = SpriteIcon.Cabbage.ToSprite(),
                FoundText = "Farm\nCabbage Seeds",
                Biomes = BiomeType.SwampJungle,
                Chance = 1f / 2f,
                TileLimit = 150,
                Collectable = true,
                NeededResearch = PlaceType.WINDMILL
            },
            new ExplorationSeed()
            {
                Name = "Wheat",
                Sprite = SpriteIcon.Wheat.ToSprite(),
                FoundText = "Farm\nWheat Seeds",
                Biomes = BiomeType.Plains | BiomeType.Hills | BiomeType.Highlands | BiomeType.Woodlands,
                Chance = 1f / 2f,
                TileLimit = 25,
                Collectable = true,
                NeededResearch = PlaceType.EXPLORER_TOWER
            },
            new ExplorationFoundablePlace()
            {
                Name = "Cave",
                Sprite = SpriteIcon.Cave.ToSprite(),
                FoundText = "",
                Biomes = BiomeType.Highlands | BiomeType.Hills,
                TileLimit = 60,
                Collectable = false,
                Chance = 1f / 2,
                NeededResearch = PlaceType.FISHINGDOCK
            },
            new ExplorationFoundablePlace()
            {
                Name = "Tent",
                Sprite = SpriteIcon.Tent.ToSprite(),
                FoundText = "",
                Biomes = BiomeType.Desert | BiomeType.Wasteland | BiomeType.Scrublands,
                TileLimit = 80,
                Collectable = false,
                Chance = 1f / 2,
                NeededResearch = PlaceType.TAILOR
            },
            new ExplorationFoundablePlace()
            {
                Name = "Pyramid",
                Sprite = SpriteIcon.Pyramid.ToSprite(),
                FoundText = "",
                Biomes = BiomeType.Desert,
                TileLimit = 150,
                Collectable = false,
                Chance = 1f / 12,
                NeededResearch = PlaceType.WELL
            },
            new ExplorationFoundablePlace()
            {
                Name = "Stilt House",
                Sprite = SpriteIcon.StiltHouse.ToSprite(),
                FoundText = "",
                Biomes = BiomeType.Jungle | BiomeType.SwampJungle,
                TileLimit = 100,
                Collectable = false,
                Chance = 1f / 12,
                NeededResearch = PlaceType.STOREHOUSE
            },
            new ExplorationFoundablePlace()
            {
                Name = "Stilt House",
                Sprite = SpriteIcon.StiltHouse.ToSprite(),
                FoundText = "",
                Biomes = BiomeType.RedDesertForest,
                TileLimit = 100,
                Collectable = false,
                Chance = 1f / 12,
                NeededResearch = PlaceType.BAKER
            },
            new ExplorationCocoaSeeds()
            {
                Name = "Cocoa",
                Sprite = SpriteIcon.Cocoa.ToSprite(),
                FoundText = "Cocoa Seeds",
                Biomes = BiomeType.Jungle | BiomeType.SwampJungle,
                TileLimit = 40,
                Collectable = true,
                Chance = 1f / 50,
                NeededResearch = PlaceType.TAILOR
            }
        };

    }
    void Awake()
    {
        Instance = this;
    }

    public void Unlock(string name)
    {
        var x = this.Foundables.First(t => t.Name == name);
        x.Found = true;
        if (x.Collectable)
        {
            x.OnClicked();
            x.Collected = true;
        }
    }

    private void OnReveal(Vector3Int pos, BiomeType type)
    {
        var tileCount = TileMapController.RevealedTiles.Count;
        foreach (var f in Foundables)
        {
            if (f.Collected || f.Found) continue;
            if (f.TileLimit > tileCount) continue;
            if (!f.Biomes.HasFlag(type)) continue;
            if (UnityEngine.Random.Range(0f, 1f) > f.Chance)
                continue;
            if (!CardController.IsUnlocked(f.NeededResearch))
                continue;
            if (time + 2 >= Time.unscaledTime)
                return;
            time = Time.unscaledTime;
            CardController.Instance.Place(pos, GhostCard, true);
            GameController.GetPlace(pos).Name = f.Name;
            break;
        }
    }
    public FoundItemBehaviour CreateFoundablePlace(ExplorationPlace place)
    {
        var f = Foundables.First(t => t.Name == place.Name);
        f.Found = true;
        var preb = Instantiate(m_foundablePrefab);
        preb.OnClicked.AddListener(() =>
        {
            if (f.Collectable)
            {
                f.OnClicked();
                place?.DestroyPlace(true);
                f.Collected = true;
                preb.OnClicked.RemoveAllListeners();
            }
        });
        preb.SetContent(f);
        preb.transform.position = place.WorldPosition;
        preb.transform.localScale = Vector3.zero;
        preb.transform.DOScale(1.25f, 0.5f).SetEase(Ease.OutBounce).SetUpdate(true);
        if (f.Collectable)
        {
            float jumpPower = 0.1f;
            int numJumps = 4;

            float duration = 2f;
            preb.transform.DOJump(preb.transform.position, jumpPower, numJumps, duration)
            .SetLoops(-1).SetUpdate(true).SetEase(Ease.Linear);
        }
        return preb;
    }
    internal void RemoveFoundablePlace(FoundItemBehaviour found)
    {
        Destroy(found.gameObject);
    }
}

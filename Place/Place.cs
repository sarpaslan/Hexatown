using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;


[Serializable]
public abstract class Place
{

    [UsefullInfo(readOnly: true)]
    public int Id;

    [UsefullInfo]
    public string Name;

    [UsefullInfo(readOnly: true)]
    public BiomeType BiomeType;
    public Vector3Int[] Neighbours;
    public Vector3Int Position;

    [NonSerialized]
    public Vector3 WorldPosition;
    public Card Card;
    public static int id;

    [UsefullInfo(readOnly: true)]
    public bool OnFire => Fire > 0;

    [UsefullInfo(readOnly: true)]
    public float Fire;

    [UsefullInfo(readOnly: true)]
    public int Tick;

    [UsefullInfo(readOnly: true)]

    public byte ErrorTick = 0;
    public PlaceError Error;

    [UsefullInfo(readOnly: true)]
    public abstract int Tax { get; }
    public bool Destroyed;

    public bool Burnt;

    public bool PaidTax = true;


    public Place(Card card, Vector3Int pos)
    {
        Id = id++;
        Card = card;
        Name = card.Name.ToString();
        Position = pos;
        Neighbours = TileMapController.Instance.GetNeighbours(pos).ToArray();
        WorldPosition = TileMapController.Instance.Grid.CellToWorld(pos);
        BiomeType = Biome.GetBiome(Position);
    }

    public virtual void OnBeforePlace(bool silent)
    {
        TileMapController.Instance.SetTile(Position, Card.Tile);
    }
    public virtual void OnPlaced(bool silent)
    {
    }


    public virtual void OnAfterPlace(bool silent)
    {

    }
    public Vector3 RandomPointInside(float offset = 0.5f)
    {
        Vector3 worldPosition = WorldPosition;
        return LittleRandom.XY(worldPosition, offset);
    }
    public void ShowHappy()
    {
        var image = GameScreen.Instance.ShowImage(WorldPosition, SpriteIcon.Happy);
        image.transform.DOMoveY(image.transform.position.y + 8, 0.7f);
        image.DOFade(0, 0.7f);
        UnityEngine.Object.Destroy(image.gameObject, 0.8f);
    }
    public bool IsPlacedIdeal()
    {
        return Card.IdealBiomeTypes.HasFlag(BiomeType);
    }

    public virtual string GetName()
    {
        return Name;
    }
    public virtual string GetDescription()
    {
        var str = "";
        str += Strings.ToString(this.Error, this);

        if (GameConfig.DEBUG_INFO)
            str += "<color=grey>\n" + WorldPosition + "</color>\n";

        if (OnFire)
            str += $"<color=red>Fire {(int)Fire}%</color>\n";
        return str + Card.Description;
    }

    public virtual void OnTick()
    {

    }

    public virtual void OnDeselected()
    {
    }

    public virtual void OnSelect()
    {
    }

    public virtual void OnNeighbourChanged(Place place)
    {
    }

    public static bool CanPlace(Card card, Tile tile)
    {
        foreach (var v in card.Rules)
        {
            if (v.PlaceableBiomes.HasFlag(Biome.GetBiome(tile)))
            {
                return true;
            }
        }
        return false;
    }

    public virtual UnityEngine.Sprite GetSprite()
    {
        var tile = TileMapController.Instance.GetTileAt(Position);
        return tile.sprite;
    }

    public virtual void NotifyPropertyChange()
    {
        TileInfoController.Instance.Refresh(this);
    }

    public virtual void OnCameraMove()
    {
    }

    public virtual void OnAnyPlaced(Place place) { }
    public virtual void OnBurnt()
    {
    }
    public virtual void DestroyPlace(bool silent = false)
    {
        if (!silent)
        {
            var spred = UnityEngine.Object.Instantiate(GameController.Resources.HexPrefab);
            spred.transform.position = WorldPosition - new Vector3(0, GameConfig.MAGIC_Y, 0);
            spred.material.EnableKeyword("FADE_ON");
            spred.material.SetFloat("_FadeAmount", 0);
            spred.material.DOFloat(1, "_FadeAmount", 2);
            GameController.Instance.PlayPositionalSoundAt(WorldPosition, "building_destroy");
            spred.sprite = GetSprite();
            spred.DOFade(0, 2);
            UnityEngine.Object.Destroy(spred.gameObject, 2.1f);
        }
        GameController.Instance.RemovePlace(this);
        if (Card.Refundable)
        {
            foreach (var price in Card.Prices)
            {
                GameController.Stats.Add(price.Stats, (int)(price.Value * (GameConfig.REFUND_PERCENT / 100f)));
            }
        }
        Destroyed = true;
    }

    public string ToUsefullString()
    {
        var text = "";
        var coll = UsefullInfoUtils.GetUsefullInfoCollection(this);
        foreach (var c in coll)
        {
            text += c.Name + " = " + c.Value + "\n";
        }
        return text;
    }
}
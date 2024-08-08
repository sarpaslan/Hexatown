using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FireController : MonoBehaviour
{
    public SpriteRenderer FirePrefab;
    public Dictionary<Place, SpriteRenderer> fires = new Dictionary<Place, SpriteRenderer>();
    public static FireController Instance;
    public Card BurntCard;

    private void OnEnable()
    {
        GameController.Instance.OnGameTick += FireTickLoop;
    }
    private void OnDisable()
    {
        GameController.Instance.OnGameTick += FireTickLoop;
    }

    public void Awake()
    {
        Instance = this;
    }
    public void RemoveFire(Place place)
    {
        if (fires.ContainsKey(place))
        {
            place.Fire = 0;
            if (this.fires.TryGetValue(place, out var pl))
            {
                Destroy(pl.gameObject);
                this.fires.Remove(place);
            }
            VillagerController.Instance.NotifyFireEnded(place);
        }
    }

    public void FireTickLoop(int tick)
    {
        foreach (var fire in fires)
        {
            fire.Key.Fire += GameConfig.FIRE_SPEED;
            VillagerController.Instance.NotifyFire(fire.Key);
            if (fire.Key.Fire > 100)
            {
                fire.Key.Fire = 0;
                Instance.Burnt(fire.Key);
                VillagerController.Instance.NotifyFireEnded(fire.Key);
                if (TileSelectionController.Instance.SelectedTile == fire.Key.Position)
                {
                    TileInfoController.Instance.Deselect();
                    TileSelectionController.Instance.UnselectTile();
                }
                break;
            }
            else
            {
                fire.Value.material.SetFloat("_Alpha", fire.Key.Fire / 100);
                VillagerController.Instance.NotifyFire(fire.Key);
                fire.Key.NotifyPropertyChange();
                bool shouldExit = false;
                if (fire.Key.Fire > GameConfig.FIRE_SPREAD_STARTS_AT)
                {
                    if (Random.Range(0f, 100f) <= GameConfig.FIRE_SPREAD_PERCENTAGE)
                    {
                        foreach (var n in fire.Key.Neighbours)
                        {
                            var place = GameController.GetPlace(n);
                            if (place != null)
                            {

                                if (place.Card.CanFire && place.Fire <= 0)
                                {
                                    CreateFire(place);
                                    shouldExit = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (shouldExit)
                    break;
            }
        }
    }



    public Vector3Int CreateFire(Place place)
    {
        if (place == null) return Vector3Int.zero;
        if (!place.Card.CanFire)
            return Vector3Int.zero;
        if (place.Burnt)
            return place.Position;
        if (place.OnFire)
            return place.Position;

        place.Fire = 0.1f;
        var spr = PlaceFire(place.GetSprite(), place.WorldPosition);
        spr.material.SetFloat("_Alpha", place.Fire / 100f);
        this.fires.Add(place, spr);
        return place.Position;
    }

    public void UseWater(Place place)
    {
        if (this.fires.TryGetValue(place, out var pl))
        {
            place.Fire -= 25f;
            if (place.Fire < 0)
            {
                Destroy(pl.gameObject);
                this.fires.Remove(place);
                VillagerController.Instance.NotifyFireEnded(place);
            }
            else
            {
                pl.material.SetFloat("_Alpha", place.Fire / 100f);
            }
        }
    }
    public void Burnt(Place place)
    {
        place.Burnt = true;
        place.Fire = 0;
        place.OnBurnt();
        place.DestroyPlace();
        if (this.fires.TryGetValue(place, out var pl))
        {
            Destroy(pl.gameObject);
            this.fires.Remove(place);
        }
        CardController.Instance.Place(place.Position, BurntCard, true);
    }

    public SpriteRenderer PlaceFire(UnityEngine.Sprite sprite, Vector3 worldLocation)
    {
        var fire = Instantiate(FirePrefab);
        fire.transform.position = worldLocation - new Vector3(0, GameConfig.MAGIC_Y, 0);
        fire.sprite = sprite;
        return fire;
    }
}


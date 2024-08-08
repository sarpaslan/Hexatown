using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    private static GameController m_instance;
    public static GameController Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<GameController>();
            }
            return m_instance;
        }
    }
    public static GameResources Resources => Instance == null ? null : Instance.m_gameResources;
    public static PlayerStats Stats;
    public static List<Place> Places;
    private static Dictionary<PlaceType, int> PlaceCounts = new Dictionary<PlaceType, int>();

    public static int CountOf(PlaceType type)
    {
        if (PlaceCounts.TryGetValue(type, out int count))
        {
            return count;
        }
        return 0;
    }

    public static Castle Castle;
    public static Camera Camera;

    public GameObject CollectableObject;
    public GameResources m_gameResources;

    public Action<int> OnGameTick;
    public static Action<Place> OnPlaceCreated;

    public int Tick;

    private void Awake()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = Application.isEditor ? 0 : 75;
        Camera = FindObjectOfType<Camera>();
        Stats = new PlayerStats();
        Places = new List<Place>();
    }

    public void Start()
    {
        current = -1;
        SaveController.Instance.LoadSave("gameSave");
        if (!Application.isEditor)
            GameScreen.Instance.ShowInfo("Leave Feedback", "This game is still in early development.\nIcons and some graphics will change.\n\nSome tutorials and content might be missing, and some texts might be incorrect.\n\nThank you for testing out the game.");


        var ranch = CardController.GetCard<FarmCard>();
        ranch.Selections.First(t => t.Name == "Potato").Unlocked = true;
    }
    private float current = -1;
    public void Update()
    {
        if (Time.time > current)
        {
            current = Time.time + GameConfig.GAME_TICK_PER_SECONDS;
            OnTick();
        }
    }

    public void OnTick()
    {
        try
        {
            foreach (var place in Places)
            {
                HandleWorkPlace(place);
                HandlePopulationPlace(place);
                if (place.Error != PlaceError.NONE)
                {
                    place.ErrorTick++;
                    if (place.ErrorTick > 20)
                    {
                        GameScreen.Instance.ShowError(place.Id, place.WorldPosition);
                    }
                }
                else
                {
                    place.ErrorTick = 0;
                    GameScreen.Instance.Hide(place.Id);
                }
                place.OnTick();
                place.Tick++;
            }

            if (Places.Count > 0)
                if (Castle.Tick > 750)
                    TryCreateFire();

            OnGameTick?.Invoke(Tick++);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e.ToString());
            GameConfig.ERROR = e.ToString();
        }
    }
    public static T[] GetPlaces<T>() where T : Place
    {
        return Places.OfType<T>().ToArray();
    }

    public static Place GetClosestBurnable(Vector3 worldPos)
    {
        Place closestPlace = default;
        float closestDistance = float.MaxValue;

        foreach (var place in Places)
        {
            if (place.Burnt) continue;
            if (!place.Card.CanFire) continue;
            float distance = Vector3.Distance(worldPos, place.WorldPosition);
            if (distance < closestDistance)
            {
                closestPlace = place;
                closestDistance = distance;
            }
        }
        return closestPlace;
    }

    public static T GetClosestPlace<T>(Vector3 worldPos) where T : Place
    {
        T closestPlace = default;
        float closestDistance = float.MaxValue;

        foreach (var place in Places.OfType<T>())
        {
            if (place.Burnt) continue;
            float distance = Vector3.Distance(worldPos, place.WorldPosition);
            if (distance < closestDistance)
            {
                closestPlace = place;
                closestDistance = distance;
            }
        }
        return closestPlace;
    }

    public static Place GetPlace(Vector3Int pos)
    {
        return Places.FirstOrDefault(p => p.Position == pos);
    }

    public static Place GetPlace(int id)
    {
        return Places.FirstOrDefault(t => t.Id == id);
    }

    public static void CreatePlace(Vector3Int pos, Card card, bool silent)
    {
        var place = card.CreatePlace(pos);
        if (card.Type == PlaceType.CASTLE)
            Castle = place as Castle;
        if (PlaceCounts.TryGetValue(card.Type, out int i))
        {
            PlaceCounts[card.Type] = i + 1;
        }
        else
        {
            PlaceCounts.Add(card.Type, 1);
        }

        Places.Add(place);
        place.OnBeforePlace(silent);
        place.OnPlaced(silent);
        place.OnAfterPlace(silent);

        //TODO FIX THIS
        foreach (var p in Places) //What is this?
        {
            if (p != place)
            {
                p.OnAnyPlaced(place);
            }
        }
        if (!silent)
        {
            foreach (var neighbourPos in place.Neighbours)
            {
                var neighbour = GetPlace(neighbourPos);
                neighbour?.OnNeighbourChanged(place);
            }
        }
        OnPlaceCreated?.Invoke(place);
    }

    public GameObject CreateCollectable(Vector3 spawnPosition, UnityEngine.Sprite icon)
    {
        spawnPosition = LittleRandom.XY(spawnPosition, 0.2f);
        var obj = Instantiate(CollectableObject);
        obj.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = icon;

        spawnPosition += new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.4f, .6f), 0);
        var sequence = DOTween.Sequence();
        obj.transform.position = new Vector3(spawnPosition.x, spawnPosition.y - 0.6f, spawnPosition.z);
        sequence.Append(obj.transform.DOMoveY(spawnPosition.y + 0.6f, 0.6f));
        sequence.Append(obj.transform.DOMoveY(spawnPosition.y, 1.0f).SetEase(Ease.OutBounce));
        return obj;
    }

    public GameObject CreateCollectable(Vector3 spawnPosition, SpriteIcon icon)
    {
        return CreateCollectable(spawnPosition, Resources.GetIcon(icon));
    }

    public GameObject CreateCollectable(Vector3 spawnPosition, Stats stat)
    {
        return CreateCollectable(spawnPosition, Resources.GetIcon(Resources.ToIconType(stat)));
    }


    private void HandleWorkPlace(Place place)
    {
        if (place is WorkPlace workPlace)
        {
            if (workPlace.CurrentWorkerCount < workPlace.NeededWorkerCount)
            {
                var villagers = VillagerController.Villagers;
                foreach (var vlg in villagers)
                {
                    if (vlg.Tier != workPlace.Tier)
                        continue;

                    if (!VillagerController.Instance.IsAvailable(vlg)) continue;
                    if (Vector3.Distance(vlg.Home.WorldPosition, workPlace.WorldPosition) > GameConfig.HOME_WORK_MAX_DISTANCE)
                    {
                        continue;
                    }
                    if (!vlg.Spawned)
                    {
                        vlg.Spawn();
                    }
                    vlg.Work = workPlace;
                    workPlace.AddWorker(vlg);
                    vlg.Stop();
                    vlg.IsBussy = false;
                    Job.Move(vlg, workPlace.WorldPosition).Forget();
                    break;
                }
            }
            if (workPlace.Error == PlaceError.CANT_WORK_ON_THIS_SEASON ||
                workPlace.Error == PlaceError.NOT_ENOUGH_WORKER ||
                workPlace.Error == PlaceError.NOT_ENOUGH_TAX)
            {
                workPlace.Error = PlaceError.NONE;
            }

            if (!workPlace.CanWorkOnThisSeason())
                workPlace.Error = PlaceError.CANT_WORK_ON_THIS_SEASON;

            if (workPlace.CurrentWorkerCount != workPlace.NeededWorkerCount)
                workPlace.Error = PlaceError.NOT_ENOUGH_WORKER;

            if (!workPlace.PaidTax)
                workPlace.Error = PlaceError.NOT_ENOUGH_TAX;
        }
    }

    private void HandlePopulationPlace(Place place)
    {
        if (place is PopulationPlace population)
        {
            HandlePopulationGrowth(population);
        }
    }

    private void HandlePopulationGrowth(PopulationPlace population)
    {
        if (Stats.Get(global::Stats.POP) < Stats.Get(global::Stats.MAXPOP) && population.Tick % GameConfig.NEW_VILLAGER_TICK == 0)
        {
            if (population.Population < population.MaxPopulation)
            {
                if (population.CanSpawn())
                {
                    var villager = VillagerController.Instance.CreateVillager(population, population.WorldPosition);
                    population.Villagers.Add(villager);
                    GameScreen.Instance.Hide(population.Id);
                    TileInfoController.Instance.Refresh(population);
                }
            }
        }
    }

    private void TryCreateFire()
    {
        if (Random.Range(0f, 1f) <= (GameConfig.FIRE_CHANCE * SeasonController.Instance.CurrentSeasonProperties.SeasonFireChance))
        {
            var place = Places.SelectRandom();
            FireController.Instance.CreateFire(place);
        }
    }

    public void OnCameraMove()
    {
        foreach (var place in Places)
        {
            place.OnCameraMove();
        }
    }

    public void PlayPositionalSoundAt(Vector3 position, string sound)
    {
        var source = Resources.SoundPlayer.PlayAtPosition(position, sound);
        var src = source.gameObject.AddComponent<PositionalSound>();
        src.VolumeMultiplier = source.volume;
        src.Source = source;
    }

    public void RemovePlace(Place place)
    {
        if (TileSelectionController.Instance.SelectedTile == place.Position)
        {
            TileInfoController.Instance.Deselect();
            TileSelectionController.Instance.UnselectTile();
        }
        FireController.Instance.RemoveFire(place);
        GameScreen.Instance.Hide(place.Id);
        Unselect(place);
        Places.Remove(place);
        TileMapController.Instance.SetTile(place.Position, null);
        TileMapController.Instance.CheckRevealedTiles();
        var type = place.Card.Type;
        if (PlaceCounts.TryGetValue(type, out int val))
        {
            PlaceCounts[type] = val - 1;
        }
    }

    public static void Unselect(Place place)
    {
        TileSelectionController.Instance.UnselectTile();
        TileInfoController.Instance.Deselect();
    }

    public void AssignNewHome(Villager villager)
    {
        var homeAssigned = false;

        foreach (var village in Places.OfType<Village>())
        {
            if (villager.Tier == village.Tier)
            {
                if (village.Population < village.MaxPopulation)
                {
                    village.Villagers.Add(villager);
                    villager.Home = village;
                    TileInfoController.Instance.Refresh(village);
                    homeAssigned = true;
                    break;
                }
            }
        }
        if (!homeAssigned)
        {
            VillagerController.Instance.Kill(villager, "No Home", false);
        }
    }
    void OnDestroy()
    {
        Castle = null;
        Camera = null;
        m_gameResources.Reset();
        Places.Clear();
        PlaceCounts.Clear();
        VillagerController.Villagers.Clear();
        VillagerController.FireWorkers.Clear();
    }
}


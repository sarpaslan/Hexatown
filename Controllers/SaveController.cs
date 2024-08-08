using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

[Serializable]
public class GameData
{
    public int Day = 0;
    public int Year = 0;
    public Season Season;
    public List<PlaceType> Researched = new List<PlaceType>();
    public List<string> UnlockedExploration = new List<string>();
}

[Serializable]
public class VillagerData
{
    public int Id;
    public string Name;
    public short Age;
    public bool Dead;
}

[Serializable]
public class PlaceData
{
    public int Id;
    public string Name;
    public float WorkPercent = 0;
    public Vector3Int Position;
    public PlaceType Type;
    public WorkTier Tier;
    public List<VillagerData> Villagers = new List<VillagerData>();
    public string Extra = "";
    public List<RequiredStat> Req = new List<RequiredStat>();
}

[Serializable]
public class StatsData
{
    public Stats Stats;
    public int Value;
}

[Serializable]
public class GameSave
{
    public GameData GameData;
    public List<Vector3Int> RevealedTiles = new List<Vector3Int>();
    public List<PlaceData> Places = new List<PlaceData>();
    public List<StatsData> Stats = new List<StatsData>();
}

public class SaveController : MonoBehaviour
{
    public TMP_Text savedText;
    public static SaveController Instance;
    public float nextSave;
    void Awake()
    {
        Instance = this;
        if (GameConfig.AUTO_SAVE)
        {
            nextSave = Time.unscaledTime + 10;
        }
    }

    public void Update()
    {
        if (!GoalController.Completed)
        {
            return;
        }

        if (GameConfig.AUTO_SAVE)
        {
            if (Time.unscaledTime >= nextSave)
            {
                Save("gameSave");
                nextSave = Time.unscaledTime + 10;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
                Save("gameSave");
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.L))
                LoadSaveRestartAsync("gameSave").Forget();
        }
    }

    public void WriteTiles(GameSave save)
    {
        save.RevealedTiles.AddRange(TileMapController.RevealedTiles);
    }
    public void WritePlaces(GameSave save)
    {
        foreach (var p in GameController.Places)
        {
            var place = new PlaceData
            {
                Id = p.Id,
                Name = p.Name,
                Position = p.Position,
                Type = p.Card.Type,
                Villagers = new List<VillagerData>()
            };
            if (p is Village village)
            {
                place.Tier = village.Tier;
                foreach (var vlg in village.Villagers)
                {
                    place.Villagers.Add(new VillagerData()
                    {
                        Id = vlg.Id,
                        Age = vlg.Age,
                        Name = vlg.Name
                    });
                }
                place.Req.AddRange(village.Requirements);
            }
            else if (p is WorkPlace workPlace)
            {
                foreach (var vlg in workPlace.Villagers)
                {
                    place.Villagers.Add(new VillagerData()
                    {
                        Id = vlg.Id,
                    });
                }
                if (p is SelectionWorkPlace sWork)
                    place.Extra = sWork.Selection?.Name;
                if (p is ProductionPlace pp)
                    place.WorkPercent = pp.WorkPercent;
                if (p is Mines mine)
                    place.Extra = mine.MineData.ToString();
            }
            save.Places.Add(place);
        }
    }

    public void WriteResources(GameSave save)
    {
        save.Stats.AddRange(GameController.Stats.All.Select(t => new StatsData() { Stats = t.Key, Value = t.Value }));
    }
    public void LoadTiles(GameSave save)
    {
        foreach (var r in save.RevealedTiles)
        {
            TileMapController.Instance.Reveal(r);
        }
    }
    public void LoadPlaces(GameSave save)
    {
        foreach (var savedplace in save.Places)
        {
            CardController.Instance.Place(savedplace.Position, savedplace.Type, true);
            var place = GameController.GetPlace(savedplace.Position);
            place.Id = savedplace.Id;
            place.Name = savedplace.Name;
            if (place is Village village)
            {
                foreach (var data in savedplace.Villagers)
                {
                    var vlg = VillagerController.Instance.CreateVillager(village, place.WorldPosition);
                    vlg.Id = data.Id;
                    vlg.Name = data.Name;
                    vlg.Age = data.Age;
                    vlg.Dead = data.Dead;
                    vlg.Tier = village.Tier;
                    village.Villagers.Add(vlg);
                }
                village.SetTier(savedplace.Tier);
                for (int i = 0; i < savedplace.Req.Count; i++)
                {
                    village.SetRequirement(savedplace.Req[i].Stats, savedplace.Req[i].CurrentAmount);
                }
            }
        }
        //Load works
        foreach (var savedPlace in save.Places)
        {
            var place = GameController.GetPlace(savedPlace.Position);
            if (place is WorkPlace workPlace)
            {
                foreach (var vlg in savedPlace.Villagers)
                {
                    var villager = VillagerController.GetVillager(vlg.Id);
                    if (villager == null)
                    {
                        GameConfig.ERROR = "Can't find id for villager at load";
                        Debug.Log("can't find id for villager load");
                        continue;
                    }
                    villager.Work = workPlace;
                    workPlace.AddWorker(villager);
                }
                if (place is SelectionWorkPlace sw)
                    sw.Selection = (sw.Card as SelectionCard).Selections.FirstOrDefault(t => t.Name == savedPlace.Extra);
                if (place is ProductionPlace pp)
                    pp.WorkPercent = savedPlace.WorkPercent;
                if (place is Mines mine)
                    mine.MineData = JsonUtility.FromJson<MineData>(savedPlace.Extra);

            }
        }
        Village.id = VillagerController.Villagers.Count + 1;
        Place.id = save.Places.Count + 1;
    }

    public void LoadResources(GameSave save)
    {
        foreach (var r in save.Stats)
            GameController.Stats.Set(r.Stats, r.Value);
    }

    public void LoadSave(string idendifier)
    {
        if (!PlayerPrefs.HasKey(idendifier)) return;
        var save = PlayerPrefs.GetString(idendifier);
        var game = JsonUtility.FromJson<GameSave>(save);
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        LoadGameData(game);
        LoadTiles(game);
        LoadPlaces(game);
        LoadResources(game);
        stopwatch.Stop();
        GoalController.Completed = true;
        savedText.text = "Loaded " + stopwatch.ElapsedMilliseconds + " ms";
        HideText();
    }
    public async UniTask LoadSaveRestartAsync(string idendifier)
    {
        await SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single).ToUniTask();
        LoadSave(idendifier);
    }

    private void LoadGameData(GameSave game)
    {
        var data = game.GameData;

        SeasonController.Instance.Day = data.Day;
        SeasonController.Instance.Year = data.Year;
        SeasonController.Instance.SetSeason(data.Season);

        CardController.Instance.UnlockedCards.Clear();
        foreach (var unlocked in data.Researched)
            CardController.Instance.Unlock(unlocked, true);

        foreach (var x in data.UnlockedExploration)
            ExplorationController.Instance.Unlock(x);

        GoalController.Completed = true;
    }


    private void WriteGameData(GameSave gameSave)
    {
        var data = new GameData();
        data.Day = SeasonController.Instance.Day;
        data.Season = SeasonController.Instance.CurrentSeason;
        data.Year = SeasonController.Instance.Year;
        data.Researched = CardController.Instance.UnlockedCards.Select(t => t.Type).ToList();
        data.UnlockedExploration.AddRange(ExplorationController.Instance.Foundables
        .Where(t => t.Collected).Select(t => t.Name));
        gameSave.GameData = data;

    }
    public void Save(string idendifier)
    {
        var st = new Stopwatch();
        st.Start();
        var gameSave = new GameSave();
        WriteTiles(gameSave);
        WritePlaces(gameSave);
        WriteResources(gameSave);
        WriteGameData(gameSave);
        var save = JsonUtility.ToJson(gameSave, Application.isEditor);
        PlayerPrefs.SetString(idendifier, save);
        PlayerPrefs.Save();
        st.Stop();
        savedText.text = "saved.." + st.ElapsedMilliseconds + " ms";
        HideText();
    }

    public async void HideText()
    {
        await UniTask.WaitForSeconds(4, ignoreTimeScale: true);
        savedText.text = "";
    }

}

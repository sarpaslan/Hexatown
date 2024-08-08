using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class WorkPlace : Place
{

    [UsefullInfo(readOnly: true)]
    public bool Pause;
    public abstract WorkTier Tier { get; }

    [UsefullInfo(readOnly: true)]
    public int NeededWorkerCount;

    [UsefullInfo(readOnly: true)]
    public int CurrentWorkerCount => Villagers.Count;

    [UsefullInfo(readOnly: true)]
    public float BonusProduction = 0;
    public List<Villager> Villagers = new List<Villager>();

    public override int Tax => -GameConfig.GetTierCost(Tier, CurrentWorkerCount);
    public virtual bool CanWork()
    {
        return PaidTax && CanWorkOnThisSeason() && !OnFire && HasEnoughWorker() && !Pause;
    }
    public bool CanWorkOnThisSeason()
    {
        return Card.Season.HasFlag(SeasonController.Instance.CurrentSeason);
    }

    public bool HasEnoughWorker()
    {
        return CurrentWorkerCount >= NeededWorkerCount;
    }
    public async virtual UniTask Work(Villager villager)
    {
        if (villager.IsMoving) return;
        if (villager.Spawned)
        {
            await Job.Move(villager, RandomPointInside());
            await Job.Despawn(villager, false);
        }
    }

    public void AddWorker(Villager villager)
    {
        Villagers.Add(villager);
        NotifyPropertyChange();
    }


    public WorkPlace(Card card, Vector3Int pos, int workerCount) : base(card, pos)
    {
        NeededWorkerCount = workerCount;
    }

    public override void OnPlaced(bool silent)
    {
        base.OnPlaced(silent);
        var bonus = IsPlacedIdeal() ? 10 : 0;
        foreach (var neighbour in Neighbours)
        {
            var n = TileMapController.Instance.Map.GetTile(neighbour);
            if (TileMapController.Instance.Placed.GetTile(neighbour) != null) continue;
            var biome = Biome.GetBiome((Tile)n);
            if (Card.IdealBiomeTypes.HasFlag(biome))
            {
                bonus += 5;
            }
        }
        BonusProduction = bonus;
        SeasonController.OnSeasonChanged += OnSeasonChanged;
    }

    public virtual void OnSeasonChanged(Season season)
    {
    }

    public void CancelWorkers()
    {
        for (int i = Villagers.Count - 1; i >= 0; i--)
        {
            Villager v = Villagers[i];
            if (v.IsBussy)
            {
                continue;
            }
            if (!v.Spawned)
            {
                v.Spawn();
            }
            v.CancelJobTokenSource.Cancel();
            v.CancelJobTokenSource = new CancellationTokenSource();
            v.RunningActions = false;
            NotifyPropertyChange();
        }
    }

    public override string GetDescription()
    {
        var str = base.GetDescription();

        if (BonusProduction > 0)
        {
            str += $"\n<color=grey>Environment: <color=green>+%{BonusProduction}</color></color>";
        }
        var names = "\n";
        for (int i = 0; i < Villagers.Count; i++)
        {
            if (i > 5) { names += "..."; break; }
            Villager v = Villagers[i];
            if (v != null)
                names += "\n" + v.Name + $"({v.Age})";
        }

        return str + names;
    }

    public void RemoveVillager(Villager v)
    {
        if (v != null)
        {
            v.Stop();
            v.Work = null;
            if (v.Spawned)
            {
                Job.Despawn(v, true).Forget();
            }
            Villagers.Remove(v);
            NotifyPropertyChange();
        }
    }
    public void RemoveVillagers()
    {
        for (int i = Villagers.Count - 1; i >= 0; i--)
        {
            Villager v = Villagers[i];
            RemoveVillager(v);
        }
    }

    public override void DestroyPlace(bool silent = false)
    {
        base.DestroyPlace();
        SeasonController.OnSeasonChanged -= OnSeasonChanged;
        foreach (var v in Villagers)
        {
            if (!v.Spawned)
            {
                v.Spawn();
            }
        }
        RemoveVillagers();
        Villagers.Clear();
    }
}

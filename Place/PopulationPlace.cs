using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RequiredStat
{
    public Stats Stats;
    public int RequireAmount;
    public int CurrentAmount;
}

public abstract class PopulationPlace : Place
{
    [UsefullInfo(readOnly: true)]
    public int Population => Villagers.Count;

    [UsefullInfo(readOnly: true)]
    public int MaxPopulation;
    public List<Villager> Villagers = new List<Villager>();
    public List<RequiredStat> Requirements = new List<RequiredStat>();

    public PopulationPlace(int maxPop, Card card, Vector3Int pos) : base(card, pos)
    {
        this.MaxPopulation = maxPop;
    }
    public override void DestroyPlace(bool silent = false)
    {
        base.DestroyPlace();

        for (int i = 0; i < Villagers.Count; i++)
        {
            Villager v = Villagers[i];
            v.Home = null;
            GameController.Instance.AssignNewHome(v);
        }
        Villagers.Clear();
        GameController.Stats.Add(Stats.MAXPOP, -MaxPopulation);
    }

    public override void OnPlaced(bool silent)
    {
        base.OnPlaced(silent);
        GameController.Stats.Add(Stats.MAXPOP, MaxPopulation);
    }

    public override string GetDescription()
    {
        var str = "";
        foreach (var v in Villagers)
        {
            if (v.Dead) continue;
            if (v.Age > GameConfig.WORKING_MAX_AGE)
            {
            }
            str += "\n" + v.Name + $"({v.Age})" + (v.Work != null ? " <color=green>Working</color>" : "") + (v.IsRetired ? "<color=yellow>Retired</color>" : "");
        }
        return base.GetDescription() + str;
    }

    public bool CanSpawn()
    {
        return true;
    }
}


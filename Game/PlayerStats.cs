using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public enum Stats
{
    NONE = -1,
    BALANCE = 999,
    POP = 0,
    MAXPOP = 1,
    REPUTATION = 2,
    WOOD = 3,
    GOLD = 4,
    WHEAT = 5,
    PORK = 6,
    CABBAGE = 7,
    TOMATO = 8,
    POTATO = 9,
    EGG = 10,
    FISH = 11,
    WOOL = 12,
    COAL = 13,
    CLOTHES = 14,
    BREAD = 15,
    SOAP = 16,
    TALLOW = 17,
    FLOUR = 18,
    CLAY = 19,
    BRICK = 20,
    MILK = 21,
    IRON_ORE = 22,
    GOLD_ORE = 23,
}

public class PlayerStats
{
    public int MaxStat = 250;
    private Dictionary<Stats, int> Stats;
    public UnityEvent<Stats> OnValueChanged = new();
    public PlayerStats()
    {
        Stats = new Dictionary<Stats, int>()
        {
            {global::Stats.WOOD, 0},
            {global::Stats.MAXPOP, 0},
            {global::Stats.POP, 0},
            {global::Stats.GOLD, 0},
        };
    }
    public List<KeyValuePair<Stats, int>> All => Stats.ToList();

    public void Set(Stats stat, int val)
    {
        EnsureStat(stat);
        var crt = Stats[stat];
        if (crt != val)
        {
            Stats[stat] = val;
            EnsureValue(stat);
            OnValueChanged?.Invoke(stat);
        }
    }
    public bool Purchase(int price)
    {
        int coin = Stats[global::Stats.GOLD];
        if (coin >= price)
        {
            coin -= price;
            Stats[global::Stats.GOLD] = coin;
            OnValueChanged?.Invoke(global::Stats.GOLD);
            return true;
        }
        return false;
    }
    public int Get(Stats stat)
    {
        EnsureStat(stat);
        return Stats[stat];
    }

    public bool Spend(Stats stat, int value)
    {
        var st = Get(stat) - value;
        if (st >= 0)
        {
            Set(stat, st);
            return true;
        }
        return false;
    }
    public void Add(Stats stat, int v)
    {
        EnsureStat(stat);
        Stats[stat] += v;
        EnsureValue(stat);
        OnValueChanged?.Invoke(stat);

    }

    private void EnsureStat(Stats st)
    {
        if (Stats.ContainsKey(st)) return;
        Stats.Add(st, 0);
    }
    public bool IsGlobalStat(Stats st)
    {
        if (st == global::Stats.GOLD || st == global::Stats.POP || st == global::Stats.MAXPOP || st == global::Stats.BALANCE)
            return true;
        return false;
    }

    private void EnsureValue(Stats st)
    {
        if (IsGlobalStat(st)) return;

        var val = Stats[st];
        if (val > MaxStat)
        {
            Stats[st] = MaxStat;
        }
    }
}

using System.Linq;
using UnityEngine;

public class Village : PopulationPlace
{
    public VillageCard m_villageCard;
    private WorkTier m_tier;
    public WorkTier Tier
    {
        get
        {
            return m_tier;
        }
        set
        {
            if (m_tier == value)
                return;
            m_tier = value;
            SetTile(value);
        }
    }

    public override string GetDescription()
    {
        return $"{Tier.ToReadableString()}'s live here. \n" + base.GetDescription();
    }

    public override int Tax => GameConfig.GetTax(m_tier) * Villagers.Count;

    private void SetTile(WorkTier value)
    {
        var tile = m_villageCard.Tiles.First(t => t.Tier == value);
        TileMapController.Instance.SetTile(Position, tile.Tiles[Random.Range(0, tile.Tiles.Length - 1)]);
    }

    public Village(Card card, Vector3Int pos) : base(4, card, pos)
    {
        this.m_villageCard = card as VillageCard;
    }

    public void Upgrade()
    {
        Tier++;
        GameController.Resources.SoundPlayer.Play("upgrade");
        SetTier(Tier);
    }

    public void SetTier(WorkTier tier)
    {
        Tier = tier;
        NotifyPropertyChange();
        foreach (var v in Villagers)
        {
            v.Work?.RemoveVillager(v);
            v.Stop();
            v.Tier = Tier;
        }
        SetRequirements();
    }
    public override void OnBeforePlace(bool silent)
    {
        SetTile(m_tier);
        SetRequirements();
    }

    public override void OnTick()
    {
        base.OnTick();
        if (Tick % GameConfig.VILLAGER_REQUEST_NEED_TICK == 0)
        {
            RequestFood();
        }
        if (Tick % GameConfig.VILLAGER_REQEST_CONSUME_TICK == 0)
        {
            EatFood();
            RequestFood();
        }
    }

    private void EatFood()
    {
        foreach (var stats in Requirements)
        {
            if (stats.CurrentAmount > 0)
            {
                stats.CurrentAmount--;
            }
        }
        var foodLevel = GameConfig.GetFoodLevel(this);
        if (foodLevel < 2)
        {
            foreach (var v in Villagers)
            {
                var max = 10;
                if (v.Work != null)
                    max = 18;
                v.Hunger -= UnityEngine.Random.Range(5, max);
                if (v.Hunger <= 0)
                {
                    VillagerController.Instance.Kill(v, "Hunger", true);
                }
            }
        }
        else
        {
            foreach (var v in Villagers)
            {
                v.Hunger += UnityEngine.Random.Range(4, 12);
                if (v.Hunger > 100)
                    v.Hunger = 100;
            }
        }
    }

    public void RequestFood()
    {
        foreach (var stats in Requirements)
        {
            if (stats.CurrentAmount < stats.RequireAmount)
            {
                if (GameController.Stats.Spend(stats.Stats, 1))
                {
                    stats.CurrentAmount++;
                }
            }
        }
        NotifyPropertyChange();
    }

    public void SetRequirements()
    {
        Requirements.Clear();
        switch (Tier)
        {
            case WorkTier.FARMER:
                Requirements.Add(new RequiredStat()
                {
                    CurrentAmount = 0,
                    RequireAmount = 6,
                    Stats = Stats.FISH,
                });
                Requirements.Add(new RequiredStat()
                {
                    CurrentAmount = 0,
                    RequireAmount = 6,
                    Stats = Stats.POTATO,
                });
                Requirements.Add(new RequiredStat()
                {
                    CurrentAmount = 0,
                    RequireAmount = 6,
                    Stats = Stats.CLOTHES,
                });
                break;
            case WorkTier.WORKER:
                Requirements.Add(new RequiredStat()
                {
                    CurrentAmount = 8,
                    RequireAmount = 8,
                    Stats = Stats.POTATO,
                });
                Requirements.Add(new RequiredStat()
                {
                    CurrentAmount = 0,
                    RequireAmount = 8,
                    Stats = Stats.BREAD
                });
                Requirements.Add(new RequiredStat()
                {
                    CurrentAmount = 8,
                    RequireAmount = 8,
                    Stats = Stats.CLOTHES,
                });
                Requirements.Add(new RequiredStat()
                {
                    CurrentAmount = 0,
                    RequireAmount = 8,
                    Stats = Stats.SOAP,
                });
                Requirements.Add(new RequiredStat()
                {
                    CurrentAmount = 8,
                    RequireAmount = 8,
                    Stats = Stats.FISH,
                });
                break;
            case WorkTier.ARTISAN:
                Requirements.Add(new RequiredStat()
                {
                    CurrentAmount = 8,
                    RequireAmount = 10,
                    Stats = Stats.BREAD
                });
                Requirements.Add(new RequiredStat()
                {
                    CurrentAmount = 8,
                    RequireAmount = 10,
                    Stats = Stats.CLOTHES,
                });
                Requirements.Add(new RequiredStat()
                {
                    CurrentAmount = 8,
                    RequireAmount = 10,
                    Stats = Stats.SOAP,
                });
                Requirements.Add(new RequiredStat()
                {
                    CurrentAmount = 8,
                    RequireAmount = 10,
                    Stats = Stats.FISH,
                });
                Requirements.Add(new RequiredStat()
                {
                    CurrentAmount = 0,
                    RequireAmount = 20,
                    Stats = Stats.EGG,
                });
                Requirements.Add(new RequiredStat()
                {
                    CurrentAmount = 0,
                    RequireAmount = 50,
                    Stats = Stats.COAL,
                });
                break;
        }
    }

    public bool CanUpgrade()
    {
        if (Tier == WorkTier.ARTISAN)
        {
            return false;
        }
        if (OnFire) return false;
        return Requirements.TrueForAll(t => t.CurrentAmount >= t.RequireAmount);
    }

    public void SetRequirement(Stats stats, int currentAmount)
    {
        for (int i = 0; i < Requirements.Count; i++)
            if (Requirements[i].Stats == stats)
                Requirements[i].CurrentAmount = currentAmount;
    }
}


using System;
using System.Linq;
using UnityEngine;

public class Castle : Place
{
    public int Depot = 250;
    public Castle(Card card, Vector3Int pos) : base(card, pos)
    {
    }

    private int m_tier;
    public int Tier
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

    public override int Tax => 0;

    private void SetTile(int index)
    {
        var tile = ((CastleCard)Card).TileSteps[index];
        TileMapController.Instance.SetTile(Position, tile);
    }

    public override void OnBeforePlace(bool silent)
    {
        SetTile(m_tier);
        SeasonController.Instance.StartSeason();
        GiveInitialResources();
    }

    private void GiveInitialResources()
    {
        //TODO remove later.
    }

    public override void OnTick()
    {
        base.OnTick();
        if (Tick % GameConfig.TAX_TICK == 0)
        {
            var allTax = 0;
            var places = GameController.Places.OrderBy(t => t.Card.Type == PlaceType.VILLAGE).ToList();
            for (int i = places.Count - 1; i >= 0; i--)
            {
                Place p = places[i];
                if (p.Tax == 0) continue;
                if (p.Tax > 0)
                {
                    p.PaidTax = true;
                    GameController.Stats.Add(Stats.GOLD, p.Tax);
                }
                else
                {
                    var gold = GameController.Stats.Get(Stats.GOLD);
                    gold += p.Tax;
                    if (gold >= 0)
                    {
                        p.PaidTax = true;
                        GameController.Stats.Set(Stats.GOLD, gold);
                    }
                    else
                    {
                        p.PaidTax = false;
                    }
                }
                allTax += p.Tax;
            }
            GameController.Stats.Set(Stats.BALANCE, allTax);
        }
        NotifyPropertyChange();
    }

    public void Upgrade()
    {
        Tier++;
        UpdateDepot();
        GameController.Resources.SoundPlayer.Play("upgrade");
        NotifyPropertyChange();
        PlayerStatsView.Instance.Start();
    }

    internal void UpdateDepot()
    {
        GameController.Stats.MaxStat = (Tier + 1) * Depot;
        GameController.Stats.OnValueChanged?.Invoke(Stats.WOOD);
    }
}


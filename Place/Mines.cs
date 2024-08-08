using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MineData
{
    public int IronChance;
    public int GoldChance;
    public int CoalChance;
    public int Coal;
    public int Iron;
    public int Gold;
    public override string ToString()
    {
        return JsonUtility.ToJson(this, Application.isEditor);
    }
}
public class Mines : ProductionPlace
{
    public Mines(Card card, Vector3Int pos) :
    base(card, pos, 10, 1, 15)
    {
    }
    public Stats CurrentProduce;
    public override Sprite RequiresIcon => SpriteIcon.Error.ToSprite();

    public override Stats Requires => Stats.NONE;

    public override Stats Produce => CurrentProduce;

    public override WorkTier Tier => WorkTier.WORKER;

    public MineData MineData;

    public override void OnPlaced(bool silent)
    {
        base.OnPlaced(silent);
        if (MineData == null)
        {
            MineData = new MineData();
            MineData.GoldChance = Random.Range(0, 8);
            int remainingChance = 100 - MineData.GoldChance;
            MineData.CoalChance = Random.Range(80, remainingChance);
            MineData.IronChance = remainingChance - MineData.CoalChance;
        }
        SelectRandom();

        switch (BiomeType)
        {
            case BiomeType.RedDesertMountain:
                BonusProduction = 50;
                break;
            case BiomeType.YellowDesertMountain:
                BonusProduction = 40;
                break;
            case BiomeType.Mountains:
                BonusProduction = 20;
                break;
        }
    }
    public void SelectRandom()
    {
        var range = Random.Range(0, 100);
        if (range < MineData.CoalChance)
        {
            CurrentProduce = Stats.COAL;
        }
        else if (range < MineData.CoalChance + MineData.IronChance)
        {
            CurrentProduce = Stats.IRON_ORE;
        }
        else
        {
            CurrentProduce = Stats.GOLD_ORE;
        }
    }

    public override async UniTask Work(Villager villager)
    {
        if (villager.IsMoving) return;
        if (villager.Spawned)
        {
            await Job.Move(villager, RandomPointInside());
            await Job.Despawn(villager, false);
        }
        ReadyForWork = CanWork();
    }

    public override void OnCompletedProduction()
    {
        GameController.Stats.Add(Produce, ProduceAmount);
        if (Produce == Stats.COAL)
            MineData.Coal += ProduceAmount;
        else if (Produce == Stats.IRON_ORE)
            MineData.Iron += ProduceAmount;
        else if (Produce == Stats.GOLD_ORE)
            MineData.Gold += ProduceAmount;

        ReadyForWork = true;
        WorkPercent = 0;
        GameScreen.PopupItem(WorldPosition, this.Produce);
        SelectRandom();
    }
}
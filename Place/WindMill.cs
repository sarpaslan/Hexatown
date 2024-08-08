using Cysharp.Threading.Tasks;
using UnityEngine;

public class WindMill : ProductionPlace
{
    public bool AutomaticGather;

    public WindMill(Card card, Vector3Int pos)
    : base(card, pos, 4, 20, 20)
    {

    }

    public override WorkTier Tier => WorkTier.FARMER;

    public override Sprite RequiresIcon => SpriteIcon.Wheat.ToSprite();

    public override Stats Requires => Stats.WHEAT;

    public override Stats Produce => Stats.FLOUR;

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

    public override string GetDescription()
    {
        var str = base.GetDescription();
        return str;
    }

    public override void OnPlaced(bool silent)
    {
        base.OnPlaced(silent);
        RefreshFarmLands();
    }
    public override void OnCompletedProduction()
    {
        GameController.Stats.Add(Produce, ProduceAmount);
        GameScreen.PopupItem(WorldPosition, this.Produce);
        ReadyForWork = true;
        WorkPercent = 0;
    }

    private void RefreshFarmLands()
    {
        var farms = GameController.GetPlaces<FarmLand>();
        foreach (var farm in farms)
        {
            farm.SetWindMill(this);
        }
    }

    public override void DestroyPlace(bool silent = false)
    {
        base.DestroyPlace();
        var farms = GameController.GetPlaces<FarmLand>();
        foreach (var farm in farms)
        {
            if (farm.WindMill == this)
            {
                farm.WindMill = null;
            }
        }
    }

    public override string GetName()
    {
        return Card.Name;
    }
}
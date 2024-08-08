using Cysharp.Threading.Tasks;
using UnityEngine;

public class ClayQuarry : ProductionPlace
{
    public ClayQuarry(Card card, Vector3Int pos) :
    base(card, pos, 10, 1, 30)
    {
    }
    public override Sprite RequiresIcon => SpriteIcon.Pickaxe.ToSprite();

    public override Stats Requires => Stats.NONE;
    public override Stats Produce => Stats.CLAY;

    public override WorkTier Tier => WorkTier.WORKER;
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
        ReadyForWork = true;
        WorkPercent = 0;
        GameScreen.PopupItem(WorldPosition, Produce);
    }
}
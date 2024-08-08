
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Baker : ProductionPlace
{
    public Baker(Card card, Vector3Int pos)
    : base(card, pos, 4, 20, 10)
    {
    }

    public override Sprite RequiresIcon => SpriteIcon.Flour.ToSprite();

    public override Stats Requires => Stats.FLOUR;

    public override Stats Produce => Stats.BREAD;

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

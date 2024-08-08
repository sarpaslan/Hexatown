using Cysharp.Threading.Tasks;
using UnityEngine;

public class ClayFactory : ProductionPlace
{
    public ClayFactory(Card card, Vector3Int pos) :
    base(card, pos, 4, 10, 5)
    {
    }
    public override Sprite RequiresIcon => SpriteIcon.Clay.ToSprite();

    public override Stats Requires => Stats.CLAY;

    public override Stats Produce => Stats.BRICK;

    public override WorkTier Tier => WorkTier.ARTISAN;
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
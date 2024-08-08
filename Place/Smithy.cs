using Cysharp.Threading.Tasks;
using UnityEngine;

public class Smithy : SelectionWorkPlace
{
    public Smithy(Card card, Vector3Int pos) : base(card, pos, 5, 5, 10)
    {

    }

    public override Sprite RequiresIcon => SpriteIcon.Coal.ToSprite();

    public override Stats Requires => Stats.COAL;

    public override Stats Produce => Stats.COAL;

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
        GameScreen.PopupItem(WorldPosition, this.Produce);
    }
}

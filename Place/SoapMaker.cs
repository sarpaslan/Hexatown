using Cysharp.Threading.Tasks;
using UnityEngine;

public class SoapMaker : ProductionPlace
{
    public SoapMaker(Card card, Vector3Int pos) : base(card, pos, 5, 5, 10)
    {

    }

    public override Sprite RequiresIcon => SpriteIcon.Tallow.ToSprite();

    public override Stats Requires => Stats.TALLOW;

    public override Stats Produce => Stats.SOAP;

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
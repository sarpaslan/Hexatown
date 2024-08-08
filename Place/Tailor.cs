using Cysharp.Threading.Tasks;
using UnityEngine;

public class Tailor : ProductionPlace
{
    public Tailor(Card card, Vector3Int pos) : base(card, pos, 4, 6, 12)
    {
    }
    public override Sprite RequiresIcon => SpriteIcon.SheepProduct.ToSprite();

    public override Stats Requires => Stats.WOOL;

    public override Stats Produce => Stats.CLOTHES;

    public override WorkTier Tier => WorkTier.FARMER;
    public override async UniTask Work(Villager villager)
    {
        if (villager.IsMoving) return;
        if (villager.Spawned)
        {
            await Job.Move(villager, RandomPointInside());
            await Job.Despawn(villager, false);
        }
        if (CanWork())
            ReadyForWork = true;
    }

    public override void OnCompletedProduction()
    {
        WorkPercent = 0;
        ReadyForWork = true;
        GameController.Stats.Add(Produce, ProduceAmount);
        GameScreen.PopupItem(WorldPosition, this.Produce);
    }
}

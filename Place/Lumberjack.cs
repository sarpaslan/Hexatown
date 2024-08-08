
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Lumberjack : ProductionPlace
{
    public int LastCollectionAmount;

    public override Sprite RequiresIcon => GameController.Resources.GetIcon(SpriteIcon.Tree);

    public override Stats Requires => Stats.NONE;

    public override Stats Produce => Stats.WOOD;

    public override WorkTier Tier => WorkTier.FARMER;
    public Lumberjack(Card card, Vector3Int pos) :

    base(card, pos, 1, 1, 24)
    {
    }

    public override bool CanWork()
    {
        return base.CanWork() && HasEnoughWorker();
    }
    public override string GetDescription()
    {
        var description = base.GetDescription();
        if (LastCollectionAmount != 0)
        {
            description += $"<color=grey> last collected ({LastCollectionAmount}) wood</color>";
        }
        return description;
    }

    public override async UniTask Work(Villager villager)
    {
        if (!CanWorkOnThisSeason())
        {
            if (villager.Spawned)
            {
                await Job.Despawn(villager, true);
            }
        }
        IsInventoryFull = true;
        if (!CanWork())
        {
            return;
        }

        if (!villager.Spawned)
            villager.Spawn();

        await Job.Move(villager, RandomPointInside());
        await Job.WaitUnscaledSeconds(villager, 1f);
        ReadyForWork = true;
        await Job.Cut(villager);
        while (WorkPercent < 100 && ReadyForWork)
        {
            if (!CanWork())
                break;
            float distance = Vector3.Distance(GameController.Camera.transform.position, WorldPosition);
            float volume = Mathf.Clamp(1 - (distance / 8), 0f, 0.5f);
            volume *= Mathf.Clamp01(1 / GameController.Camera.orthographicSize);
            if (volume > 0)
            {
                GameController.Resources.SoundPlayer.Play("cut", volume);
                await Job.WaitSeconds(villager, 0.20f);
            }
            await Job.WaitSeconds(villager, 1f);
        }
        await Job.Idle(villager);
    }
    public override void OnCompletedProduction()
    {
        WorkPercent = 0;
        GameController.Stats.Add(this.Produce, this.ProduceAmount);
        GameScreen.PopupItem(WorldPosition, this.Produce);
    }
}
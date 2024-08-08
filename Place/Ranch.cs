using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;


public class Ranch : SelectionWorkPlace
{
    private Villager m_workingVillager;

    public override Sprite RequiresIcon => Selection == null ?
    SpriteIcon.QuestionMarkIcon.ToSprite() : Selection.Icon;


    public override string GetDescription()
    {
        return base.GetDescription() + "\nActive: " + m_workingVillager?.Name;
    }
    public override void OnPlaced(bool silent)
    {
        base.OnPlaced(silent);
    }

    public override Stats Requires => Stats.NONE;
    public override Stats Produce => Selection == null ? Stats.NONE : Selection.Stats;
    public override WorkTier Tier => WorkTier.FARMER;

    public Ranch(Card card, Vector3Int pos) : base(card, pos, 3, 1, 10)
    {
    }
    public override Sprite GetSprite()
    {
        return Card.Tile.sprite;
    }

    public override async UniTask Work(Villager villager)
    {
        if (!CanWorkOnThisSeason())
        {
            if (villager.Spawned)
            {
                await Job.Despawn(villager, true);
            }
            if (m_selectionSprite != null)
            {
                GameObject.Destroy(m_selectionSprite.gameObject);
                m_selectionSprite = null;
            }
        }

        if (m_workingVillager != null)
            if (!m_workingVillager.Spawned || m_workingVillager.Dead)
                m_workingVillager = null;

        if (m_workingVillager == null)
            m_workingVillager = villager;

        IsInventoryFull = true;

        if (!CanWork()) return;

        if (m_workingVillager != villager)
        {
            if (villager.Spawned)
                await Job.Despawn(villager, false);
            return;
        }

        if (!villager.Spawned)
            villager.Spawn();

        await Job.Move(villager, RandomPointInside());
        ReadyForWork = true;
        await Job.Jump(villager);
        StartJumpingRandomPoint();
        await Job.Move(villager, RandomPointInside());
    }

    public override void OnCompletedProduction()
    {
        GameController.Stats.Add(this.Produce, this.ProduceAmount);
        GameScreen.PopupItem(WorldPosition, this.Produce);
        ReadyForWork = true;
        WorkPercent = 0;
    }

    void StartJumpingRandomPoint()
    {
        Vector3 randomPoint = RandomPointInside();
        float jumpPower = 0.1f;
        int numJumps = 4;
        float duration = 2f;
        if (m_selectionSprite == null) return;
        m_selectionSprite.transform.DOJump(randomPoint, jumpPower, numJumps, duration).SetEase(Ease.Linear);
    }

    public override void UpdateSprites()
    {
        base.UpdateSprites();
        UpdateBonus();
    }
    protected override void OnSelectionSpriteCreated()
    {
        StartJumpingRandomPoint();
    }

    private void UpdateBonus()
    {
        var bonus = 0;
        if (Selection != null)
        {
            var ranchCard = (RanchCard)Card;
            var biome = ranchCard.Biomes.First(t => t.Name == Selection.Name);
            if (biome.Biome.HasFlag(BiomeType))
            {
                bonus += 20;
            }
        }
        BonusProduction = bonus;
    }
}
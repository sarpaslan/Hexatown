using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DeadVillagerInfo
{
    public string Name;
    public string DeadReason;
    public short Age;
}

public class Cemetery : WorkPlace
{
    public int Population => DeadVillagerInfos.Count;

    public override WorkTier Tier => WorkTier.WORKER;

    public List<DeadVillagerInfo> DeadVillagerInfos = new List<DeadVillagerInfo>();
    public CemeteryCard m_cemeteryCard;
    private SpriteRenderer m_renderer;
    public Cemetery(Card card, Vector3Int pos) : base(card, pos, 2)
    {
        m_cemeteryCard = card as CemeteryCard;
    }

    public override void OnTick()
    {
        base.OnTick();
    }

    public override async UniTask Work(Villager villager)
    {
        if (!CanWork()) return;

        if (villager.IsBussy)
            return;


        Villager m_villagerToBurry = null;
        for (int i = 0; i < VillagerController.Villagers.Count; i++)
        {
            var vlg = VillagerController.Villagers[i];
            if (vlg.Dead && !vlg.BurryOrder && !vlg.Buried)
            {
                vlg.BurryOrder = true;
                m_villagerToBurry = vlg;
                break;
            }
        }
        if (m_villagerToBurry == null || Population > m_cemeteryCard.MaxPopulation)
        {
            if (villager.Spawned)
            {
                await Job.Move(villager, RandomPointInside());
                await Job.Despawn(villager, false);
            }
            return;
        }

        try
        {

            if (!villager.Spawned)
                villager.Spawn();

            villager.IsBussy = true;
            {
                await Job.CarryEnd(villager);
                await Job.Move(villager, m_villagerToBurry.Transform.position);
                m_villagerToBurry.Buried = true;
                await Job.Carry(villager, GameController.Resources.GetIcon(SpriteIcon.Villager));
                await Job.Move(villager, RandomPointInside());
                await Job.CarryEnd(villager);
                await Job.Build(villager);
                await Job.Idle(villager);
                AddVillager(villager);
            }
            villager.IsBussy = false;
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException)
            {

            }
            else
            {
                villager.IsBussy = false;
            }
        }
    }

    public void AddVillager(Villager villager)
    {
        DeadVillagerInfos.Add(new DeadVillagerInfo()
        {
            Name = villager.Name,
            Age = villager.Age,
            DeadReason = villager.DeadReason
        });
        if (m_renderer == null)
        {
            m_renderer = GameObject.Instantiate(m_cemeteryCard.Layer);
            m_renderer.transform.position =
            new Vector3(WorldPosition.x, WorldPosition.y - GameConfig.MAGIC_Y, WorldPosition.z);
        }
        m_renderer.sprite = GetSpriteBasedOnPopulation();
        NotifyPropertyChange();
    }

    private UnityEngine.Sprite GetSpriteBasedOnPopulation()
    {
        float ratio = (float)Population / m_cemeteryCard.MaxPopulation;
        int index = Mathf.Clamp((int)(ratio * m_cemeteryCard.Sprites.Length), 0, m_cemeteryCard.Sprites.Length - 1);
        return m_cemeteryCard.Sprites[index];
    }

    public override void DestroyPlace(bool silent = false)
    {
        base.DestroyPlace();

        if (m_renderer != null)
        {
            UnityEngine.Object.Destroy(m_renderer.gameObject);
            m_renderer = null;
        }
    }
    public override void OnDeselected()
    {
        base.OnDeselected();
    }
    public override string GetDescription()
    {
        return base.GetDescription() + $"\n{Population} / {m_cemeteryCard.MaxPopulation}";
    }
}

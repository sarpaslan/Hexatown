using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class FishingDock : ProductionPlace
{
    public FishingDockCard m_fishingCard;
    public FishingDock(Card card, Vector3Int pos) : base(card, pos, 5, 1, 20)
    {
        m_fishingCard = card as FishingDockCard;
    }
    public List<Vector3> m_waterAreas = new List<Vector3>();
    public VillagerBoatBehaviour m_boat;

    public override WorkTier Tier => WorkTier.FARMER;

    public override Sprite RequiresIcon => SpriteIcon.Water.ToSprite();

    public override Stats Requires => Stats.NONE;

    public override Stats Produce => Stats.FISH;

    public override void OnPlaced(bool silent)
    {
        base.OnPlaced(silent);
        foreach (var p in Neighbours)
        {
            if (TileMapController.Instance.Foreground.HasTile(p))
            {
                continue;
            }
            TileMapController.BlockedBackgroundPositions.Add(p);
            m_waterAreas.Add(TileMapController.Instance.Placed.CellToWorld(p));
        }
    }
    public override void OnTick()
    {
        base.OnTick();
        if (m_boat == null) return;
        if (m_boat.State == BoatState.IDLE && CanWork())
        {
            IsInventoryFull = true;
            var randomPos = m_waterAreas.SelectRandom();
            var targetPos = LittleRandom.XY(randomPos, 0.4f);
            if (!m_boat.Spawned)
            {
                m_boat.Spawned = true;
                m_boat.transform.localScale = Vector3.zero;
                m_boat.transform.position = WorldPosition;
                while (Vector3.Distance(m_boat.transform.position, targetPos) > 1f)
                {
                    m_boat.transform.position = Vector3.MoveTowards(m_boat.transform.position, targetPos, 0.01f);
                }
                m_boat.transform.DOScale(0.8f, 1f);
            }
            m_boat.SetTarget(targetPos);
        }
        if (this.m_boat.Villager.Dead)
        {
            Object.Destroy(m_boat.gameObject);
            this.m_boat = null;
        }
    }

    public override async UniTask Work(Villager villager)
    {
        IsInventoryFull = true;
        if (villager.Spawned)
        {
            await Job.Move(villager, RandomPointInside());
            await Job.Despawn(villager, false);
        }
        if (m_boat != null)
        {
            if (m_boat.State != BoatState.DROPPING)
                ReadyForWork = true;
            return;
        }

        villager.IsBussy = true;
        var boatBehaviour = Object.Instantiate(m_fishingCard.BoatPrefab);
        boatBehaviour.transform.localScale = Vector3.zero;
        boatBehaviour.Dock = this;
        boatBehaviour.Villager = villager;
        boatBehaviour.PossiblePositions = m_waterAreas;
        m_boat = boatBehaviour;
        ReadyForWork = true;
    }

    public override void DestroyPlace(bool silent = false)
    {
        base.DestroyPlace();
        foreach (var p in Neighbours)
        {
            if (TileMapController.Instance.Placed.HasTile(p))
            {
                continue;
            }
            TileMapController.BlockedBackgroundPositions.Remove(p);
        }
        if (m_boat != null)
        {
            Object.Destroy(m_boat.gameObject);
            m_boat = null;
        }
    }
}

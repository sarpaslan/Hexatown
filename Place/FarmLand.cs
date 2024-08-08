using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

public class FarmLand : SelectionWorkPlace
{
    private FarmCard m_farmCard => Card as FarmCard;
    public FarmLand(Card card, Vector3Int pos) : base(card, pos, 1, 1, 2)
    {
    }
    public WindMill WindMill;
    private float m_distanceToWindMill = float.MaxValue;
    [UsefullInfo(readOnly: true)]
    public bool Collecting;
    public override UnityEngine.Sprite RequiresIcon => Selection == null ? null : Selection.Icon;
    public override Stats Requires => Stats.NONE;
    public override Stats Produce => Selection == null ? Stats.NONE : Selection.Stats;
    public override WorkTier Tier => WorkTier.FARMER;

    public override void OnPlaced(bool silent)
    {
        base.OnPlaced(silent);
        FindWindMill();
    }

    public override void UpdateSprites()
    {
        base.UpdateSprites();
        if (Selection != null && CanWorkOnThisSeason())
        {
            TileMapController.Instance.SetTile(Position, m_farmCard.EmptyTile);
        }
        else
        {
            TileMapController.Instance.SetTile(Position, m_farmCard.Tile);
        }
    }

    public override void OnSeasonChanged(Season season)
    {
        if (!CanWorkOnThisSeason())
        {
            CancelWorkers();
            WorkPercent = 0;
            ReadyForWork = false;
        }
    }

    public async override UniTask Work(Villager villager)
    {
        IsInventoryFull = true;

        if (!CanWorkOnThisSeason())
        {
            if (villager.Spawned)
                await Job.Despawn(villager, true);
            return;
        }

        if (!villager.Spawned)
            villager.Spawn();


        if (!villager.IsMoving && villager.Spawned)
            await Job.Move(villager, RandomPointInside());


        if (!CanWork())
            return;


        if (!ReadyForWork && WorkPercent <= 0)
            Collecting = false;

        if (!Collecting)
        {
            ReadyForWork = true;
            while (WorkPercent < 100 && CanWork())
            {
                if (Pause)
                {
                    await Job.WaitSeconds(villager, 0.5f);
                    continue;
                }
                await Job.Build(villager);
                await Job.WaitSeconds(villager, 0.5f);
                await Job.Move(villager, RandomPointInside());
            }
            if (CanWork())
            {
                Collecting = true;
                ReadyForWork = false;
            }
        }
        else
        {
            Gather();
            await Job.Carry(villager, Selection.Stats.ToSprite());
            var targetPos = GameController.Castle.WorldPosition;
            if (WindMill != null)
            {
                targetPos = WindMill.WorldPosition;
            }
            await Job.Move(villager, targetPos);
            await Job.CarryEnd(villager);
            var collected = this.ProduceAmount;
            collected += (int)(collected * BonusProduction / 100f);
            GameController.Stats.Add(this.Produce, collected);
            GameScreen.PopupItem(villager.Transform.position, this.Produce);
        }
    }

    public void Gather()
    {
        if (WorkPercent <= 0)
            return;
        WorkPercent -= 20;
        NotifyPropertyChange();
    }

    public SpriteRenderer m_windMillHighlight;

    public override void OnSelect()
    {
        base.OnSelect();
        if (WindMill != null)
        {
            if (m_windMillHighlight == null)
            {
                m_windMillHighlight = TileSelectionController.Instance.Highlight(WindMill.Position, new Color(0, 1, 0, 0.2f));
            }
        }
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
        if (m_windMillHighlight != null)
        {
            Object.Destroy(m_windMillHighlight.gameObject);
            m_windMillHighlight = null;
        }
    }

    public void FindWindMill()
    {
        if (this.WindMill != null)
            return;

        var closestWindMill = GameController.GetClosestPlace<WindMill>(WorldPosition);
        if (closestWindMill == null) return;
        SetWindMill(closestWindMill);
    }
    public void SetWindMill(WindMill windMill)
    {
        if (windMill == null)
            return;
        if (this.WindMill == null)
        {
            this.WindMill = windMill;
        }
        else if (this.m_distanceToWindMill > Vector3.Distance(WorldPosition, windMill.WorldPosition))
        {
            this.WindMill = windMill;
        }
        if (this.WindMill == windMill)
            m_distanceToWindMill = Vector3.Distance(WorldPosition, windMill.WorldPosition);

        if (Vector3.Distance(WorldPosition, this.WindMill.WorldPosition) > Vector3.Distance(GameController.Castle.WorldPosition, WorldPosition))
            this.WindMill = null;
    }
}
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum BoatState
{
    IDLE,
    MOVING,
    DROPPING,
    GOING_DOCK,
    FISHING,
}

public class VillagerBoatBehaviour : VillagerBehaviour
{
    public bool Spawned;
    public FishingDock Dock;
    public SpriteRenderer VillagerSprite;
    public float Speed = 1;
    public BoatState State = BoatState.IDLE;
    public Vector3 Target;
    public static int FishDropTick = 2;
    public bool Collecting;

    public void SetTarget(Vector3 newTarget)
    {
        Target = newTarget;
        State = BoatState.MOVING;
        if (Target.x >= transform.position.x)
        {
            transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        }
        else
        {
            transform.localScale = new Vector3(-0.8f, 0.8f, 0.8f);
        }
    }
    void Start()
    {
        transform.DORotate(new Vector3(0, 0, 10), 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    public override string ToString()
    {
        var str = State == BoatState.DROPPING ? "Offloading" : "Fishing";
        return base.ToString() + $"\n<color=grey>{str}({(int)Dock.WorkPercent}%)</color>";
    }

    public List<Vector3> PossiblePositions;
    private float m_prevTime;

    public override void Update()
    {
        base.Update();
        switch (State)
        {
            case BoatState.IDLE:
                Dock.OnRequest();
                break;
            case BoatState.MOVING:
                if (Dock.WorkPercent >= 100)
                {
                    Dock.ReadyForWork = false;
                    State = BoatState.GOING_DOCK;
                    return;
                }
                if (!Dock.CanWork())
                {
                    return;
                }
                if (Vector3.Distance(Target, Dock.WorldPosition) < 1.27)
                {
                    transform.DOPunchPosition(Vector3.one, 0.1f);
                }
                transform.position = Vector3.MoveTowards(transform.position, Target, Time.deltaTime * Speed);
                break;
            case BoatState.GOING_DOCK:
                if (Vector3.Distance(transform.position, Dock.WorldPosition) > GameConfig.MAGIC_Y + 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, Dock.WorldPosition, Time.deltaTime * Speed);
                }
                else
                {
                    State = BoatState.DROPPING;
                }
                break;
            case BoatState.DROPPING:
                Dock.ReadyForWork = false;
                VillagerSprite.gameObject.SetActive(false);
                if (m_prevTime == 0)
                {
                    m_prevTime = Time.time + 0.5f;
                }
                if (Time.time > m_prevTime)
                {
                    m_prevTime = Time.time + 0.5f;
                    Dock.WorkPercent -= 3;
                    Dock.NotifyPropertyChange();
                    if (Dock.WorkPercent <= 0)
                    {
                        Dock.ReadyForWork = true;
                        GameController.Stats.Add(Dock.Produce, Dock.ProduceAmount);
                        GameScreen.PopupItem(Dock.WorldPosition, Dock.Produce);
                        State = BoatState.IDLE;
                        m_prevTime = 0;
                        VillagerSprite.gameObject.SetActive(true);
                    }
                }
                break;
            default:
                break;
        }
    }
}


using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
[Serializable]
public enum WorkTier
{
    FARMER,
    WORKER,
    ARTISAN,
}

public class Villager
{
    [UsefullInfo(readOnly: true)]
    public int Id;
    public static int _id;
    public static Vector3 Scale = new Vector3(0.75f, 0.75f, 0.75f);
    public Animator Animator;
    public Transform Transform;
    public SpriteRenderer Hand;
    [UsefullInfo(readOnly: true, editorOnly: true)]
    public string Query;
    [UsefullInfo(readOnly: true, editorOnly: true)]
    public bool IsMoving;

    [UsefullInfo(readOnly: true, editorOnly: true)]
    public bool IsBussy;
    [UsefullInfo(readOnly: true, editorOnly: true)]
    public bool RunningActions = false;
    [UsefullInfo(readOnly: true)]
    public WorkPlace Work;

    [UsefullInfo(readOnly: true)]
    public PopulationPlace Home;

    [UsefullInfo]
    public string Name;

    [UsefullInfo(isCheatToChange: true)]
    public short Age;

    [UsefullInfo(readOnly: true, editorOnly: true)]
    public bool Dead;

    [UsefullInfo(readOnly: true, editorOnly: true)]
    public bool Spawned;
    [UsefullInfo(readOnly: true, editorOnly: true)]
    public bool Buried;

    [UsefullInfo(readOnly: true, editorOnly: true)]
    public bool BurryOrder;
    public Vector3 SpawnPosition;

    [UsefullInfo(readOnly: true, editorOnly: true)]
    public string DeadReason;

    [UsefullInfo(readOnly: true)]
    public WorkTier Tier = WorkTier.FARMER;
    public CancellationTokenSource CancelJobTokenSource;
    public Villager()
    {
        Id = _id++;
    }

    [UsefullInfo(isCheatToChange: true)]
    public int Hunger = 100;

    [UsefullInfo(readOnly: true, editorOnly: true)]
    public int Decay = 0;
    public bool IsRetired => Age > GameConfig.WORKING_MAX_AGE;

    public bool HasLabel;
    public void Spawn()
    {
        VillagerController.Instance.CreateVillagerInstance(this);
    }
    public void SetHands(UnityEngine.Sprite sprite)
    {
        Animator.SetBool("Carry", true);
        Hand.gameObject.SetActive(true);
        Hand.transform.localScale = Vector3.zero;
        Hand.transform.DOScale(new Vector3(0.25f, 0.25f, 0.25f), 0.3f);
        Hand.sprite = sprite;
    }

    public void SetHandsEmpty()
    {
        Transform.gameObject.SetActive(false);
        Animator.SetBool("Carry", false);
        Transform.gameObject.SetActive(true);
        Hand.gameObject.SetActive(false);
        Hand.sprite = null;
    }

    public void Stop()
    {
        IsMoving = false;
        IsBussy = false;
        RunningActions = false;
        CancelJobTokenSource.Cancel();
        CancelJobTokenSource = new System.Threading.CancellationTokenSource();
        if (Work != null)
            if (Work is ProductionPlace place)
                place.ReadyForWork = false;
    }
}
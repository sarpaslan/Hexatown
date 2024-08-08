using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public enum GoblinState
{

    IDLE,
    ATTACKPLACE,
    GOINGPLACE,
    GOINGCAVE,
}

public class GoblinBehaviour : AIBehaviour
{
    public ExplorationPlace Cave;
    public GoblinState State;

    public void Start()
    {
        tolerance = 0.75f;
        Scale = Scale * UnityEngine.Random.Range(0.7f, 1f);
    }
    public override void OnStartedWalking()
    {
        base.OnStartedWalking();
        transform.localScale = new Vector3(TargetPosition.x > transform.position.x ? Scale.x : -Scale.x, Scale.y, Scale.z);
    }
    public override void OnStoppedWalking()
    {
        base.OnStoppedWalking();
        switch (State)
        {
            case GoblinState.ATTACKPLACE:
                if (TargetPlace.Burnt || TargetPlace.Destroyed)
                {
                    GoblinController.Instance.MoveToCloesestBurnable(this);
                    return;
                }
                Attack().Forget();
                break;
            case GoblinState.GOINGCAVE:
                GoblinController.Instance.Remove(this);
                break;
        }
    }

    public async UniTask GoCaveIfBurnt()
    {
        if (TargetPlace == null || TargetPlace.Destroyed || TargetPlace.Burnt)
        {
            await UniTask.WaitForSeconds(0.5f);
            State = GoblinState.GOINGCAVE;
            Move(Cave);
        }
    }

    public async UniTask Attack()
    {
        int fire = 4;
        while (State == GoblinState.ATTACKPLACE)
        {
            await GoCaveIfBurnt();
            if (State != GoblinState.ATTACKPLACE)
                break;

            Animator.SetTrigger("Attack");
            await UniTask.WaitForSeconds(0.5f);
            await GoCaveIfBurnt();
            if (State != GoblinState.ATTACKPLACE)
                break;

            if (!TargetPlace.OnFire)
            {
                FireController.Instance.CreateFire(TargetPlace);
            }
            fire += 2;
            TargetPlace.Fire += fire;
            await UniTask.WaitForSeconds(1f);
        }
    }
}

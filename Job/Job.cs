using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Job
{
    static void JobAnyStart(string job, Villager vlg, bool stop = true)
    {
        if (vlg.Dead) new InvalidOperationException(vlg.Name + " is dead but you are trying to move it ");
        if (vlg.RunningActions)
        {
            UnityEngine.Debug.LogError($"There is already a job running you cannot run this new job {job}");
        }
        vlg.RunningActions = stop;
        if (!string.IsNullOrEmpty(job))
        {
            vlg.Query = job;
        }
    }
    static void JobAnyStop(Villager vlg)
    {
        vlg.RunningActions = false;
    }
    static bool IsApproximatelyEqual(Vector3 a, Vector3 b, float tolerance = 0.001f)
    {
        return Vector3.Distance(a, b) < tolerance;
    }
    public async static UniTask Move(Villager villager, Vector3 destination)
    {
        if (!villager.Spawned)
        {
            throw new InvalidOperationException("You are trying to move a not spawned villager");
        }
        JobAnyStart("Moving", villager);
        var isMovingLeft = villager.Transform.position.x < destination.x;
        var scale = Villager.Scale.x;
        villager.Transform.localScale = new Vector3(isMovingLeft ? scale : -scale, scale, scale); ;
        villager.Animator.SetBool("Run", true);
        var carry = villager.Animator.GetBool("Carry");
        villager.IsMoving = true;
        if (villager.Transform == null)
        {
            UnityEngine.Debug.LogError("Villager is spawned: " + villager.Spawned + " but transform is null " + new StackTrace().ToString());
        }

        float speedMultiplier = GetSpeedMultiplier(villager.Age);
        float tolerance = 0.1f;

        while (!villager.CancelJobTokenSource.IsCancellationRequested)
        {
            if (villager.Transform == null)
            {
                UnityEngine.Debug.Log("villager transform is null but you are trying to move it ");
                break;
            }
            var newPos = Vector3.MoveTowards(villager.Transform.position, destination, carry ? GameConfig.VILLAGER_CARRY_SPEED * speedMultiplier * Time.deltaTime : GameConfig.VILLAGER_SPEED * speedMultiplier * Time.deltaTime);
            if (Vector3.Distance(newPos, destination) <= tolerance)
            {
                villager.Transform.position = destination;
                break;
            }
            else
            {
                villager.Transform.position = newPos;
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, villager.CancelJobTokenSource.Token);
            }
        }
        if (villager.Transform == null)
            return;

        villager.Animator.SetBool("Run", false);
        villager.IsMoving = false;
        JobAnyStop(villager);
    }
    private static float GetSpeedMultiplier(int age)
    {
        if (age <= GameConfig.VILLAGER_MOVEMENT_SLOWING_STARTING_AGE)
        {
            return 1.0f;
        }
        else
        {
            return Mathf.Max(0.1f, 1.0f - ((age - GameConfig.VILLAGER_MOVEMENT_SLOWING_STARTING_AGE) * 0.03f));
        }
    }
    public async static UniTask ExtinguishFire(Villager villager, Place place)
    {
        if (villager.Dead)
            throw new InvalidOperationException("you are trying to extinguish a fire with a dead villager");

        villager.Stop();
        villager.IsBussy = true;
        JobAnyStart("Extinguish Fire", villager, false);
        while (place.OnFire)
        {
            var well = GameController.GetClosestPlace<Well>(place.WorldPosition);
            if (well != null)
            {
                while (place.OnFire)
                {
                    await Job.CarryEnd(villager);
                    await Job.Move(villager, well.WorldPosition);
                    await Job.Carry(villager, GameController.Resources.GetIcon(SpriteIcon.WellWater));
                    await Job.Move(villager, place.WorldPosition);
                    await Job.CarryEnd(villager);
                    FireController.Instance.UseWater(place);
                    await Job.WaitSeconds(villager, 0.5f);
                }
            }
            else
            {
                await Job.CarryEnd(villager);
                await Job.Move(villager, place.RandomPointInside());
                await Job.WaitSeconds(villager, 1);
            }
        }
        villager.IsBussy = false;
        JobAnyStop(villager);
    }


    public async static UniTask Build(Villager villager)
    {
        JobAnyStart("Farming", villager);
        villager.Animator.Play("Build");
        await UniTask.WaitForSeconds(2, cancellationToken: villager.CancelJobTokenSource.Token);
        JobAnyStop(villager);
    }

    public async static UniTask Carry(Villager villager, UnityEngine.Sprite sprite)
    {
        JobAnyStart("Carrying", villager);
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, villager.CancelJobTokenSource.Token);
        villager.SetHands(sprite);
        JobAnyStop(villager);
    }

    public async static UniTask CarryEnd(Villager villager)
    {
        JobAnyStart("Dropped", villager);
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, villager.CancelJobTokenSource.Token);
        villager.SetHandsEmpty();
        JobAnyStop(villager);
    }
    public async static UniTask Kill(Villager villager)
    {
        await CarryEnd(villager);
        JobAnyStart("Died", villager);
        villager.Animator.Play("Dead");
        villager.Transform.DOScale(villager.Transform.localScale / 1.1f, 1f);
        villager.Transform.GetComponent<SpriteRenderer>().color = Color.gray;
        while (!villager.Buried)
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }
        villager.Transform.DOKill();
        JobAnyStop(villager);
        await Job.Despawn(villager, false);
    }
    public async static UniTask Despawn(Villager villager, bool despawnHome, string query = "Despawned")
    {
        if (!villager.Spawned)
            throw new InvalidOperationException("This villager is already despawned.");
        villager.Stop();
        if (despawnHome)
        {
            if (villager.Home != null)
            {
                await Job.CarryEnd(villager);
                await Job.Move(villager, villager.Home.WorldPosition);
            }
        }


        JobAnyStart(query, villager);
        villager.Transform.DOScale(Vector3.zero, 0.5f);
        await UniTask.WaitForSeconds(0.5f);
        //Debug Log villager may be despawned by now
        if (villager.Spawned)
            VillagerController.Instance.DespawnVillager(villager);
        JobAnyStop(villager);
    }
    public static async UniTask WaitSeconds(Villager villager, float v)
    {
        JobAnyStart("", villager);
        await UniTask.WaitForSeconds(v, cancellationToken: villager.CancelJobTokenSource.Token);
        JobAnyStop(villager);
    }

    public static async UniTask WaitUnscaledSeconds(Villager villager, float v)
    {
        JobAnyStart("", villager);
        await UniTask.WaitForSeconds(v, cancellationToken: villager.CancelJobTokenSource.Token, ignoreTimeScale: true);
        JobAnyStop(villager);
    }



    public static async UniTask WaitForEndOfFrame(Villager villager)
    {
        JobAnyStart("", villager);
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, villager.CancelJobTokenSource.Token);
        JobAnyStop(villager);
    }

    public static async UniTask Cut(Villager villager)
    {
        JobAnyStart("Cutting tree", villager);
        villager.Animator.Play("Cut");
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, villager.CancelJobTokenSource.Token);
        JobAnyStop(villager);
    }

    public static async UniTask Idle(Villager villager)
    {
        JobAnyStart("Standing", villager);
        villager.Animator.Play("Idle");
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, villager.CancelJobTokenSource.Token);
        JobAnyStop(villager);
    }

    public static async UniTask WaitUntilCanceled(Villager villager, CancellationTokenSource source)
    {
        JobAnyStart("Working", villager);
        await UniTask.WaitUntilCanceled(source.Token);
        JobAnyStop(villager);
    }

    public static async UniTask Jump(Villager villager)
    {
        JobAnyStart("Jumping", villager);
        villager.Animator.SetBool("JumpUp", true);
        await UniTask.WaitForSeconds(1f, cancellationToken: villager.CancelJobTokenSource.Token);
        villager.Animator.SetBool("JumpUp", false);
        JobAnyStop(villager);
    }

    public static async Task WaitUntill(Villager villager, Func<bool> waitUntill)
    {
        JobAnyStart("Waiting", villager);
        await UniTask.WaitUntil(waitUntill);
        JobAnyStop(villager);
    }
}
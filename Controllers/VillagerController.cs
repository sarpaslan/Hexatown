using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class VillagerController : MonoBehaviour
{
    public static VillagerController Instance;
    public static List<Villager> Villagers = new List<Villager>();
    public static Dictionary<Place, List<Villager>> FireWorkers = new Dictionary<Place, List<Villager>>();
    public GameObject VillagerPrefab;
    public int Count => Villagers.Count;
    public Action<Villager> OnVillagerCreated;
    public static int GetCountOf(WorkTier workTier)
    {
        return Villagers.Count(t => !t.Dead && t.Tier == workTier);
    }
    void OnEnable()
    {
        GameController.Instance.OnGameTick += Tick;
    }

    void OnDisable()
    {
        GameController.Instance.OnGameTick -= Tick;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void CreateVillagerInstance(Villager villager)
    {
        if (villager.Dead)
        {
            Debug.LogError("You are trying to create a dead villager.");
            return;
        }

        if (villager.Spawned)
        {
            Debug.LogError("This villager is already spawned");
            return;
        }

        villager.CancelJobTokenSource = new CancellationTokenSource();

        var villagerObj = Instantiate(VillagerPrefab);
        villagerObj.transform.localScale = Villager.Scale;
        var villagerBehaviour = villagerObj.GetComponent<VillagerBehaviour>();
        villagerBehaviour.Villager = villager;
        villager.Spawned = true;
        villager.IsBussy = false;
        villager.Animator = villagerObj.GetComponent<Animator>();
        villager.Transform = villagerObj.transform;
        villager.Hand = villagerObj.transform.GetChild(0).GetComponent<SpriteRenderer>();
        villager.Transform.position = LittleRandom.XY(villager.SpawnPosition, 0.5f);
    }

    public Villager CreateVillager(PopulationPlace home, Vector3 worldPosition)
    {
        var villager = VillagerFactory.CreateVillager(home, worldPosition);
        Villagers.Add(villager);
        OnVillagerCreated?.Invoke(villager);
        return villager;
    }
    public Villager GetWorker()
    {
        return Villagers.FirstOrDefault(v => IsAvailable(v));
    }

    public IEnumerable<Villager> GetAvailableWorkers()
    {
        return Villagers.Where(v => IsAvailable(v));
    }

    public bool IsAvailable(Villager villager)
    {
        return !villager.Dead && villager.Work == null && !villager.IsBussy && villager.Age < GameConfig.WORKING_MAX_AGE;
    }

    public void DespawnVillager(Villager villager)
    {
        if (!villager.Spawned)
            throw new InvalidOperationException("This villager isn't spawned yet but you are trying to despawn it.");

        villager.SpawnPosition = villager.Transform.position;
        Destroy(villager.Transform.gameObject);
        villager.Animator = null;
        villager.Hand = null;
        villager.Transform = null;
        villager.Spawned = false;
        villager.IsBussy = false;
    }

    public Vector3 RandomPointInside(Vector3 worldPosition, float offset = 0.4f)
    {
        return LittleRandom.XY(worldPosition, offset);
    }

    public void NotifyFireEnded(Place place)
    {
        if (!FireWorkers.ContainsKey(place)) return;

        FireWorkers[place].ForEach(villager =>
        {
            if (villager.Spawned)
            {
                villager.SetHandsEmpty();
                villager.Stop();
                villager.IsBussy = false;
                villager.Query = "Waiting";
            }
        });
        FireWorkers.Remove(place);
    }

    public void NotifyFire(Place place)
    {
        CleanupDeadFireWorkers();

        if (!FireWorkers.TryGetValue(place, out var fireWorkers))
        {
            fireWorkers = new List<Villager>();
            FireWorkers[place] = fireWorkers;
        }

        if (fireWorkers.Count >= GameConfig.MIN_FIREFIGHTER_COUNT) return;

        AssignFirefighters(place, fireWorkers);
    }

    private void CleanupDeadFireWorkers()
    {
        foreach (var kvp in FireWorkers)
        {
            kvp.Value.RemoveAll(villager => villager.Dead);
        }
    }

    private void AssignFirefighters(Place place, List<Villager> fireWorkers)
    {
        if (place is PopulationPlace populationPlace)
        {
            foreach (var villager in populationPlace.Villagers)
            {
                if (fireWorkers.Contains(villager) || !IsAvailableForFirefighting(villager, populationPlace))
                    continue;

                AssignToFirefighting(villager, place, fireWorkers);
                if (fireWorkers.Count >= GameConfig.MIN_FIREFIGHTER_COUNT) return;
            }
        }

        foreach (var villager in Villagers)
        {
            if (fireWorkers.Count > GameConfig.MIN_FIREFIGHTER_COUNT) break;

            if (!IsAvailableForFirefighting(villager, place)) continue;

            AssignToFirefighting(villager, place, fireWorkers);
        }
    }

    private bool IsAvailableForFirefighting(Villager villager, Place place)
    {
        if (villager.Dead || villager.IsBussy) return false;

        bool shouldJoin = Vector3.Distance(villager.SpawnPosition, place.WorldPosition) < GameConfig.FIRE_NOTICE_RADIUS;
        if (!shouldJoin && villager.Work != null)
        {
            shouldJoin = Vector3.Distance(villager.Work.WorldPosition, place.WorldPosition) < GameConfig.FIRE_NOTICE_RADIUS;
        }
        return shouldJoin;
    }

    private void AssignToFirefighting(Villager villager, Place place, List<Villager> fireWorkers)
    {
        if (!villager.Spawned) villager.Spawn();
        Job.ExtinguishFire(villager, place).Forget();
        fireWorkers.Add(villager);
    }

    public void Tick(int tick)
    {
        if (tick % GameConfig.AGE_TICK == 0)
        {
            AgeVillagers();
        }

        if (tick % GameConfig.VILLAGER_REQUEST_NEED_TICK == 0)
        {
            HandleVillagerHunger();
        }

        if (tick % GameConfig.ASSIGN_WORK_TO_VILLAGERS_TICK == 0)
        {
            AssignWorkToVillagers();
        }
    }
    public void HandleVillagerHunger()
    {
        var hungryVillagers = Villagers.Where(v => !v.Dead).OrderBy(v => v.Hunger).ToList();
        foreach (var villager in hungryVillagers)
        {
            // var totalFood = GameController.Stats.Get(global::Stats.FOOD);
            // var minEatFood = Mathf.Min(totalFood, GameConfig.VILLAGER_EAT_FOOD);
            // if (GameController.Stats.Spend(global::Stats.FOOD, minEatFood))
            // {
            //     villager.Hunger += minEatFood * 2;
            // }
            // else
            // {
            //     villager.Hunger -= villager.Work != null ? 6 : 3;
            // }

            // if (villager.Hunger > 100)
            //     villager.Hunger = 100;

            // if (villager.Hunger <= 0)
            // {
            //     VillagerController.Instance.Kill(villager, "Hunger", true);
            // }
        }
    }

    private void AgeVillagers()
    {
        for (int i = Villagers.Count - 1; i >= 0; i--)
        {
            if (Villagers[i].Dead) continue;

            var villager = Villagers[i];
            villager.Age += 1;
            if (villager.Age > GameConfig.WORKING_MAX_AGE)
            {
                if (villager.Work != null)
                {
                    villager.Work.RemoveVillager(villager);
                }
            }
            if (villager.Age > GameConfig.DEATH_START_AGE)
            {
                int prob = villager.Age > 70 ? 15 : 0;
                if (UnityEngine.Random.Range(0, 250) <= 2 + prob)
                {
                    Kill(villager, "Old age", true);
                }
            }
        }
    }


    private void AssignWorkToVillagers()
    {
        for (int i = Villagers.Count - 1; i >= 0; i--)
        {
            try
            {
                //TODO: What the fuck decay doing here. Fix later.
                Villager v = Villagers[i];
                HandleDecay(v);
                if (ShoulDespawn(v))
                    Job.Despawn(v, true).Forget();
                if (CanWork(v))
                    v.Work.Work(v).Forget();
            }
            catch (Exception e)
            {
                Debug.LogError("Something went wrong on villagers " + e.ToString());
            }
        }
    }
    public bool CanWork(Villager v)
    {
        return v.Age < GameConfig.WORKING_MAX_AGE && !v.Dead && v.Work != null && !v.RunningActions;
    }
    public bool ShoulDespawn(Villager v)
    {
        return v.Spawned && !v.Dead && v.Work == null && v.Home != null && !v.IsBussy && !v.RunningActions;
    }

    private void HandleDecay(Villager v)
    {
        if (v.Dead && !v.BurryOrder && !v.Buried)
        {
            v.Decay += GameConfig.CORPSE_DECAY_PER_TICK;
            if (v.Decay >= 100)
            {
                if (v.Spawned)
                {
                    v.BurryOrder = true;
                    v.Buried = true;
                    Job.Despawn(v, false, "Decayed").Forget();
                }
            }
        }
    }

    public void Kill(Villager villager, string reason, bool needsBury)
    {
        villager.Stop();

        villager.Work?.Villagers.Remove(villager);
        villager.Work?.NotifyPropertyChange();
        villager.Work = null;

        villager.Home?.Villagers.Remove(villager);
        villager.Home?.NotifyPropertyChange();

        villager.DeadReason = reason;
        GameController.Stats.Set(Stats.POP, Villagers.Count(v => !v.Dead));

        if (needsBury)
        {
            villager.Buried = false;
            if (!villager.Spawned) villager.Spawn();
            Job.Kill(villager).Forget();
        }
        else
        {
            villager.Buried = true;
            if (villager.Spawned)
            {
                Job.Despawn(villager, false).Forget();
            }
        }

        villager.Dead = true;
    }

    public static Villager GetVillager(int id)
    {
        return Villagers.FirstOrDefault(v => v.Id == id);
    }
}


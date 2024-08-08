using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GoalController : MonoBehaviour
{
    public GoalItemView ItemViewPrefab;
    public RectTransform Container;
    public bool IsOn = false;
    private Dictionary<int, GoalItemView> goalQueue = new Dictionary<int, GoalItemView>();
    public int Step = 0;
    private int tempSpawnFood = -1;
    private int tempPopulationTick = -1;
    private int tempEatFoodTick = -1;
    private float tempGrowRateMultiplier = -1;
    private float tempRefund = -1;
    public FarmCard FarmCard;
    public VillageCard VillageCard;
    public Card LumberjackCard;
    public Button ToggleGoalsButton;
    public Transform ResearchButtonTransform;
    public static GoalController Instance;
    public static bool Skip = false;
    public static bool Completed = false;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        Step = 0;
        GameController.Instance.OnGameTick += OnTick;
    }

    private void ResetConfig()
    {
        if (tempSpawnFood != -1)
            GameConfig.VILLAGER_SPAWN_FOOD = tempSpawnFood;

        if (tempPopulationTick != -1)
            GameConfig.NEW_VILLAGER_TICK = tempPopulationTick;

        if (tempEatFoodTick != -1)
            GameConfig.VILLAGER_REQUEST_NEED_TICK = tempEatFoodTick;

        if (tempRefund != -1)
            GameConfig.REFUND_PERCENT = tempRefund;

        tempSpawnFood = tempPopulationTick = tempEatFoodTick = -1;
        tempGrowRateMultiplier = tempRefund = -1;
    }


    private void OnTick(int obj)
    {
        if (Skip)
        {
            if (!ResearchButtonTransform.gameObject.activeSelf)
                ResearchButtonTransform.gameObject.SetActive(true);
            return;
        }
        if (Completed)
        {
            Skip = true;
            return;
        }
        if (GameController.Castle == null) return;

        if (Application.isEditor)
        {
            if (GameConfig.SKIP_TUTORIAL)
            {
                Completed = true;
                if (Step == 0)
                {
                    UnlockAndGiveResourcesToBuildIt(PlaceType.FARMLAND, 2);
                    UnlockAndGiveResourcesToBuildIt(PlaceType.VILLAGE, 3);
                    UnlockAndGiveResourcesToBuildIt(PlaceType.LUMBERJACK, 1);
                    ResearchButtonTransform.gameObject.SetActive(true);
                    Step = 100;
                    return;
                }
            }
        }
        if (Step == 100) return;

        switch (Step)
        {
            case 0:
                StartOfTutorial();
                Place2HousesGoal();
                break;
            case 1:
                HousePlacedCheck();
                break;
            case 2:
                Reach4PopulationCheck();
                break;
            case 3:
                Step++;
                break;
            case 4:
                Step++;
                break;
            case 5:
                PlaceFarmlandGoal();
                break;
            case 6:
                CheckFarmlandPlaced();
                break;
            case 7:
                Reach100FoodGoal();
                break;
            case 8:
                CheckIf20PotatoReached();
                break;
            case 9:
                PlaceLumberjackGoal();
                break;
            case 10:
                CheckIfLumberJackPlaced();
                break;
            case 11:
                Reach200WoodGoal();
                break;
            case 12:
                CheckIfWoodGoalReached();
                break;
            case 13:
                ResearchWellBuilding();
                break;
            case 14:
                CheckIfWellPlaced();
                break;
            case 15:
                RewardWellPlace();
                break;
        }
    }

    private void RewardWellPlace()
    {
        Step++;
        QueueGoal(15, "Upgrade your first house.\n<size=24>Select and upgrade the house by fulfilling the villagers needs.</size>");
        GameController.Stats.Add(Stats.GOLD, 100);
        Completed = true;
    }

    private void StartOfTutorial()
    {
        ResearchButtonTransform.gameObject.SetActive(false);
        tempRefund = GameConfig.REFUND_PERCENT;
        GameConfig.REFUND_PERCENT = 100;
    }
    private void ResearchWellBuilding()
    {
        QueueGoal(14, "Build a Well.\n<size=24>Open Research Menu and research the <b>Well.</b> You might need to build more farmer houses.</size>");
        Step++;
        GameConfig.REFUND_PERCENT = tempRefund;
        GameController.Stats.Add(Stats.GOLD, 250);
    }
    private void CheckIfWellPlaced()
    {
        if (GameController.CountOf(PlaceType.WELL) > 0)
        {
            CompleteGoal(14);
            Step++;
        }
    }

    private void Place2HousesGoal()
    {
        UnlockAndGiveResourcesToBuildIt(PlaceType.VILLAGE, 2);
        tempSpawnFood = GameConfig.VILLAGER_SPAWN_FOOD;
        tempEatFoodTick = GameConfig.VILLAGER_REQUEST_NEED_TICK;

        GameConfig.VILLAGER_SPAWN_FOOD = int.MaxValue;
        GameConfig.VILLAGER_REQUEST_NEED_TICK = int.MaxValue;
        QueueGoal(0, "Place 2 Houses");
        Step++;
    }

    private void HousePlacedCheck()
    {
        if (GameController.CountOf(PlaceType.VILLAGE) >= 2)
        {
            CompleteGoal(0);
            GameConfig.VILLAGER_SPAWN_FOOD = tempSpawnFood;
            QueueGoal(1, "Reach 4 Population");
            tempPopulationTick = GameConfig.NEW_VILLAGER_TICK;
            GameConfig.NEW_VILLAGER_TICK = 3;
            Step++;
        }
    }

    private void Reach4PopulationCheck()
    {
        if (VillagerController.Villagers.Count >= 4)
        {
            GameConfig.NEW_VILLAGER_TICK = tempPopulationTick;
            CompleteGoal(1);
            Step++;
            GameConfig.VILLAGER_REQUEST_NEED_TICK = tempEatFoodTick;
        }
    }


    private void PlaceFarmlandGoal()
    {
        QueueGoal(3, "Place 2 Farmland\n<size=26>Its better to place near town center or Windmills when you unlock them</size>");
        UnlockAndGiveResourcesToBuildIt(PlaceType.FARMLAND, 2);
        Step++;
    }

    private void CheckFarmlandPlaced()
    {
        if (GameController.CountOf(PlaceType.FARMLAND) >= 2)
        {
            var villages = GameController.GetPlaces<Village>();
            foreach (var v in villages)
            {
                foreach (var req in v.Requirements)
                {
                    if (req.Stats == Stats.POTATO)
                        req.CurrentAmount = req.RequireAmount;
                }
            }
            CompleteGoal(3);
            Step++;
        }
    }

    private void Reach100FoodGoal()
    {
        QueueGoal(4, "Reach 10 Potato\n<size=26>Wait for farm to produce 10 potato.Select to see it progress.</size>\n<size=24>You can speed up time.</size>");
        Step++;
    }

    private void CheckIf20PotatoReached()
    {
        if (GameController.Stats.Get(Stats.POTATO) >= 10)
        {
            CompleteGoal(4);
            Step++;
        }
    }

    private void PlaceLumberjackGoal()
    {
        QueueGoal(5, "Place the Lumberjack\n<size=26>The lumberjack can only be placed in areas with trees. For best results, place it in forests.</size>");
        UnlockAndGiveResourcesToBuildIt(PlaceType.LUMBERJACK, 1);
        Step++;

    }

    private void CheckIfLumberJackPlaced()
    {
        if (GameController.CountOf(PlaceType.LUMBERJACK) >= 1)
        {
            CompleteGoal(5);
            Step++;
        }
    }

    private void Reach200WoodGoal()
    {
        QueueGoal(6, "Reach 30 Wood\n<size=26>You can place additional lumberjacks to speed up the process, but they cost to maintain. Keeping the gold balance is important.</size>");
        Step++;
    }

    private void CheckIfWoodGoalReached()
    {
        if (GameController.Stats.Get(Stats.WOOD) >= 30)
        {
            ResearchButtonTransform.gameObject.SetActive(true);
            CompleteGoal(6);
            Step++;
        }
    }

    public void ToggleContainer()
    {
        IsOn = !IsOn;
        ToggleGoalsButton.transform.localEulerAngles = new Vector3(0, 0, IsOn ? 0 : 180);
        Container?.DOKill();
        float targetX = IsOn ? 0 : -460;
        Container.DOAnchorPosX(targetX, 0.2f).SetEase(Ease.Linear).SetUpdate(true);
    }

    public void QueueGoal(int id, string goal)
    {
        var view = Instantiate(ItemViewPrefab, Container);
        view.transform.localScale = Vector3.zero;
        view.transform.DOScale(Vector3.one, 0.5f).SetUpdate(true);
        view.Title.text = goal;
        goalQueue.Add(id, view);
        if (!IsOn) ToggleContainer();
    }

    public void CompleteGoal(int id)
    {
        if (goalQueue.TryGetValue(id, out var view))
        {
            view.TickImage.gameObject.SetActive(true);
            view.transform.DOScale(Vector3.zero, 0.2f).SetDelay(1f).SetUpdate(true);
            Destroy(view.gameObject, 1.5f);
            goalQueue.Remove(id);
        }
    }

    private void UnlockAndGiveResourcesToBuildIt(PlaceType placeType, int multiplier)
    {
        Card card = null;
        switch (placeType)
        {
            case PlaceType.FARMLAND:
                card = FarmCard;
                break;
            case PlaceType.VILLAGE:
                card = VillageCard;
                break;
            case PlaceType.LUMBERJACK:
                card = LumberjackCard;
                break;
        }
        CardController.Instance.Unlock(card);
        foreach (var price in card.Prices)
        {
            GameController.Stats.Add(price.Stats, price.Value * multiplier);
        }
    }

    private void OnDisable()
    {
        Step = 0;
        ResetConfig();
        GameController.Instance.OnGameTick -= OnTick;
    }
}


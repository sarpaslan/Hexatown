using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerStatsView : MonoBehaviour
{
    public static PlayerStatsView Instance;

    private void Awake()
    {
        Instance = this;
    }

    public TMP_Text PopulationText;

    [FormerlySerializedAs("FoodText")]
    public TMP_Text BalanceText;
    [FormerlySerializedAs("WarriorText")]
    public TMP_Text WoodText;
    public TMP_Text GoldText;

    public void OnStatChanged(Stats stat)
    {
        switch (stat)
        {
            case Stats.POP:
            case Stats.MAXPOP:
                PopulationText.text = $"{GameController.Stats.Get(Stats.POP)}/{GameController.Stats.Get(Stats.MAXPOP)}({VillagerController.Villagers.Count(t => t.Work != null)})";
                PopulationText.transform.DOKill();
                PopulationText.transform.localScale = Vector3.one;
                PopulationText.transform.DOShakeScale(0.1f);
                break;
            case Stats.BALANCE:
                var balance = GameController.Stats.Get(Stats.BALANCE);
                var color = balance >= 0 ? "<color=green>+" : "<color=red>";
                BalanceText.text = $"{color}{balance}</color> / {GameConfig.TAX_TICK}s";
                BalanceText.transform.DOKill();
                BalanceText.transform.localScale = Vector3.one;
                BalanceText.transform.DOShakeScale(0.1f);
                break;
            case Stats.WOOD:
                WoodText.text = $"{GameController.Stats.Get(Stats.WOOD)}/{GameController.Stats.MaxStat}";
                WoodText.transform.DOKill();
                WoodText.transform.localScale = Vector3.one;
                WoodText.transform.DOShakeScale(0.1f);
                break;
            case Stats.GOLD:
                GoldText.text = $"{GameController.Stats.Get(Stats.GOLD)}";
                GoldText.transform.DOKill();
                GoldText.transform.localScale = Vector3.one;
                GoldText.transform.DOShakeScale(0.1f);
                break;
        }
    }

    public void Start()
    {
        WoodText.text = $"{GameController.Stats.Get(Stats.WOOD)}/{GameController.Stats.MaxStat}";
        var balance = GameController.Stats.Get(Stats.BALANCE);
        var color = balance >= 0 ? "<color=green>+" : "<color=red>";
        BalanceText.text = $"{color}{balance}</color> / {GameConfig.TAX_TICK}s";
        PopulationText.text = $"{GameController.Stats.Get(Stats.POP)}/{GameController.Stats.Get(Stats.MAXPOP)}({VillagerController.Villagers.Count(t => t.Work != null)})";
        GoldText.text = $"{GameController.Stats.Get(Stats.GOLD)}";
    }

    private void OnEnable()
    {
        GameController.Stats.OnValueChanged.AddListener(OnStatChanged);
    }
    private void OnDisable()
    {
        GameController.Stats.OnValueChanged.RemoveListener(OnStatChanged);
    }
}

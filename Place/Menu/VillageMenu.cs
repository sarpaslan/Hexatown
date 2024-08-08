using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VillageMenu : PlaceMenu
{
    public int UpgradePrice = 2000;
    [SerializeField]
    private Button m_upgradeButton;
    void OnEnable()
    {
    }
    public override void SetPlace(Place place)
    {
        base.SetPlace(place);
        m_upgradeButton.GetComponentInChildren<TMP_Text>().text = $"Upgrade\n{((Place as Castle).Tier + 1) * UpgradePrice} gold";
    }
    void Start()
    {
        if (this.Place is Castle castle)
        {
            if (castle.Tier >= 4)
            {
                m_upgradeButton.gameObject.SetActive(false);
            }
        }
        else
        {
            var vlg = (Village)this.Place;
            if (vlg.Tier == WorkTier.ARTISAN)
            {
                m_upgradeButton.gameObject.SetActive(false);
            }
        }
    }

    public void Upgrade()
    {
        if (this.Place is Castle castle)
        {
            if (GameController.Stats.Purchase((castle.Tier + 1) * UpgradePrice))
            {
                castle.Upgrade();
                Close();
            }
            else
            {
                GameScreen.Instance.ShowInfo("Error", $"You need {(castle.Tier + 1) * UpgradePrice} gold to upgrade your castle");
            }
        }
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}

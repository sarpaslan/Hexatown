using Cysharp.Threading.Tasks;
using KeuGames.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
public class TileInfoController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_selectedTileNameText;
    [SerializeField]
    private TMP_Text m_selectedTileDescriptionText;
    [SerializeField]
    private Image m_selectedTileImage;
    public static TileInfoController Instance;

    [FormerlySerializedAs("m_showSettingsButton")]
    [SerializeField]
    private Button m_menuButton;

    [SerializeField]
    private Button m_destroyButton;

    [SerializeField]
    private Transform m_tileInfo;
    public TMP_Text m_hoverTileInfoText;

    public static Place selection;
    public static Place lastSelection;

    [SerializeField]
    private Transform m_tileImageContainer;

    [SerializeField]
    private UnityEngine.Sprite m_setupIcon;

    private GameObject m_currentSettingMenu;
    private PlaceView m_currentPlaceView;
    [SerializeField]
    private TMP_Text m_tileCostText;
    [SerializeField]
    private Image m_currentTierIcon;
    [SerializeField]
    private TMP_Text m_currentPopulationText;


    private void Awake()
    {
        Instance = this;
    }

    public void Refresh(Place place)
    {
        if (selection != place)
        {
            return;
        }
        RefreshPlaceUI(place);
    }

    public void Select(Vector3Int pos)
    {
        Place place = GameController.GetPlace(pos);
        Select(place);
        GameController.Resources.SoundPlayer.Play("select");
        CardController.Instance.CardScrollView.gameObject.SetActive(false); ;
    }


    public void SetIncomeText(int income)
    {
        var color = "<color=white>";
        if (income > 0)
        {
            color = "<color=green>+";
        }
        else if (income < 0)
        {
            color = "<color=red>";
        }
        m_tileCostText.text = $"{color}{income}</color>";
    }
    public void RefreshPlaceUI(Place place)
    {
        m_tileInfo.gameObject.SetActive(true);
        m_selectedTileNameText.text = place.GetName();
        m_selectedTileDescriptionText.text = place.GetDescription() + $"\n\n<color=grey><size=22>{place.BiomeType}</size></color>";
        m_selectedTileImage.sprite = place.GetSprite();
        SetIncomeText(place.Tax);

        m_menuButton.gameObject.SetActive(place.Card.MenuTemplate != null);
        m_destroyButton.gameObject.SetActive(place.Card.Destroyable && place.Fire <= 0);

        if (place.Card.PlaceView != null)
        {
            if (m_currentPlaceView == null)
            {
                m_currentPlaceView = Instantiate(place.Card.PlaceView, m_tileInfo.transform.parent); ;
            }
            m_currentPlaceView.OnSetContext(place);
        }

        if (place is PopulationPlace pop)
        {
            m_currentTierIcon.gameObject.SetActive(true);
            m_currentPopulationText.text = $"{pop.Villagers.Count}/{pop.MaxPopulation}";
            m_currentTierIcon.sprite = (pop as Village).Tier.ToSprite();
        }
        else if (place is WorkPlace work)
        {
            m_currentTierIcon.gameObject.SetActive(true);
            m_currentPopulationText.text = $"{work.Villagers.Count}/{work.NeededWorkerCount}";
            m_currentTierIcon.sprite = work.Tier.ToSprite();
        }
        else
        {
            m_currentTierIcon.gameObject.SetActive(false);
            m_currentPopulationText.text = "";
        }
    }

    public void OnDestroyPlaceCLicked()
    {
        if (selection.Fire > 0)
        {
            return;
        }

        if (selection.Tick > 5)
            selection.DestroyPlace();
    }

    public void OnShowMenuClicked()
    {
        var card = selection.Card;
        if (card.MenuTemplate != null)
        {
            if (m_currentSettingMenu != null)
            {
                Destroy(m_currentSettingMenu.gameObject);
            }
            m_currentSettingMenu = Instantiate(card.MenuTemplate);
            var placeMenu = m_currentSettingMenu.GetComponent<PlaceMenu>();
            placeMenu.SetPlace(selection);
        }
    }

    public async void Select(Place place)
    {
        if (selection != null)
        {
            selection.OnDeselected();
        }
        lastSelection = place;
        var btn = m_tileImageContainer.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {

        });
        selection = place;
        RefreshPlaceUI(place);
        m_tileInfo.gameObject.SetActive(true);
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        selection.OnSelect();
    }

    public void Deselect()
    {
        if (selection != null)
        {
            selection.OnDeselected();
            selection = null;
            if (m_currentSettingMenu != null)
            {
                Destroy(m_currentSettingMenu.gameObject);
                m_currentSettingMenu = null;
            }
            if (m_currentPlaceView != null)
            {
                Destroy(m_currentPlaceView.gameObject);
                m_currentPlaceView = null;
            }
            CardController.Instance.CardScrollView.gameObject.SetActive(true);
        }
    }
    public void Show(Vector3Int pos)
    {
        Select(pos);
    }
    public void Hide()
    {
        m_tileInfo.gameObject.SetActive(false);
        Deselect();
    }
}

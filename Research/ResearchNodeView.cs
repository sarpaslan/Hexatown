using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResearchNodeView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Image Icon;
    public TMP_Text Name;
    public Card Card;
    public Image FillImage;
    public WorkTier NeedPopulationWorkTier;
    public int PopulationNeed = 25;
    public TMP_Text NeededPopulationText;
    public Button Button;
    public Image PopulationIcon;
    public ResearchNodeView Connection;
    [SerializeField]
    private bool m_unlocked;
    public bool CanUnlock => Connection == null ? true : Connection.Unlocked;
    public Image Background;
    public Image CanUnlockImage;
    public bool Unlocked
    {
        get
        {
            return m_unlocked;
        }
        set
        {
            m_unlocked = value;
            FillImage.color = m_unlocked ? Color.green : Color.clear;
        }
    }
    void OnEnable()
    {
        m_unlocked = CardController.IsUnlocked(this.Card.Type);
        UpdateConnections();
    }
    public void OnClicked()
    {

    }
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (Connection != null)
        {
            // Draws a blue line from this transform to the target
            Gizmos.color = Color.white;
            DrawConnections(this);
        }
    }
    public void DrawConnections(ResearchNodeView connection)
    {
        Gizmos.DrawSphere(connection.transform.position, 5);
        if (connection.Connection == null)
            return;
        Gizmos.DrawLine(connection.transform.position, connection.Connection.transform.position);
        DrawConnections(connection.Connection);
    }
#endif

    public void OnValidate()
    {
        if (Card == null) return;
        this.Icon.sprite = Card.Tile.sprite;
        this.Name.text = Card.Name;
        gameObject.name = Card.Name;
        UpdateConnections();
        NeededPopulationText.text = PopulationNeed.ToString();
        PopulationIcon.sprite = NeedPopulationWorkTier.ToSprite();
    }
    public void UpdateConnections()
    {
        FillImage.fillAmount = m_unlocked ? 1 : 0;
        Background.color = Color.white;
        CanUnlockImage = transform.GetChild(0).GetComponent<Image>();
        CanUnlockImage.gameObject.SetActive(CanUnlock);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        ResearchView.Instance.Select(this);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
    }
}

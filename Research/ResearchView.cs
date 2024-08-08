using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchView : MonoBehaviour
{
    public GameObject CurrentUnlocking;
    public GameObject ResourceTreeView;

    [SerializeField]
    private Image m_currentResearching;
    [SerializeField]
    private TMP_Text m_currentName;

    [SerializeField]
    private TMP_Text m_loadingText;

    [SerializeField]
    private Slider m_slider;
    public ResearchNodeView m_researching;
    public static float RESEARCH_HOLD_SPEED = 1f;
    public List<ResearchNodeView> Nodes = new List<ResearchNodeView>();
    public static ResearchView Instance;
    private ResearchNodeView m_selected;
    public float Percent;
    public static int ResearcherCount = 0;

    [Header("Selection")]
    public Transform SelectionContainer;
    public Image SelectionIcon;
    public TMP_Text SelectionName;
    public TMP_Text SelectionDescription;
    public Button StartResearchingButton;
    public TMP_Text ErrorText;
    public Slider ResearchSlider;
    public ScrollRect ScrollView;
    public TMP_Text FarmerCountText;
    public TMP_Text WorkerCountText;
    public TMP_Text ArtisanCountText;

    public void Deselect()
    {
        SelectionContainer.gameObject.SetActive(false);
        m_selected = null;
    }


    public void Awake()
    {
        Instance = this;
        GameController.Instance.OnGameTick += OnGameTick;
        ScrollView.onValueChanged.AddListener(OnScrollRectMove);
    }

    private void OnScrollRectMove(Vector2 arg0)
    {
        this.SelectionContainer.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        m_selected = null;
        SelectionContainer.gameObject.SetActive(false);
        CurrentUnlocking.SetActive(m_researching != null);
        ResourceTreeView.SetActive(m_researching == null);
    }
    private void OnDestroy()
    {
        GameController.Instance.OnGameTick -= OnGameTick;
        ScrollView.onValueChanged.RemoveAllListeners();
    }

    void OnValidate()
    {
        Nodes = GetComponentsInChildren<ResearchNodeView>().ToList();
    }


    //Maybe optimize this later
    private void OnGameTick(int obj)
    {
        if (obj % 5 == 0)
        {
            var libraries = GameController.GetPlaces<Library>().Where(t => t.CanWork());
            ResearcherCount = libraries.Sum(t => t.Villagers.Count) + 1;
        }
        if (m_researching != null)
        {
            ResearchSlider.gameObject.SetActive(true);
            Percent += GameConfig.RESEARCH_PER_TICK * ResearcherCount;
            ResearchSlider.value = Percent / 100f;

            if (Percent >= 100)
            {
                Percent = 0;
                Unlock(m_researching);
                m_researching = null;
                CurrentUnlocking.SetActive(m_researching != null);
                ResourceTreeView.SetActive(m_researching == null);
                return;
            }

            if (gameObject.activeSelf)
            {
                m_slider.value = Percent;
                if (ResearcherCount > 0)
                {
                    m_loadingText.text = $"Researching %{Mathf.Round(Percent)}(x{ResearcherCount})";
                }
            }
        }
        else
        {
            ResearchSlider.gameObject.SetActive(false);
        }
        if (m_selected != null)
        {
            var isActive = SelectionContainer.gameObject.activeSelf;
            if (isActive)
            {
                Select(m_selected);
            }
        }

        var count = VillagerController.GetCountOf(WorkTier.FARMER);
        this.FarmerCountText.text = "x" + count;
        count = VillagerController.GetCountOf(WorkTier.WORKER);
        this.WorkerCountText.text = "x" + count;
        count = VillagerController.GetCountOf(WorkTier.ARTISAN);
        this.ArtisanCountText.text = "x" + count;
    }


    public void Unlock(ResearchNodeView resourceTreeNode)
    {
        resourceTreeNode.Unlocked = true;
        foreach (var n in Nodes)
        {
            n.UpdateConnections();
        }
        CardController.Instance.Unlock(resourceTreeNode.Card);
    }
    public void Cancel()
    {
        Percent = 0;
        m_slider.maxValue = 100;
        m_slider.minValue = 0;
        m_slider.value = 0;
        m_researching = null;
        CurrentUnlocking.SetActive(m_researching != null);
        ResourceTreeView.SetActive(m_researching == null);
    }

    public void Select(ResearchNodeView resourceTreeNode)
    {
        m_selected = resourceTreeNode;
        this.SelectionContainer.gameObject.SetActive(true);
        this.SelectionName.text = resourceTreeNode.Card.Name;
        this.SelectionDescription.text = resourceTreeNode.Card.Description;
        this.ErrorText.text = string.Empty;
        this.SelectionIcon.sprite = resourceTreeNode.Icon.sprite;

        var count = VillagerController.GetCountOf(resourceTreeNode.NeedPopulationWorkTier);
        if (count < resourceTreeNode.PopulationNeed)
        {
            var sterror = $"Your city doesn't have enough {resourceTreeNode.NeedPopulationWorkTier.ToReadableString()}'s.";
            if (resourceTreeNode.NeedPopulationWorkTier == WorkTier.FARMER)
            {
                sterror += " Build more houses";
            }
            else
            {
                sterror += $"\nUpgrade houses to {resourceTreeNode.NeedPopulationWorkTier.ToReadableString()}'s";
            }
            this.ErrorText.text = sterror; ;
        }
        if (resourceTreeNode.Connection != null)
        {
            if (!resourceTreeNode.Connection.Unlocked)
            {
                this.ErrorText.text = $"You have to first unlock {resourceTreeNode.Connection.Card.Name}.";
            }
        }

        StartResearchingButton.gameObject.SetActive(!resourceTreeNode.Unlocked);
        this.StartResearchingButton.interactable = string.IsNullOrEmpty(this.ErrorText.text);
    }
    public void StartResearching()
    {
        SelectionContainer.gameObject.SetActive(false);
        m_researching = m_selected;
        m_slider.maxValue = 100;
        m_slider.minValue = 0;
        m_slider.value = 0;
        m_currentName.text = m_researching.Card.Name;
        m_currentResearching.sprite = m_researching.Card.Tile.sprite;
        CurrentUnlocking.gameObject.SetActive(true);
        ResourceTreeView.SetActive(m_researching == null);
        CurrentUnlocking.SetActive(m_researching != null);
    }
}


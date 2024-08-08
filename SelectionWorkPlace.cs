
using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class Selection
{
    public Sprite Icon;
    public string Name;
    [NonSerialized]
    public bool Unlocked;
    public Stats Stats;
    public Sprite[] Steps;
}


public abstract class SelectionWorkPlace : ProductionPlace
{

    public SpriteRenderer m_selectionSprite;
    [UsefullInfo]
    public Selection Selection { get; set; }
    private int m_selectionCircleId = -1;
    public SelectionWorkPlace(Card card, Vector3Int pos, int workerCount,
    int requireAmount, int produceAmount) : base(card, pos, workerCount, requireAmount, produceAmount)
    {

    }
    public override void OnSeasonChanged(Season season)
    {
        if (m_selectionSprite != null)
            this.m_selectionSprite.material = SeasonController.Instance.CurrentSeasonProperties.Material;
    }
    public override bool CanWork()
    {
        return base.CanWork() && Selection != null;
    }

    public int SelectionCount => (Card as SelectionCard).Selections.Count(t => t.Unlocked);
    public int Step = -1;

    public virtual void OnSelection(Selection selection)
    {
        CameraMovement.MovedLastFrame = false;
        Selection = selection;
        WorkPercent = 0;
        UpdateSprites();
        NotifyPropertyChange();
    }

    public Selection[] GetSelections() => (Card as SelectionCard).Selections.Where(t => t.Unlocked).ToArray();

    public override string GetName()
    {
        if (Selection != null)
            return Selection.Name + " " + base.GetName();
        return base.GetName();
    }

    public override void OnCameraMove()
    {
        base.OnCameraMove();
        if (m_selectionCircleId == -1)
            return;
        GameScreen.Instance.HideSelection(ref m_selectionCircleId, null);
    }

    public override void OnPlaced(bool silent)
    {
        base.OnPlaced(silent);
        if (!silent)
        {
            if (SelectionCount > 1)
            {
                ShowSelections();
            }
            if (SelectionCount == 1)
            {
                OnSelection(GetSelections().FirstOrDefault(t => t.Unlocked));
            }
        }
        UpdateSprites();
    }

    public override void OnTick()
    {
        base.OnTick();
        if (Selection == null && Error == PlaceError.NONE)
            Error = PlaceError.SELECTION_NEEDED;
        UpdateSprites();
    }

    public override Sprite GetSprite()
    {
        return Selection == null ? SpriteIcon.QuestionMarkIcon.ToSprite() : Selection.Steps[Step];
    }
    public override void OnAnyPlaced(Place place)
    {
        base.OnAnyPlaced(place);
        if (m_selectionCircleId == -1)
            return;
        if (place != this)
        {
            GameScreen.Instance.HideSelection(ref m_selectionCircleId, Selection);
        }
    }

    public void ShowSelections()
    {
        if (m_selectionCircleId != -1)
            return;

        var center = GameController.Camera.WorldToScreenPoint(WorldPosition);
        if (Selection == null)
        {
            m_selectionCircleId = GameScreen.Instance.ShowButtonIconCircle(GetSelections(), center, (i) =>
            {
                m_selectionCircleId = -1;
                Selection = i;
                OnSelection(Selection);
                if (Error == PlaceError.SELECTION_NEEDED)
                    Error = PlaceError.NONE;
                ShowSelections();
            });
        }
        else
        {
            center += new Vector3(0, 0.5f, 0);
            m_selectionCircleId = GameScreen.Instance.ShowButtonIcon(Selection, center, () =>
            {
                m_selectionCircleId = GameScreen.Instance.ShowButtonIconCircle(GetSelections(), center, (i) =>
                {
                    m_selectionCircleId = -1;
                    if (i.Name == Selection.Name)
                        return;
                    Selection = i;
                    OnSelection(i);
                    if (Error == PlaceError.SELECTION_NEEDED)
                        Error = PlaceError.NONE;
                });
            });
        }
    }
    public override void OnSelect()
    {
        base.OnSelect();

        if (SelectionCount > 1)
            ShowSelections();
        if (Selection == null && SelectionCount == 1)
            ShowSelections();
    }
    public virtual void UpdateSprites()
    {
        if (WorkPercent < 20)
        {
            Step = 0;
        }
        else if (WorkPercent < 40)
        {
            Step = 1;
        }
        else if (WorkPercent < 60)
        {
            Step = 2;
        }
        else if (WorkPercent <= 100)
        {
            Step = 3;
        }
        if (Selection == null)
        {
            if (m_selectionSprite != null)
            {
                GameObject.Destroy(m_selectionSprite.gameObject);
                m_selectionSprite = null;
            }
            return;
        }
        if (m_selectionSprite == null && CanWorkOnThisSeason())
        {
            m_selectionSprite = UnityEngine.Object.Instantiate((Card as SelectionCard).SelectionSpritePrefab);
            m_selectionSprite.transform.position = WorldPosition - new Vector3(0, GameConfig.MAGIC_Y, 0);
            OnSelectionSpriteCreated();
        }
        if (m_selectionSprite != null)
            m_selectionSprite.sprite = Selection.Steps[Step];
    }

    protected virtual void OnSelectionSpriteCreated() { }

    public override void DestroyPlace(bool silent = false)
    {
        base.DestroyPlace(silent);
        if (m_selectionSprite != null)
        {
            GameObject.Destroy(m_selectionSprite.gameObject);
        }
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
        GameScreen.Instance.HideSelection(ref m_selectionCircleId, null);
    }
}
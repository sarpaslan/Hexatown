using DG.Tweening;
using UnityEngine;

public class MilestoneReachedView : MonoBehaviour
{
    public MilestoneReachedSelectionView MilestoneReachedSelectionViewPrefab;
    public Transform MileStoneReachedContainer;

    public void SetMilestone(Milestone mileStone)
    {
        for (int i = 0; i < mileStone.Selections.Length; i++)
        {
            var selectionView = Instantiate(MilestoneReachedSelectionViewPrefab, MileStoneReachedContainer);
            selectionView.Icon.sprite = GameController.Resources.GetIcon(mileStone.Selections[i].Icon);
            selectionView.Name.text = mileStone.Selections[i].Name;
            selectionView.Description.text = mileStone.Selections[i].Description;
            int x = i;
            selectionView.Button.onClick.AddListener(() =>
            {
                mileStone.OnSelected?.Invoke(x);
                GetComponent<CanvasGroup>().DOFade(0, 0.3f).OnComplete(() =>
                {
                    Destroy(gameObject);
                });
            });
        }
    }
}

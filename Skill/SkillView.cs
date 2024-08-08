using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillData
{
    public int Id;
    public string Description;
    public int Level;
    public int MaxLevel;

    public int Price;
    public SpriteIcon Icon;
}

public class SkillView : MonoBehaviour
{
    public SkillItemView SkillViewTemplate;
    public Transform SkillViewContainer;
    public TMP_Text TitleText;
    public Action<SkillData> OnIncreaseSkill;

    public void UpdateUI(SkillItemView view, SkillData data)
    {
        view.LevelText.text = $"{data.Level}/{data.MaxLevel}";
        view.LevelFillImage.fillAmount = data.Level / (float)data.MaxLevel;
        view.Description.text = data.Description;
        view.IncreaseCostText.text = data.Price.ToString();
    }

    public void CreateSkill(SkillData data)
    {
        var view = Instantiate(SkillViewTemplate, SkillViewContainer);
        UpdateUI(view, data);
        view.IncreaseLevelButton.gameObject.SetActive(data.MaxLevel > data.Level);
        view.IncreaseLevelButton.onClick.AddListener(() =>
        {
            if (GameController.Stats.Purchase(data.Price))
            {
                if (data.Level < data.MaxLevel)
                {
                    data.Level++;
                    UpdateUI(view, data);
                    OnIncreaseSkill?.Invoke(data);
                }
            }
        });
        var icon = GameController.Resources.GetIcon(data.Icon);
        view.SkillIcon.sprite = icon;
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//_TextureScrollXSpeed("Speed X Axis", Range(-5, 5)) = 1 //106
public class ProductionPlaceView : PlaceView
{

    [SerializeField]
    private Image m_arrowDecor;
    public Image InputImage;
    public Image OutputImage;
    public TMP_Text WorkPercentText;
    public Button PlayPauseButton;
    public Image PlayPauseImage;
    public UnityEngine.Sprite PauseIcon;
    public UnityEngine.Sprite PlayIcon;
    public Image FillImage;
    public TMP_Text InputAmountText;
    public TMP_Text OutputAmountText;
    public override void OnInitialize(Place place)
    {
        base.OnInitialize(place);
        PlayPauseButton.onClick.RemoveAllListeners();
        PlayPauseButton.onClick.AddListener(OnPlayClicked);

    }

    private void OnPlayClicked()
    {
        var m_work = m_place as ProductionPlace;
        m_work.Pause = !m_work.Pause;
        OnSetContext(m_place);
    }

    public override void OnSetContext(Place place)
    {
        base.OnSetContext(place);
        var production = place as ProductionPlace;

        var res = GameController.Resources;

        InputImage.sprite = production.RequiresIcon == null ? res.GetIcon(SpriteIcon.QuestionMarkIcon) : production.RequiresIcon;
        OutputImage.sprite = production.Produce == Stats.NONE ? res.GetIcon(SpriteIcon.QuestionMarkIcon) : res.GetIcon(res.ToIconType(production.Produce));

        InputAmountText.gameObject.SetActive(production.RequiresIcon != null);
        OutputAmountText.gameObject.SetActive(production.Produce != Stats.NONE);

        WorkPercentText.text = (int)production.WorkPercent + "%";
        PlayPauseImage.sprite = production.Pause ? PlayIcon : PauseIcon;
        FillImage.fillAmount = production.WorkPercent / 100f;

        InputAmountText.text = "x" + production.RequireAmount;
        OutputAmountText.text = "x" + production.ProduceAmount;
        m_arrowDecor.material.SetFloat("_TextureScrollXSpeed", production.Pause || !production.ReadyForWork ? 0 : -0.1f);
    }
}

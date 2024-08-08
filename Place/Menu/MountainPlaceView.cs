
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MountainPlaceView : PlaceView
{
    public Button PlayPauseButton;
    public Image PlayPauseImage;
    public TMP_Text Output;
    public Image OutputImage;

    public Image FillImage;
    public TMP_Text WorkPercentText;
    [SerializeField]
    private Image m_arrowDecor;
    public TMP_Text CoalPercentageText;
    public TMP_Text IronPercentageText;
    public TMP_Text GoldPercentageText;
    public TMP_Text CoalCountText;
    public TMP_Text IronCountText;
    public TMP_Text GoldCountText;
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
        var mines = place as Mines;
        PlayPauseImage.sprite = mines.Pause ? SpriteIcon.Play.ToSprite() : SpriteIcon.Pause.ToSprite();
        m_arrowDecor.material.SetFloat("_TextureScrollXSpeed", mines.Pause || !mines.ReadyForWork ? 0 : -0.1f);
        OutputImage.sprite = mines.Produce == Stats.NONE ? SpriteIcon.QuestionMarkIcon
        .ToSprite() : mines.Produce.ToSprite();
        Output.text = "x" + mines.ProduceAmount;
        WorkPercentText.text = (int)mines.WorkPercent + "%";
        FillImage.fillAmount = mines.WorkPercent / 100f;

        CoalPercentageText.text = "%" + mines.MineData.CoalChance;
        IronPercentageText.text = "%" + mines.MineData.IronChance;
        GoldPercentageText.text = "%" + mines.MineData.GoldChance;


        CoalCountText.text = mines.MineData.Coal.ToString();
        IronCountText.text = mines.MineData.Iron.ToString();
        GoldCountText.text = mines.MineData.Gold.ToString();
    }
}

using System;
using UnityEngine;

public class UnlockedBiomesView : MonoBehaviour
{
    public BiomeView BiomeViewTemplate;
    public Transform BiomeViewContainer;
    public void Start()
    {
        var biomes = Enum.GetValues(typeof(BiomeType));
        foreach (object v in biomes)
        {
            var name = v.ToString();
            if (name == "None")
                continue;

            var biomeType = (BiomeType)v;
            var view = Instantiate(BiomeViewTemplate, BiomeViewContainer);
            if (Biome.IsBiomeUnlocked(biomeType))
            {
                view.Icon.sprite = GameController.Resources.GetBiomeIcon(biomeType);
                view.NameText.text = name;
                view.DescriptionText.text = Biome.GetDescription(biomeType);
            }
            else
            {
                view.Icon.sprite = GameController.Resources.GetIcon(SpriteIcon.QuestionMarkIcon);
                view.NameText.text = "";
                view.DescriptionText.text = "Not Unlocked";
            }
        }
    }

    public void Close()
    {
        GameScreen.Instance.HideUnlockedBiomes();
    }
}

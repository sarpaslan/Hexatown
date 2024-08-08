
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(TMP_Text))]
public class FontAssetResource : MonoBehaviour
{
    public FontType Type;

    void OnValidate()
    {
        if (GameController.Resources != null)
        {
            var icon = GameController.Resources.GetFont(Type);
            if (icon != null)
            {
                var text = GetComponent<TMP_Text>();
                text.font = icon.Font;
                text.fontSize = icon.Size;
            }
        }
    }
}
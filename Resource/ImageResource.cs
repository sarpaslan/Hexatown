using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class ImageResource : MonoBehaviour
{
    public SpriteIcon Type;

    void OnValidate()
    {
        if (GameController.Resources != null)
        {
            var icon = GameController.Resources.GetIcon(Type);
            if (icon != null)
            {
                GetComponent<Image>().sprite = icon;
            }
        }
    }
}


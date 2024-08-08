using UnityEngine;

[CreateAssetMenu(fileName = "Cemetery", menuName = "Cards/Cemetery")]
public class CemeteryCard : Card
{
    public int MaxPopulation = 20;
    public SpriteRenderer Layer;
    public UnityEngine.Sprite[] Sprites;
}

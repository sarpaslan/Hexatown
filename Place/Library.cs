using UnityEngine;

public class Library : WorkPlace
{
    public Library(Card card, Vector3Int pos) : base(card, pos, 2)
    {
    }

    public override WorkTier Tier => WorkTier.ARTISAN;
}
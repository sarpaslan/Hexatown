using UnityEngine;

public class Marketplace : WorkPlace
{
    public Marketplace(Card card, Vector3Int pos) : base(card, pos, 5)
    {
    }
    public override WorkTier Tier => WorkTier.WORKER;
}
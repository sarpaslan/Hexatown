using UnityEngine;

public class BurntPlace : Place
{
    public BurntPlace(Card card, Vector3Int pos) : base(card, pos)
    {

    }

    public override int Tax => 0;
}
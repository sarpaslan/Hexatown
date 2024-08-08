using UnityEngine;

public class Well : Place
{
    public Well(Card card, Vector3Int pos) : base(card, pos)
    {
    }

    public override int Tax => -1;
}

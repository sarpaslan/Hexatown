using UnityEngine;

public class ExplorerTower : Place
{
    public ExplorerTower(Card card, Vector3Int pos) : base(card, pos)
    {
    }
    public override int Tax => 0;
}

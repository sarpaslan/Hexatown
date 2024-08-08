using UnityEngine;

public class Storehouse : WorkPlace
{
    public Storehouse(Card card, Vector3Int pos) : base(card, pos, 3)
    {

    }

    public override void OnPlaced(bool silent)
    {
        base.OnPlaced(silent);
        GameController.Castle.Depot += 100;
        GameController.Castle.UpdateDepot();
    }

    public override void DestroyPlace(bool silent = false)
    {
        base.DestroyPlace();
        GameController.Castle.Depot -= 100;
        GameController.Castle.UpdateDepot();
    }

    public override WorkTier Tier => WorkTier.FARMER;
}

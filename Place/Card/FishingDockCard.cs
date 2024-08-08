using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "FishingDock", menuName = "Cards/FishingDock")]
public class FishingDockCard : Card
{
    public VillagerBoatBehaviour BoatPrefab;
    public override bool CanPlace(Vector3Int position)
    {
        var neighbours = TileMapController.Instance.GetNeighbours(position);
        foreach (var n in neighbours)
        {
            var tile = TileMapController.Instance.Foreground.HasTile(n);
            if (!tile)
                return true;
        }
        return false;
    }
}

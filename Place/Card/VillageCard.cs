
using System;
using UnityEngine;
using UnityEngine.Tilemaps;


[Serializable]
public class BiomeTileTypes
{
    public WorkTier Tier;
    public Tile[] Tiles;
}

[CreateAssetMenu(fileName = "VillageCard", menuName = "Cards/Village")]
public class VillageCard : Card
{
    public BiomeTileTypes[] Tiles;
}

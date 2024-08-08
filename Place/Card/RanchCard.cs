
using System;
using UnityEngine;


[Serializable]
public class AnimalBiomes
{
    public string Name;
    public BiomeType Biome;
}

[CreateAssetMenu(fileName = "Ranch", menuName = "Cards/Ranch")]
public class RanchCard : SelectionCard
{
    public AnimalBiomes[] Biomes;
};

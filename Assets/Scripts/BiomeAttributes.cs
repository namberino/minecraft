using UnityEngine;

// this will generate some universal data for all biomes
[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    public string biomeName;
    public int offset; // biome offset
    public float scale; // biome scale

    public int terrainHeight; // the height from the solidGroundHeight to the highest set point
    public float terrainScale;

    public byte surfaceBlock; // id of surface block
    public byte subSurfaceBlock; // id of subsurface block

    [Header("Major Floras")]
    public float majorFloraZoneScale = 1.3f;
    public int majoFloraIndex;
    [Range(0.1f, 1f)]
    public float majorFloraZoneThreshold = 0.6f; // the threshold (max distance) the flora zone 
    public float majorFloraPlacementScale = 15f; // the scale at which the flora will be placed
    [Range(0.1f, 1f)]
    public float majorFloraPlacementThreshold = 0.8f; // the threshold for flora placement
    public bool placeMajorFlora = true;

    public int maxHeight = 12;
    public int minHeight = 5;

    public Lode[] lodes;
}

// Lode is the data for the biomes (this can be edited in the Data/Biomes folder)
[System.Serializable] // for editing in the editor
public class Lode
{
    public string nodeName;
    public byte blockID;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;
}

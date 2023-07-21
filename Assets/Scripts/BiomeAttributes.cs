using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    public string biomeName;
    public int offset;
    public float scale;

    public int terrainHeight; // the height from the solidGroundHeight to the highest set point
    public float terrainScale;

    public byte surfaceBlock;
    public byte subSurfaceBlock;

    [Header("Major Floras")]
    public float majorFloraZoneScale = 1.3f;
    public int majoFloraIndex;
    [Range(0.1f, 1f)]
    public float majorFloraZoneThreshold = 0.6f;
    public float majorFloraPlacementScale = 15f;
    [Range(0.1f, 1f)]
    public float majorFloraPlacementThreshold = 0.8f;
    public bool placeMajorFlora = true;

    public int maxHeight = 12;
    public int minHeight = 5;

    public Lode[] lodes;
}

// for editing in the editor
[System.Serializable]
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 16; // width of a chunk
    public static readonly int ChunkHeight = 128; // maximum height of a chunk
    public static readonly int WorldSizeInChunks = 100; // determine the amount of chunks to load around the players

    // lighting Values
    public static float minLightLevel = 0.1f;
    public static float maxLightLevel = 0.9f;
    public static float lightFalloff = 0.08f; // for shadow effect and for future implementation of night and day

    public static int seed; // world seed

    // calculate the centre of the world
    public static int WorldCentre
    {
        get { return (WorldSizeInChunks * ChunkWidth) / 2; }
    }

    // calculate the amount of voxels in the world
    public static int WorldSizeInVoxels
    {
        get { return WorldSizeInChunks * ChunkWidth; }
    }

    // texture sprite values
    public static readonly int TextureAtlasSizeInBlocks = 16; 
    public static float NormalizedBlockTextureSize // normalize the block texture 
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }

    // these are the locations of a voxel's corners
    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
    };

    // represents offsets (represents the voxel adjacent to it) of the faces (basically checking which face is which)
    public static readonly Vector3[] faceChecks = new Vector3[6]
    {
        new Vector3(0.0f, 0.0f, -1.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, -1.0f, 0.0f),
        new Vector3(-1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f)
    };

    // Vertices for voxel's faces
    public static readonly int[,] voxelTris = new int[6, 4]
    {
        // Back, Front, Top, Bottom, Left, Right
		{0, 3, 1, 2}, // Back Face
		{5, 6, 4, 7}, // Front Face
		{3, 7, 2, 6}, // Top Face
		{1, 5, 0, 4}, // Bottom Face
		{4, 7, 0, 3}, // Left Face
		{1, 2, 5, 6} // Right Face
	};

    // uvs store the corners or the textures so that we can apply the textures
    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {
        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 1.0f)
    };
}

/*
A voxel (block) has 8 points (corners). Essentially, everything we need to know is in the location of these 8 corners.
*/
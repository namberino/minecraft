using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// The infamous debug screen that gives data like the fps of the game
public class DebugScreen : MonoBehaviour
{
    World world;
    TMP_Text text;

    float frameRate;
    float timer;

    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;

    void Start()
    {
        // getting the world and text along with the data of the world 
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<TMP_Text>();

        halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;
        halfWorldSizeInChunks = VoxelData.WorldSizeInChunks / 2;
    }

    void Update()
    {
        // these are the fps and coordinates of the player
        string debugText = "Minecraft by nam";
        debugText += "\n";
        debugText += frameRate + " fps";
        debugText += "\n\n";
        debugText += "X: " + (Mathf.FloorToInt(world.player.transform.position.x) - halfWorldSizeInVoxels) + " Y: " + Mathf.FloorToInt(world.player.transform.position.y) + " Z: " + (Mathf.FloorToInt(world.player.transform.position.z) - halfWorldSizeInVoxels) + "\n";
        debugText += "Chunk: " + (world.playerChunkCoord.x - halfWorldSizeInChunks) + " / " + (world.playerChunkCoord.z - halfWorldSizeInChunks);

        text.text = debugText;

        // this calculates the frame rate of the player
        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
}

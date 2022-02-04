using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour {

    World world;
    Text text;

    float frameRate;
    float timer;

    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;

    void Start() {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();

        halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;
        halfWorldSizeInChunks = VoxelData.WorldSizeInChunks / 2;
    }

    // We can allow ourself to update in the Update() method because this script wouldn't
    // be called if this GameObject wasn't activated. In other words, if this script is activated
    // inherently that means that we want it updating.
    void Update() {
        string debugText = "b3agz' Code a Game Like Minecraft in Unity";
        debugText += "\n";
        debugText += frameRate + " FPS";
        debugText += "\n\n";
        debugText += "XYZ: " + 
            (Mathf.FloorToInt(world.player.transform.position.x) - halfWorldSizeInVoxels) + " / " +
            Mathf.FloorToInt(world.player.transform.position.y) + " / " +
            (Mathf.FloorToInt(world.player.transform.position.z) - halfWorldSizeInVoxels) + " / ";
        debugText += "\n";
        debugText += "Chunk: " + 
            (world.playerChunkCoord.x - halfWorldSizeInChunks) + " / " +
            (world.playerChunkCoord.z - halfWorldSizeInChunks);

        text.text = debugText;

        // the reason for the timer check is because Update() runs once a frame and the framerate varies
        // from frame to frame. This would be too quick and not very readable.
        // This check makes reading the framerate easier.
        // In other words, we are updating the framerate every second for readability.  
        if (timer > 1f) {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        } else {
            timer += Time.deltaTime;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Not the most performant; Simplex would be better
public static class Noise {

    // scale will determine how "bumpy" the world will be: 
    // higher scale = more mountains
    // lower scale = less mountains, more of a rolling hill world
    public static float Get2DPerlin(Vector2 position, float offset, float scale) {
        // There is a weird bug in Unity Perlin noise function where if you pass it a whole
        // number, you will always get the same value. Adding 0.1f makes it such that whatever
        // we pass in won't be a whole number. 
        float newX = position.x + 0.1f;
        float newY = position.y + 0.1f;
        return Mathf.PerlinNoise(
            newX / VoxelData.ChunkWidth * scale + offset,
            newY / VoxelData.ChunkWidth * scale + offset
            );
    }

    // Check against the noise and decide whether there's gonna be a block there or not.
    // Takes a cross-section of 3 different Perlin noises using the values that you pass in.
    // See Carpilot's Perlin noise video on Youtube
    // https://www.youtube.com/watch?v=Aga0TBJkchM&t=0s
    public static bool Get3DPerlin(Vector3 position, float offset, float scale, float threshold) {
        float x = (position.x + offset + 0.1f) * scale;
        float y = (position.y + offset + 0.1f) * scale;
        float z = (position.z + offset + 0.1f) * scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        if ((AB + BC + AC + BA + CB + CA) / 6f > threshold)
            return true;
        else
            return false;
    }

}

using UnityEngine;

// All the array here are designed to work in the SAME order!
public static class VoxelData {

    public static readonly int ChunkWidth = 16; // Actual Minecraft value
    public static readonly int ChunkHeight = 128; // Actual Minecraft value
    public static readonly int WorldSizeInChunks = 100;

    public static int WorldSizeInVoxels {
        get { return WorldSizeInChunks * ChunkWidth; }
    }

    public static readonly int ViewDistanceInChunks = 5;

    // 4 because the Atlas is 4 blocks across
    public static readonly int TextureAtlasSizeInBlocks = 4;

    public static float NormalizedBlockTextureSize {
        get { return  1f / (float)TextureAtlasSizeInBlocks; }
    }

    // All the vertices of a given voxel
    public static readonly Vector3[] voxelVerts = new Vector3[8]{
        new Vector3(0.0f, 0.0f, 0.0f), // 0
        new Vector3(1.0f, 0.0f, 0.0f), // 1
        new Vector3(1.0f, 1.0f, 0.0f), // 2
        new Vector3(0.0f, 1.0f, 0.0f), // 3
        new Vector3(0.0f, 0.0f, 1.0f), // 4
        new Vector3(1.0f, 0.0f, 1.0f), // 5
        new Vector3(1.0f, 1.0f, 1.0f), // 6
        new Vector3(0.0f, 1.0f, 1.0f) // 7
    };

    // Represents a bunch of offsets in the same order that we check the 
    // faces in voxelTris. For each face we have an offset value which
    // represents the voxel adjacent to it.
    public static readonly Vector3[] faceChecks = new Vector3[6] {
        new Vector3(0.0f, 0.0f, -1.0f), // Check the back face
        new Vector3(0.0f, 0.0f, 1.0f),  // Check the front face
        new Vector3(0.0f, 1.0f, 0.0f),  // Check the top face
        new Vector3(0.0f, -1.0f, 0.0f), // Check the bottom face
        new Vector3(-1.0f, 0.0f, 0.0f), // Check the left face
        new Vector3(1.0f, 0.0f, 0.0f)   // Check the right face
    };

    // Unity renders any object as a collection of triangles
    // The vertices of a triangle must be defined clockwise: 
    // Unity will always render the top face of the triangle
    // and never render the bottom
    public static readonly int[,] voxelTris = new int[6,4]{ 
        // Each face (square) of the cube can contain 2 triangles, and therefore 6 vertices.
        // The actual shape of (say) the first elemen would be:
        // {0, 3, 1, 1, 3, 2}
        // These should be read as:
        // Back, Front, Top, Bottom, Left, Right
        // But the 4th and 5th values (1 and 3, respectively) are just the
        // 2nd and 3rd values reversed (3 and 1), so they are redundant and we can be
        // a bit more efficient and remove the duplicates.
        {0, 3, 1, 2}, // Back face
        {5, 6, 4, 7}, // Front face
        {3, 7, 2, 6}, // Top face
        {1, 5, 0, 4}, // Bottom face
        {4, 7, 0, 3}, // Left face
        {1, 2, 5, 6}  // Right face
    };

    // The order of addition needs to be the same on each face; this helps with texturing
    // Bottom left; top left; bottom right.
    // Bottom right; top left; top right.
    public static readonly Vector2[] voxelUvs = new Vector2[4]{
        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        // new Vector2(1.0f, 0.0f),     As above, these two lines are just the 
        // new Vector2(0.0f, 1.0f),     previous two lines reversed, therefore redundant
        new Vector2(1.0f, 1.0f)
    };

}

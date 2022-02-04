using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public ChunkCoord coord;

    GameObject chunkObject;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    World world;
    
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    // Apply the material defined in World.blockTypes; in other words, 
    // voxelMap[x, y, z] = 0;
    // means that we are going to use the first element of the array World.blockTypes 
    public byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    public bool isVoxelMapPopulated = false;

    private bool _isActive;

    public Chunk(ChunkCoord _coord, World _world, bool generateOnLoad) {
        coord = _coord;
        world = _world;
        IsActive = true;
        if(generateOnLoad)
            Init();
    }

    public void Init() {
        chunkObject = new GameObject();

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = world.material;

        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.z;
        
        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }

    void CreateMeshData() {
        // Kind of layering the chunks from bottom to top
        for (int y = 0; y < VoxelData.ChunkHeight; y++) {
            for (int x = 0; x < VoxelData.ChunkWidth; x++) {
                // ChunkWidth again because the chunks will be square across
                for (int z = 0; z < VoxelData.ChunkWidth; z++) {
                    if (world.blockTypes[voxelMap[x, y, z]].isSolid) // only draw a voxel if it's solid
                        AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    void PopulateVoxelMap(){
        for (int y = 0; y < VoxelData.ChunkHeight; y++) {
            for (int x = 0; x < VoxelData.ChunkWidth; x++) {
                // ChunkWidth again because the chunks will be square across
                for (int z = 0; z < VoxelData.ChunkWidth; z++) {
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + position);
                }
            }
        }
        isVoxelMapPopulated = true;
    }

    public byte GetVoxelFromGlobalVector3(Vector3 pos) {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        // Takes the current xCheck value (which represents the current x position of this voxel
        // in world space) and it is removing the chunk's absolute position. 
        // This gives the position of the voxel withing this chunk. 
        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }

    bool CheckVoxel(Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z)) {
            Vector3 globalPosition = pos + position;
            return world.CheckForVoxel(globalPosition);
        }
        return world.blockTypes[voxelMap[x, y, z]].isSolid;
    }

    bool IsVoxelInChunk(int x, int y, int z) {
        // Since we are checking for _adjacent_ voxels in the array, we might end up checking
        // at position -1 or at position 6 in a 6-length (0 based) array, therefore
        // leading to IndexOutRangeExceptions. 
        // E.g. if we're checking say voxel (0, 0, 0) then the voxel to the left of it is going to 
        // be (-1, 0, 0).
        // We need to prevent that.
        if (
            x < 0 || x > VoxelData.ChunkWidth - 1  ||
            y < 0 || y > VoxelData.ChunkHeight - 1 ||
            z < 0 || z > VoxelData.ChunkWidth - 1
        ) {
            // If the condition is true, then the voxel we are trying to check is outside of the current chunk
            return false;
        } else {
            return true;
        }
    }

    public bool IsActive {
        get { return _isActive; }
        set {
            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);
        }
    }

    public Vector3 position {
        get { return chunkObject.transform.position; }
    }

    void AddVoxelDataToChunk(Vector3 pos) {
        for(int p = 0; p < 6; p++){
            if (!CheckVoxel(pos + VoxelData.faceChecks[p])) {

                byte blockId = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                // Texturing
                AddTexture(world.blockTypes[blockId].GetTextureId(p));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                // the 4th and 5th values are just the 2nd and 3rd values reversed
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);

                vertexIndex += 4;
            }
        }
    }

    void CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.uv = uvs.ToArray(); // Texturing

        // Each vertex has a direction that it is facing, so that Unity can render the 
        // light and other things accurately. Recalculating the normals prevents weird
        // issues with lights and other things.
        // Each face of the cube needs its own set of 4 vertices; therefore, each vertex
        // has actually got 3 vertices in the same position.
        // Otherwise Unity would try to smooth between vertices, instead of giving us the
        // hard edge of a cube. Even though it might seem more efficient otherwise, visually
        // you need to have vertices for every edge; therefore a face of the cube can't share
        // vertices with the neares faces, otherwise edges would be smoothed over.  
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void AddTexture (int textureId) {
        // Calculate the row by integer division.
        float y = textureId / VoxelData.TextureAtlasSizeInBlocks;

        // This formula here has the effect of making us move in the texture atlas starting
        // from the top, because the texture atlas define blocks starting from the top left. 
        float x = textureId - (y * VoxelData.TextureAtlasSizeInBlocks);

        // Normalise the row/col numbers to get values between 0f and 1f.
        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        // We're working from the top, but the coordinates start from the bottom.
        // This step is needed to accommodate for that
        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

}

// Chunk position within the Chunk map.
// Therefore, this isn't the absolute position.
public class ChunkCoord {
    public int x;
    public int z;

    public ChunkCoord(): this(0,0) { }

    public ChunkCoord(int _x, int _z) {
        x = _x;
        z = _z;
    }

    public ChunkCoord(Vector3 pos) {
        // Get the bottom left back corner of whatever voxel we're on

        // Global values of the voxel
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        // Chunk values in X and Z coordinates
        int _x = xCheck / VoxelData.ChunkWidth;
        int _z = zCheck / VoxelData.ChunkWidth;

        x = _x;
        z = _z;
    }

    public bool Equals(ChunkCoord other) {
        if (other == null)
            return false;
        else if (other.x == x && other.z == z)
            return true;
        else
            return false;
    }

}
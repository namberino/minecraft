using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Game object transform: position, rotation and scale of an object

public class Chunk
{
    public ChunkCoord coord;

    GameObject chunkObject;
    MeshRenderer meshRenderer; // for rendering
    MeshFilter meshFilter; // for filtering the mesh and making it look nice

    int vertexIndex = 0; 
    List<Vector3> vertices = new List<Vector3>(); // vertices of a voxel
    List<int> triangles = new List<int>(); // list of triangles to draw on a voxel to make a face
    List<int> transparentTriangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    Material[] materials = new Material[2];
    List<Color> colors = new List<Color>();
    List<Vector3> normals = new List<Vector3>();

    public Vector3 position;

    public VoxelState[,,] voxelMap = new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth]; // map of voxels 

    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    World world;

    private bool _isActive;
    private bool isVoxelMapPopulated = false;

    public Chunk(ChunkCoord _coord, World _world)
    {
        coord = _coord;
        world = _world;
    }

    // method for initializing the 16x16xheight chunks
    public void Init()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = world.material;
        materials[1] = world.transparentMaterial;
        meshRenderer.materials = materials;

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = "Chunk: " + coord.x + ", " + coord.z;
        position = chunkObject.transform.position;

        PopulateVoxelMap();
    }

    // fill in voxel map with voxel data
    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = new VoxelState(world.GetVoxel(new Vector3(x, y, z) + position));
                }
            }
        }

        isVoxelMapPopulated = true;

        lock (world.ChunkUpdateThreadLock)
        {
            world.chunksToUpdate.Add(this);
        }

        if (world.settings.enableAnimatedChunks)
            chunkObject.AddComponent<ChunkLoadAnimation>();
    }

    // update the chunks (redo the mesh and light)
    public void UpdateChunk()
    {
        while (modifications.Count > 0)
        {

            VoxelMod v = modifications.Dequeue();
            Vector3 pos = v.position - position;
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z].id = v.id;

        }

        ClearMeshData();

        CalculateLight();

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    if (world.blocktypes[voxelMap[x, y, z].id].isSolid)
                        UpdateMeshData(new Vector3(x, y, z));

                }
            }
        }

        // drawing the chunks with threading 
        lock (world.chunksToDraw)
        {
            world.chunksToDraw.Enqueue(this);
        }
    }

    // a method for calculating the light level a voxel face would have
    void CalculateLight()
    {
        Queue<Vector3Int> litVoxels = new Queue<Vector3Int>();

        for (int x = 0; x < VoxelData.ChunkWidth; x++)
        {
            for (int z = 0; z < VoxelData.ChunkWidth; z++)
            {
                float lightRay = 1f;

                for (int y = VoxelData.ChunkHeight - 1; y >= 0; y--)
                {
                    VoxelState thisVoxel = voxelMap[x, y, z];

                    if (thisVoxel.id > 0 && world.blocktypes[thisVoxel.id].transparency < lightRay)
                        lightRay = world.blocktypes[thisVoxel.id].transparency;

                    thisVoxel.globalLightPercent = lightRay;

                    voxelMap[x, y, z] = thisVoxel;

                    if (lightRay > VoxelData.lightFalloff)
                        litVoxels.Enqueue(new Vector3Int(x, y, z));
                }
            }
        }

        while (litVoxels.Count > 0)
        {
            Vector3Int v = litVoxels.Dequeue();

            for (int p = 0; p < 6; p++)
            {
                Vector3 currentVoxel = v + VoxelData.faceChecks[p];
                Vector3Int neighbor = new Vector3Int((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z);

                if (IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z))
                {
                    if (voxelMap[neighbor.x, neighbor.y, neighbor.z].globalLightPercent < voxelMap[v.x, v.y, v.z].globalLightPercent - VoxelData.lightFalloff)
                    {
                        voxelMap[neighbor.x, neighbor.y, neighbor.z].globalLightPercent = voxelMap[v.x, v.y, v.z].globalLightPercent - VoxelData.lightFalloff;

                        if (voxelMap[neighbor.x, neighbor.y, neighbor.z].globalLightPercent > VoxelData.lightFalloff)
                            litVoxels.Enqueue(neighbor);
                    }

                }
            }
        }
    }

    // clearing the data of mesh (used when a voxel is out of the player's visible chunk)
    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
        colors.Clear();
        normals.Clear();
    }

    // check if the chunk is active or not (if not and set the activity according to the player's view 
    public bool isActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;

            if (chunkObject != null)
            {
                chunkObject.SetActive(value);
            }
        }
    }

    // check if a voxel is editable (ie can be broken and placed on
    public bool isEditable
    {
        get
        {
            if (!isVoxelMapPopulated)
                return false;
            else
                return true;
        }
    }

    // check if the voxel is in certain chunk
    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
            return false;
        else
            return true;
    }

    // a method that allows the player to edit a certain voxel and then updating the chunk to refresh 
    public void EditVoxel(Vector3 pos, byte newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[xCheck, yCheck, zCheck].id = newID;

        lock (world.ChunkUpdateThreadLock)
        {
            world.chunksToUpdate.Insert(0, this);
            UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
        }
    }

    // update the surrounding voxels of a certain voxel (because when a voxel is rendered, if a face is not visible to the player, it won't get rendered.
    // So if the player were to break the block, the surrounding voxels' faces would have to get updated)
    void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int i = 0; i < 6; i++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[i];

            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.chunksToUpdate.Insert(0, world.GetChunkFromVector3(currentVoxel + position));
            }
        }
    }

    // check and return a certain voxel
    VoxelState CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
            return world.GetVoxelState(pos + position);

        return voxelMap[x, y, z];
    }

    // getting a voxel from a certain position
    public VoxelState GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(position.x);
        zCheck -= Mathf.FloorToInt(position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }

    // a method for updating the chunks mesh data
    void UpdateMeshData(Vector3 pos)
    {
        // get the position of the voxel
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        byte blockID = voxelMap[x, y, z].id; // getting the block id
        //bool isTransparent = world.blocktypes[blockID].renderNeighborFaces;

        for (int p = 0; p < 6; p++)
        {
            VoxelState neighbor = CheckVoxel(pos + VoxelData.faceChecks[p]);
            // rendering the faces of the block and its neighbor blocks' faces
            if (neighbor != null && world.blocktypes[neighbor.id].renderNeighborFaces)
            {
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                for (int i = 0; i < 4; i++)
                    normals.Add(VoxelData.faceChecks[p]);

                AddTexture(world.blocktypes[blockID].GetTextureID(p)); // adding the texture to the block

                float lightlevel = neighbor.globalLightPercent; // get the light level for the alpha

                colors.Add(new Color(0, 0, 0, lightlevel));
                colors.Add(new Color(0, 0, 0, lightlevel));
                colors.Add(new Color(0, 0, 0, lightlevel));
                colors.Add(new Color(0, 0, 0, lightlevel));

                // adding the solid block triangles to the faces to create a block
                if (!world.blocktypes[neighbor.id].renderNeighborFaces)
                {
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);
                }
                else // this is the same thing but for the transparent block
                {
                    transparentTriangles.Add(vertexIndex);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 3);
                }

                vertexIndex += 4;
            }
        }
    }

    // a method for rendering the mesh of a voxel
    public void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();
        //mesh.triangles = triangles.ToArray();

        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);
        //mesh.RecalculateNormals();

        mesh.normals = normals.ToArray();

        meshFilter.mesh = mesh;
    }

    // adding in the texture for the voxel
    void AddTexture(int textureID)
    {
        // x and y is the position of the block face in the image in the Resources/AtlasPacker
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        // normalize the block texture size
        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }
}

// a class to get the coordinate of the chunk
public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.ChunkWidth;
        z = zCheck / VoxelData.ChunkWidth;
    }

    // check if 2 chunk coords is equal to each other
    public bool Equals(ChunkCoord other)
    {
        if (other == null)
            return false;
        else if (other.x == x && other.z == z)
            return true;
        else
            return false;
    }
}

// a class to save the current state of a voxel (for lighting)
public class VoxelState
{
    public byte id;
    public float globalLightPercent;

    public VoxelState()
    {
        id = 0;
        globalLightPercent = 0f;
    }

    public VoxelState(byte _id)
    {
        id = _id;
        globalLightPercent = 0f;
    }
}
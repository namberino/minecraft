using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour
{
    public int cloudHeight = 100;

    [SerializeField] private Texture2D cloudPattern = null;
    [SerializeField] private Material cloudMaterial = null;
    [SerializeField] private World world = null;
    bool[,] cloudData; // array of bools representing where cloud is.

    int cloudTexWidth;

    int cloudTileSize;
    Vector3Int offset;

    Dictionary<Vector2Int, GameObject> clouds = new Dictionary<Vector2Int, GameObject>();

    private void Start()
    {
        cloudTexWidth = cloudPattern.width;
        cloudTileSize = VoxelData.ChunkWidth;
        offset = new Vector3Int(-(cloudTexWidth / 2), 0, -(cloudTexWidth / 2));

        transform.position = new Vector3(VoxelData.WorldCentre, cloudHeight, VoxelData.WorldCentre);

        LoadCloudData();
        CreateClouds();
    }

    // getting the data (color and opacity) of the cloud voxels
    private void LoadCloudData()
    {
        cloudData = new bool[cloudTexWidth, cloudTexWidth];
        Color[] cloudTex = cloudPattern.GetPixels();

        // loop through color array and set bools depending on opacity of colour.
        for (int x = 0; x < cloudTexWidth; x++)
        {
            for (int y = 0; y < cloudTexWidth; y++)
            {
                cloudData[x, y] = (cloudTex[y * cloudTexWidth + x].a > 0);
            }
        }
    }

    // creating clouds
    private void CreateClouds()
    {
        for (int x = 0; x < cloudTexWidth; x += cloudTileSize)
        {
            for (int y = 0; y < cloudTexWidth; y += cloudTileSize)
            { 
                Vector3 position = new Vector3(x, cloudHeight, y);
                clouds.Add(CloudTilePosFromV3(position), CreateCloudTile(CreateCloudMesh(x, y), position));
            }
        }
    }

    // updating the clouds according to the player's position
    public void UpdateClouds()
    {
        for (int x = 0; x < cloudTexWidth; x += cloudTileSize)
        {
            for (int y = 0; y < cloudTexWidth; y += cloudTileSize)
            {
                Vector3 position = world.player.position + new Vector3(x, 0, y) + offset;
                position = new Vector3(RoundToCloud(position.x), cloudHeight, RoundToCloud(position.z));
                Vector2Int cloudPosition = CloudTilePosFromV3(position);

                clouds[cloudPosition].transform.position = position;
            }
        }
    }

    // rounding the cloud size to make it looks blocky
    private int RoundToCloud(float value)
    {
        return Mathf.FloorToInt(value / cloudTileSize) * cloudTileSize;
    }

    // creating a cloud mesh to render 
    private Mesh CreateCloudMesh(int x, int z)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        int vertCount = 0;

        for (int xIncrement = 0; xIncrement < cloudTileSize; xIncrement++)
        {
            for (int zIncrement = 0; zIncrement < cloudTileSize; zIncrement++)
            {
                int xVal = x + xIncrement;
                int zVal = z + zIncrement;

                if (cloudData[xVal, zVal])
                {
                    // add four vertices for cloud face.
                    vertices.Add(new Vector3(xIncrement, 0, zIncrement));
                    vertices.Add(new Vector3(xIncrement, 0, zIncrement + 1));
                    vertices.Add(new Vector3(xIncrement + 1, 0, zIncrement + 1));
                    vertices.Add(new Vector3(xIncrement + 1, 0, zIncrement));

                    // we know what direction our faces are... facing, so we just add them directly.
                    for (int i = 0; i < 4; i++)
                        normals.Add(Vector3.down);

                    // add first triangle.
                    triangles.Add(vertCount + 1);
                    triangles.Add(vertCount);
                    triangles.Add(vertCount + 2);
                    // add second triangle.
                    triangles.Add(vertCount + 2);
                    triangles.Add(vertCount);
                    triangles.Add(vertCount + 3);
                    // increment vertCount.
                    vertCount += 4;
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        return mesh;
    }

    // creating the tiles of the cloud
    private GameObject CreateCloudTile(Mesh mesh, Vector3 position)
    {
        GameObject newCloudTile = new GameObject();
        newCloudTile.transform.position = position;
        newCloudTile.transform.parent = transform;
        newCloudTile.name = "Cloud " + position.x + ", " + position.z;
        MeshFilter mF = newCloudTile.AddComponent<MeshFilter>();
        MeshRenderer mR = newCloudTile.AddComponent<MeshRenderer>();

        mR.material = cloudMaterial;
        mF.mesh = mesh;

        return newCloudTile;
    }

    // getting the cloud tile position
    private Vector2Int CloudTilePosFromV3(Vector3 pos)
    {
        return new Vector2Int(CloudTileCoordFromFloat(pos.x), CloudTileCoordFromFloat(pos.z));
    }

    // a helper method for getting the cloud position
    private int CloudTileCoordFromFloat(float value)
    {
        float a = value / (float)cloudTexWidth; // gets the position using cloudtexture width as units.
        a -= Mathf.FloorToInt(a); // subtract whole numbers to get a 0-1 value representing position in cloud texture.
        int b = Mathf.FloorToInt((float)cloudTexWidth * a); // multiply cloud texture width by a to get position in texture globally.

        return b;
    }
}
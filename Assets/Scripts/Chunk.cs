using System;
using UnityEngine;

enum BlockTypes
{
    Air,
    Solid,
}

struct BlockFaceSettings
{
    public Vector3Int neighbourOffset;
    public int[] triangleIndices;
}

public class Chunk : MonoBehaviour
{
    [SerializeField]
    ChunkData chunkData;

    private bool dirty = true;
    
    private int MaxBlockCount => chunkData.width * chunkData.depth * chunkData.height;
    
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    private readonly BlockFaceSettings bottomFaceSettings = new BlockFaceSettings
    {
        neighbourOffset = Vector3Int.down,
        triangleIndices = new[] { 0, 2, 1, 0, 3, 2 }
    };

    private readonly BlockFaceSettings backFaceSettings = new BlockFaceSettings
    {
        neighbourOffset = Vector3Int.back,
        triangleIndices = new[] { 0, 4, 3, 4, 5, 3 }
    };

    private readonly BlockFaceSettings frontFaceSettings = new BlockFaceSettings
    {
        neighbourOffset = Vector3Int.forward,
        triangleIndices = new[] { 1, 2, 6, 6, 2, 7 }
    };

    private readonly BlockFaceSettings topFaceSettings = new BlockFaceSettings
    {
        neighbourOffset = Vector3Int.up,
        triangleIndices = new[] { 4, 6, 5, 5, 6, 7 }
    };

    private readonly BlockFaceSettings leftFaceSettings = new BlockFaceSettings
    {
        neighbourOffset = Vector3Int.left,
        triangleIndices = new[] { 0, 1, 6, 0, 6, 4 }
    };

    private readonly BlockFaceSettings rightFaceSettings = new BlockFaceSettings
    {
        neighbourOffset = Vector3Int.right,
        triangleIndices = new[] { 3, 5, 2, 2, 5, 7 }
    };

    private void Update()
    {
        if (dirty)
        {
            dirty = false;
            GenerateChunk(chunkData.width, chunkData.height, chunkData.depth);
        }
    }
    
    void OnGUI()
    {
        if (dirty)
        {
            return;
        }
        
        var posY = 25;
        var oldValue = 0;
        
        oldValue = chunkData.width;
        GUI.Label(new Rect(265, posY, 200, 30), $"width: {chunkData.width}");
        chunkData.width = (int)GUI.HorizontalSlider(new Rect(25, posY, 200, 30), chunkData.width, 0, 16);
        dirty |= chunkData.width != oldValue;
        
        oldValue = chunkData.depth;
        GUI.Label(new Rect(265, posY + 25, 200, 30), $"depth: {chunkData.depth}");
        chunkData.depth = (int)GUI.HorizontalSlider(new Rect(25, posY + 25, 200, 30), chunkData.depth, 0, 16);
        dirty |= chunkData.depth != oldValue;
        
        oldValue = chunkData.height;
        GUI.Label(new Rect(265, posY + 50, 200, 30), $"height: {chunkData.height}");
        chunkData.height = (int)GUI.HorizontalSlider(new Rect(25, posY + 50, 200, 30), chunkData.height, 0, 256);
        dirty |= chunkData.height != oldValue;
    }
    
    private BlockTypes GetBlock(int x, int y, int z)
    {
        return BlockTypes.Solid;
    }
    
    private bool IsBlockOutsideBoundaries(int x, int y, int z, int maxX, int maxY, int maxZ)
    {
        return !(x >= 0 && x < maxX &&
                y >= 0 && y < maxY &&
                z >= 0 && z < maxZ);
    }

    private void AddBlockFace(ref int triangleIndex, ref int[] triangles, int verticesStartIndex, int x, int y, int z, in BlockFaceSettings blockFaceSettings)
    {
        var neighbourPosX = x + blockFaceSettings.neighbourOffset.x;
        var neighbourPosY = y + blockFaceSettings.neighbourOffset.y;
        var neighbourPosZ = z + blockFaceSettings.neighbourOffset.z;
        var neighbourBlock = GetBlock(neighbourPosX, neighbourPosY, neighbourPosZ);
        if (neighbourBlock != BlockTypes.Solid || IsBlockOutsideBoundaries(neighbourPosX, neighbourPosY, neighbourPosZ, chunkData.width, chunkData.height, chunkData.depth))
        {
            triangleIndex++;
            triangles[triangleIndex] = verticesStartIndex + blockFaceSettings.triangleIndices[0];
            triangleIndex++;
            triangles[triangleIndex] = verticesStartIndex + blockFaceSettings.triangleIndices[1];
            triangleIndex++;
            triangles[triangleIndex] = verticesStartIndex + blockFaceSettings.triangleIndices[2];

            triangleIndex++;
            triangles[triangleIndex] = verticesStartIndex + blockFaceSettings.triangleIndices[3];
            triangleIndex++;
            triangles[triangleIndex] = verticesStartIndex + blockFaceSettings.triangleIndices[4];
            triangleIndex++;
            triangles[triangleIndex] = verticesStartIndex + blockFaceSettings.triangleIndices[5];
        }
    }
    
    void GenerateChunk(int width, int height, int depth)
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        print("Generating chunk...");
        
        var startTime = Time.realtimeSinceStartupAsDouble;

        vertices = new Vector3[MaxBlockCount * 8];
        var verticesIndex = -1;

        triangles = new int[MaxBlockCount * 36];
        var triangleIndex = -1;

        for (var x = 0; x < width; ++x)
        {
            for (var z = 0; z < depth; ++z)
            {
                for (var y = 0; y < height; ++y)
                {
                    var blockType = GetBlock(x, y, z);

                    if (blockType != BlockTypes.Solid)
                    {
                        continue;
                    }
                    
                    // [B = Back; F = Forward] [B = Bottom, T = Top] [L = Left, R = Right]
                    
                    verticesIndex++;
                    vertices[verticesIndex] = new Vector3(0 + x, 0 + y, 0 + z); // BBL 0
                    
                    verticesIndex++;
                    vertices[verticesIndex] = new Vector3(0 + x, 0 + y, 1 + z); // FBL 1
                    
                    verticesIndex++;
                    vertices[verticesIndex] = new Vector3(1 + x, 0 + y, 1 + z); // FBR 2
                    
                    verticesIndex++;
                    vertices[verticesIndex] = new Vector3(1 + x, 0 + y, 0 + z); // BBR 3
                    
                    verticesIndex++;
                    vertices[verticesIndex] = new Vector3(0 + x, 1 + y, 0 + z); // BTL 4
                    
                    verticesIndex++;
                    vertices[verticesIndex] = new Vector3(1 + x, 1 + y, 0 + z); // BTR 5
                    
                    verticesIndex++;
                    vertices[verticesIndex] = new Vector3(0 + x, 1 + y, 1 + z); // FTL 6
                    
                    verticesIndex++;
                    vertices[verticesIndex] = new Vector3(1 + x, 1 + y, 1 + z); // FTR 7

                    // Start index for the current block of vertices
                    var verticesStartIndex = verticesIndex - 7;

                    AddBlockFace(ref triangleIndex, ref triangles, verticesStartIndex, x, y, z, in bottomFaceSettings);
                    AddBlockFace(ref triangleIndex, ref triangles, verticesStartIndex, x, y, z, in backFaceSettings);
                    AddBlockFace(ref triangleIndex, ref triangles, verticesStartIndex, x, y, z, in frontFaceSettings);
                    AddBlockFace(ref triangleIndex, ref triangles, verticesStartIndex, x, y, z, in topFaceSettings);
                    AddBlockFace(ref triangleIndex, ref triangles, verticesStartIndex, x, y, z, in leftFaceSettings);
                    AddBlockFace(ref triangleIndex, ref triangles, verticesStartIndex, x, y, z, in rightFaceSettings);
                }
            }
        }
        
        mesh.Clear();

        Array.Resize(ref vertices, verticesIndex + 1);
        mesh.vertices = vertices;
        
        Array.Resize(ref triangles, triangleIndex + 1);
        mesh.triangles = triangles;

        var duration = Time.realtimeSinceStartupAsDouble - startTime;
        print($"Duration: {(duration * 1000)}ms");
    }
}

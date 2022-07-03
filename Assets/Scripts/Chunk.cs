using System;
using UnityEngine;

internal enum BlockTypes
{
    Air,
    Solid,
}

internal struct BlockFaceSettings
{
    public Vector3Int NeighbourOffset;
    public int[] TriangleIndices;
}

public class Chunk : MonoBehaviour
{
    [SerializeField]
    private ChunkData chunkData;

    private bool dirty = true;
    
    private int MaxBlockCount => chunkData.width * chunkData.depth * chunkData.height;
    
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    private readonly BlockFaceSettings bottomFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.down,
        TriangleIndices = new[] { 0, 2, 1, 0, 3, 2 }
    };

    private readonly BlockFaceSettings backFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.back,
        TriangleIndices = new[] { 0, 4, 3, 4, 5, 3 }
    };

    private readonly BlockFaceSettings frontFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.forward,
        TriangleIndices = new[] { 1, 2, 6, 6, 2, 7 }
    };

    private readonly BlockFaceSettings topFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.up,
        TriangleIndices = new[] { 4, 6, 5, 5, 6, 7 }
    };

    private readonly BlockFaceSettings leftFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.left,
        TriangleIndices = new[] { 0, 1, 6, 0, 6, 4 }
    };

    private readonly BlockFaceSettings rightFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.right,
        TriangleIndices = new[] { 3, 5, 2, 2, 5, 7 }
    };

    private void Update()
    {
        if (dirty)
        {
            dirty = false;
            GenerateChunk(chunkData.width, chunkData.height, chunkData.depth);
        }
    }

    private void OnGUI()
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
    
    private static bool IsBlockOutsideBoundaries(int x, int y, int z, int maxX, int maxY, int maxZ)
    {
        return !(x >= 0 && x < maxX &&
                y >= 0 && y < maxY &&
                z >= 0 && z < maxZ);
    }

    private static void AddBlockFace(ref int triangleIndex, ref int[] triangles, int verticesStartIndex, in BlockFaceSettings blockFaceSettings)
    {
        triangleIndex++;
        triangles[triangleIndex] = verticesStartIndex + blockFaceSettings.TriangleIndices[0];
        triangleIndex++;
        triangles[triangleIndex] = verticesStartIndex + blockFaceSettings.TriangleIndices[1];
        triangleIndex++;
        triangles[triangleIndex] = verticesStartIndex + blockFaceSettings.TriangleIndices[2];

        triangleIndex++;
        triangles[triangleIndex] = verticesStartIndex + blockFaceSettings.TriangleIndices[3];
        triangleIndex++;
        triangles[triangleIndex] = verticesStartIndex + blockFaceSettings.TriangleIndices[4];
        triangleIndex++;
        triangles[triangleIndex] = verticesStartIndex + blockFaceSettings.TriangleIndices[5];
    }
    
    /// <summary>
    /// Returns true if the neighbour of the specified face is solid
    /// </summary>
    /// x, y and z: The current blocks coordinates
    /// <param name="blockFaceSettings">Settings for the face you want to check</param>
    private bool IsFaceNeighbourSolid(int x, int y, int z, in BlockFaceSettings blockFaceSettings)
    {
        var neighbourPosX = x + blockFaceSettings.NeighbourOffset.x;
        var neighbourPosY = y + blockFaceSettings.NeighbourOffset.y;
        var neighbourPosZ = z + blockFaceSettings.NeighbourOffset.z;
        
        // Render faces that point outside of the chunk anyways for now, could be debug flagged as well
        // So dont make neighbour blocks that are outside of the boundaries count as solid
        var isNeighbourOutsideOfBoundary = IsBlockOutsideBoundaries(
            neighbourPosX, 
            neighbourPosY, 
            neighbourPosZ, 
            chunkData.width, 
            chunkData.height, 
            chunkData.depth
        );
        
        if (isNeighbourOutsideOfBoundary)
        {
            return false;
        }
        
        var neighbourBlock = GetBlock(neighbourPosX, neighbourPosY, neighbourPosZ);
        return neighbourBlock == BlockTypes.Solid;
    }
    
    private void GenerateChunk(int width, int height, int depth)
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        print($"Generating chunk (width: {chunkData.width}, height: {chunkData.height}, depth: {chunkData.depth}) ...");
        
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

                    var bottomSolid = IsFaceNeighbourSolid(x, y, z, bottomFaceSettings);
                    var backSolid = IsFaceNeighbourSolid(x, y, z, backFaceSettings);
                    var frontSolid = IsFaceNeighbourSolid(x, y, z, frontFaceSettings);
                    var topSolid = IsFaceNeighbourSolid(x, y, z, topFaceSettings);
                    var leftSolid = IsFaceNeighbourSolid(x, y, z, leftFaceSettings);
                    var rightSolid = IsFaceNeighbourSolid(x, y, z, rightFaceSettings);

                    if (bottomSolid && backSolid && frontSolid && topSolid && leftSolid && rightSolid)
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

                    if (!bottomSolid)
                    {
                        AddBlockFace(ref triangleIndex, ref triangles, verticesStartIndex, in bottomFaceSettings);
                    }
                    
                    if (!backSolid)
                    {
                        AddBlockFace(ref triangleIndex, ref triangles, verticesStartIndex, in backFaceSettings);
                    }
                    
                    if (!frontSolid)
                    {
                        AddBlockFace(ref triangleIndex, ref triangles, verticesStartIndex, in frontFaceSettings);
                    }
                    
                    if (!topSolid)
                    {
                        AddBlockFace(ref triangleIndex, ref triangles, verticesStartIndex, in topFaceSettings);
                    }
                    
                    if (!leftSolid)
                    {
                        AddBlockFace(ref triangleIndex, ref triangles, verticesStartIndex, in leftFaceSettings);
                    }
                    
                    if (!rightSolid)
                    {
                        AddBlockFace(ref triangleIndex, ref triangles, verticesStartIndex, in rightFaceSettings);
                    }
                }
            }
        }
        
        mesh.Clear();

        Array.Resize(ref vertices, verticesIndex + 1);
        mesh.vertices = vertices;
        
        Array.Resize(ref triangles, triangleIndex + 1);
        mesh.triangles = triangles;

        var duration = Time.realtimeSinceStartupAsDouble - startTime;
        print($"Vertices in the buffer: {verticesIndex}");
        print($"Duration: {(duration * 1000)}ms");
    }
}

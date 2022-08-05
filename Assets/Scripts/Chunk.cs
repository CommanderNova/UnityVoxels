using System;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

internal enum BlockTypes
{
    Air,
    Solid,
}

internal enum DebugGenerationTypes
{
    Full,
    Seconds,
    Thirds,
    Fourths,
    Random,
}

internal struct BlockFaceSettings
{
    public Vector3Int NeighbourOffset;
    public Vector3[] VertexIndices;
    public int[] TriangleIndices;
}

public class Chunk : MonoBehaviour
{
    [SerializeField]
    private ChunkData chunkData;

    private DebugGenerationTypes generationType = DebugGenerationTypes.Full;
    private int seed = 0;
    private bool dirty = true;
    
    private int MaxBlockCount => chunkData.width * chunkData.depth * chunkData.height;
    
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    
    // [L = Left, R = Right] [B = Bottom, T = Top] [B = Back; F = Front]
    
    private readonly BlockFaceSettings bottomFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.down,
        VertexIndices = new[] {
            new Vector3(0, 0, 0), // LBB 0
            new Vector3(0, 0, 1), // LBF 1
            new Vector3(1, 0, 1), // RBF 2
            new Vector3(1, 0, 0), // RBB 3
        },
        TriangleIndices = new[] { 0, 3, 1, 3, 2, 1 },
    };

    private readonly BlockFaceSettings backFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.back,
        VertexIndices = new[] {
            new Vector3(0, 0, 0), // LBB 0
            new Vector3(0, 1, 0), // LTB 1
            new Vector3(1, 1, 0), // RTB 2
            new Vector3(1, 0, 0), // RBB 3
        },
        TriangleIndices = new[] { 0, 1, 3, 3, 1, 2 },
    };

    private readonly BlockFaceSettings frontFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.forward,
        VertexIndices = new[] {
            new Vector3(0, 0, 1), // LBF 0
            new Vector3(0, 1, 1), // LTF 1
            new Vector3(1, 1, 1), // RTF 2
            new Vector3(1, 0, 1), // RBF 3
        },
        TriangleIndices = new[] { 0, 2, 1, 0, 3, 2 }
    };

    private readonly BlockFaceSettings topFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.up,
        VertexIndices = new[] {
            new Vector3(0, 1, 0), // LTB 0
            new Vector3(0, 1, 1), // LTF 1
            new Vector3(1, 1, 1), // RTF 2
            new Vector3(1, 1, 0), // RTB 3
        },
        TriangleIndices = new[] { 0, 1, 3, 3, 1, 2 }
    };

    private readonly BlockFaceSettings leftFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.left,
        VertexIndices = new[] {
            new Vector3(0, 0, 0), // LBB 0
            new Vector3(0, 0, 1), // LBF 1
            new Vector3(0, 1, 1), // LTF 2
            new Vector3(0, 1, 0), // LTB 3
        },
        TriangleIndices = new[] { 0, 1, 2, 0, 2, 3 }
    };

    private readonly BlockFaceSettings rightFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.right,
        VertexIndices = new[] {
            new Vector3(1, 0, 0), // RBB 0
            new Vector3(1, 0, 1), // RBF 1
            new Vector3(1, 1, 1), // RTF 2
            new Vector3(1, 1, 0), // RTB 3
        },
        TriangleIndices = new[] { 0, 3, 1, 1, 3, 2 }
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
        chunkData.width = (int)GUI.HorizontalSlider(new Rect(25, posY, 200, 25), chunkData.width, 0, 16);
        dirty |= chunkData.width != oldValue;
        
        oldValue = chunkData.depth;
        GUI.Label(new Rect(265, posY + 25, 200, 30), $"depth: {chunkData.depth}");
        chunkData.depth = (int)GUI.HorizontalSlider(new Rect(25, posY + 25, 200, 25), chunkData.depth, 0, 16);
        dirty |= chunkData.depth != oldValue;
        
        oldValue = chunkData.height;
        GUI.Label(new Rect(265, posY + 50, 200, 30), $"height: {chunkData.height}");
        chunkData.height = (int)GUI.HorizontalSlider(new Rect(25, posY + 50, 200, 25), chunkData.height, 0, 256);
        dirty |= chunkData.height != oldValue;

        if (GUI.Button(new Rect(350, posY, 100, 30), "Regenerate"))
        {
            dirty = true;
        }
        
        var i = 0;
        foreach (DebugGenerationTypes genType in Enum.GetValues(typeof(DebugGenerationTypes)))
        {
            posY = 100 + (i * 35);
            
            if (GUI.Button(new Rect(25, posY, 100, 30), genType.ToString()))
            {
                if (genType == DebugGenerationTypes.Random)
                {
                    seed = Random.Range(0, int.MaxValue);
                }
                
                generationType = genType;
                dirty = true;
            }
            
            i++;
        }
    }
    
    private BlockTypes GetBlock(int x, int y, int z)
    {
        var blockNum = x + y + z;
        
        switch (generationType)
        {
            case DebugGenerationTypes.Full:
                return BlockTypes.Solid;
            case DebugGenerationTypes.Seconds:
                return blockNum % 2 == 1 ? BlockTypes.Solid : BlockTypes.Air;
            case DebugGenerationTypes.Thirds:
                return blockNum % 3 == 1 ? BlockTypes.Solid : BlockTypes.Air;
            case DebugGenerationTypes.Fourths:
                return blockNum % 4 == 1 ? BlockTypes.Solid : BlockTypes.Air;
            case DebugGenerationTypes.Random:
                Random.InitState(Hash(seed, x, y, z));
                return Mathf.RoundToInt(Random.value) == 1 ? BlockTypes.Solid : BlockTypes.Air;
            default:
                throw new Exception("Generation type not found!");
        }
    }
    
    private static int Hash(int seed, params int[] numbers)
    {
        unchecked // Allow arithmetic overflow, numbers will just "wrap around"
        {
            var hashcode = seed;
            foreach (var number in numbers)
            {
                hashcode = hashcode * 7302013 ^ number;
            }
            return hashcode;
        }
    }
    
    private static bool IsBlockOutsideBoundaries(int x, int y, int z, int maxX, int maxY, int maxZ)
    {
        return !(x >= 0 && x < maxX &&
                y >= 0 && y < maxY &&
                z >= 0 && z < maxZ);
    }

    private static void AddBlockVertices(in int x, in int y, in int z, ref int verticesIndex, ref Vector3[] vertices, in BlockFaceSettings blockFaceSettings)
    {
        verticesIndex++;
        vertices[verticesIndex] = blockFaceSettings.VertexIndices[0];
        vertices[verticesIndex].x += x;
        vertices[verticesIndex].y += y;
        vertices[verticesIndex].z += z;
        
        verticesIndex++;
        vertices[verticesIndex] = blockFaceSettings.VertexIndices[1];
        vertices[verticesIndex].x += x;
        vertices[verticesIndex].y += y;
        vertices[verticesIndex].z += z;
        
        verticesIndex++;
        vertices[verticesIndex] = blockFaceSettings.VertexIndices[2];
        vertices[verticesIndex].x += x;
        vertices[verticesIndex].y += y;
        vertices[verticesIndex].z += z;
        
        verticesIndex++;
        vertices[verticesIndex] = blockFaceSettings.VertexIndices[3];
        vertices[verticesIndex].x += x;
        vertices[verticesIndex].y += y;
        vertices[verticesIndex].z += z;
    }

    private static void AddBlockTriangles(ref int triangleIndex, ref int[] triangles, int verticesStartIndex, in BlockFaceSettings blockFaceSettings)
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
        mesh = new Mesh
        {
            indexFormat = IndexFormat.UInt32,
        };

        GetComponent<MeshFilter>().mesh = mesh;
        
        print($"Generating chunk (width: {chunkData.width}, height: {chunkData.height}, depth: {chunkData.depth}) ...");
        
        var startTime = Time.realtimeSinceStartupAsDouble;

        vertices = new Vector3[MaxBlockCount * 24];
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

                    if (!bottomSolid)
                    {
                        AddBlockVertices(in x, in y, in z, ref verticesIndex, ref vertices, in bottomFaceSettings);
                        
                        var verticesStartIndex = verticesIndex - 3;
                        AddBlockTriangles(ref triangleIndex, ref triangles, verticesStartIndex, in bottomFaceSettings);
                    }
                    
                    if (!backSolid)
                    {
                        AddBlockVertices(in x, in y, in z, ref verticesIndex, ref vertices, in backFaceSettings);
                        
                        var verticesStartIndex = verticesIndex - 3;
                        AddBlockTriangles(ref triangleIndex, ref triangles, verticesStartIndex, in backFaceSettings);
                    }
                    
                    if (!frontSolid)
                    {
                        AddBlockVertices(in x, in y, in z, ref verticesIndex, ref vertices, in frontFaceSettings);
                        
                        var verticesStartIndex = verticesIndex - 3;
                        AddBlockTriangles(ref triangleIndex, ref triangles, verticesStartIndex, in frontFaceSettings);
                    }
                    
                    if (!topSolid)
                    {
                        AddBlockVertices(in x, in y, in z, ref verticesIndex, ref vertices, in topFaceSettings);
                        
                        var verticesStartIndex = verticesIndex - 3;
                        AddBlockTriangles(ref triangleIndex, ref triangles, verticesStartIndex, in topFaceSettings);
                    }
                    
                    if (!leftSolid)
                    {
                        AddBlockVertices(in x, in y, in z, ref verticesIndex, ref vertices, in leftFaceSettings);
                        
                        var verticesStartIndex = verticesIndex - 3;
                        AddBlockTriangles(ref triangleIndex, ref triangles, verticesStartIndex, in leftFaceSettings);
                    }
                    
                    if (!rightSolid)
                    {
                        AddBlockVertices(in x, in y, in z, ref verticesIndex, ref vertices, in rightFaceSettings);
                        
                        var verticesStartIndex = verticesIndex - 3;
                        AddBlockTriangles(ref triangleIndex, ref triangles, verticesStartIndex, in rightFaceSettings);
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
        print($"Vertices in the buffer: {verticesIndex + 1}");
        print($"Duration: {(duration * 1000)}ms");
    }
}

using System;
using TreeEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class Chunk : MonoBehaviour
{
    [SerializeField]
    public ChunkData chunkData;

    public const int MaxChunkHorizontalSize = 16;

    [HideInInspector]
    public Texture2D debugNoise;
    

    internal DebugGenerationTypes generationType = DebugGenerationTypes.Procedural;

    private Perlin perlin = new Perlin();

    private Vector3Int origin;
    public Vector3Int Origin => origin;

    private int seed;
    public int Seed
    {
        get => seed;
        set
        {
            seed = value;
            
            perlin = new Perlin();
            perlin.SetSeed(seed);
        }
    }
    
    [HideInInspector]
    public bool dirty = true;
    
    private int MaxBlockCount => chunkData.width * chunkData.depth * chunkData.height;
    
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;

    public void Initialize(Vector3 position)
    {
        origin = new Vector3Int(
            (int)Math.Round(position.x),
            (int)Math.Round(position.y),
            (int)Math.Round(position.z)
        );
    }
    
    private void Update()
    {
        if (dirty)
        {
            dirty = false;
            GenerateChunk(chunkData.width, chunkData.height, chunkData.depth);
        }
    }

    public float GetNoise(int noiseSeed, int x, int z)
    {
        return Mathf.PerlinNoise
        (
            ((float)x / chunkData.width) + noiseSeed, 
            ((float)z / chunkData.depth) + noiseSeed
        );
    }

    private BlockTypes GetBlock(int x, int y, int z)
    {
        if (generationType == DebugGenerationTypes.Procedural)
        {
            var noise = GetNoise(Seed, x, z);
            var surfaceY = noise * 60;
            
            // var frequency = 0.2;
            // var amplitude = 10;
            // var xOffset = Math.Sin(x * frequency) * amplitude;
            // var zOffset = Math.Sin(z * frequency) * amplitude;
            // var surfaceY = 100 + xOffset + zOffset;
            
            return y < surfaceY ? BlockTypes.Solid : BlockTypes.Air;
        }
        else
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
                    Random.InitState(Hash(Seed, x, y, z));
                    return Mathf.RoundToInt(Random.value) == 1 ? BlockTypes.Solid : BlockTypes.Air;
                default:
                    throw new Exception("Generation type not found!");
            }
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

    /// <summary>
    /// Returns true if the neighbour of the specified face is solid
    /// </summary>
    /// x, y and z: The current blocks coordinates
    /// <param name="blockFaceSettings">Settings for the face you want to check</param>
    private bool IsFaceNeighbourSolid(in int x, in int y, in int z, in BlockFaceSettings blockFaceSettings)
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

        uvs = new Vector2[vertices.Length];
        var uvsIndex = -1;
        
        triangles = new int[MaxBlockCount * 36];
        var triangleIndex = -1;

        for (var i = 0; i < width; ++i)
        {
            for (var j = 0; j < depth; ++j)
            {
                for (var k = 0; k < height; ++k)
                {
                    var x = i + origin.x;
                    var z = j + origin.z;
                    var y = k + origin.y;
                    
                    var blockType = GetBlock(x, y, z);

                    if (blockType != BlockTypes.Solid)
                    {
                        continue;
                    }

                    var bottomSolid = IsFaceNeighbourSolid(in x, in y, in z, in BlockRendering.bottomFaceSettings);
                    var backSolid = IsFaceNeighbourSolid(in x, in y, in z, in BlockRendering.backFaceSettings);
                    var frontSolid = IsFaceNeighbourSolid(in x, in y, in z, in BlockRendering.frontFaceSettings);
                    var topSolid = IsFaceNeighbourSolid(in x, in y, in z, in BlockRendering.topFaceSettings);
                    var leftSolid = IsFaceNeighbourSolid(in x, in y, in z, in BlockRendering.leftFaceSettings);
                    var rightSolid = IsFaceNeighbourSolid(in x, in y, in z, in BlockRendering.rightFaceSettings);

                    if (bottomSolid && backSolid && frontSolid && topSolid && leftSolid && rightSolid)
                    {
                        continue;
                    }

                    if (!bottomSolid)
                    {
                        BlockRendering.AddBlockVertices(in x, in y, in z, ref verticesIndex, ref vertices, in BlockRendering.bottomFaceSettings);
                        
                        var verticesStartIndex = verticesIndex - 3;
                        BlockRendering.AddBlockTriangles(ref triangleIndex, ref triangles, in verticesStartIndex, in BlockRendering.bottomFaceSettings);
                        BlockRendering.AddBlockUvs(ref uvsIndex, ref uvs, in BlockRendering.bottomFaceSettings);
                    }
                    
                    if (!backSolid)
                    {
                        BlockRendering.AddBlockVertices(in x, in y, in z, ref verticesIndex, ref vertices, in BlockRendering.backFaceSettings);
                        
                        var verticesStartIndex = verticesIndex - 3;
                        BlockRendering.AddBlockTriangles(ref triangleIndex, ref triangles, in verticesStartIndex, in BlockRendering.backFaceSettings);
                        BlockRendering.AddBlockUvs(ref uvsIndex, ref uvs, in BlockRendering.backFaceSettings);
                    }
                    
                    if (!frontSolid)
                    {
                        BlockRendering.AddBlockVertices(in x, in y, in z, ref verticesIndex, ref vertices, in BlockRendering.frontFaceSettings);
                        
                        var verticesStartIndex = verticesIndex - 3;
                        BlockRendering.AddBlockTriangles(ref triangleIndex, ref triangles, in verticesStartIndex, in BlockRendering.frontFaceSettings);
                        BlockRendering.AddBlockUvs(ref uvsIndex, ref uvs, in BlockRendering.frontFaceSettings);
                    }
                    
                    if (!topSolid)
                    {
                        BlockRendering.AddBlockVertices(in x, in y, in z, ref verticesIndex, ref vertices, in BlockRendering.topFaceSettings);
                        
                        var verticesStartIndex = verticesIndex - 3;
                        BlockRendering.AddBlockTriangles(ref triangleIndex, ref triangles, in verticesStartIndex, in BlockRendering.topFaceSettings);
                        BlockRendering.AddBlockUvs(ref uvsIndex, ref uvs, in BlockRendering.topFaceSettings);
                    }
                    
                    if (!leftSolid)
                    {
                        BlockRendering.AddBlockVertices(in x, in y, in z, ref verticesIndex, ref vertices, in BlockRendering.leftFaceSettings);
                        
                        var verticesStartIndex = verticesIndex - 3;
                        BlockRendering.AddBlockTriangles(ref triangleIndex, ref triangles, in verticesStartIndex, in BlockRendering.leftFaceSettings);
                        BlockRendering.AddBlockUvs(ref uvsIndex, ref uvs, in BlockRendering.leftFaceSettings);
                    }
                    
                    if (!rightSolid)
                    {
                        BlockRendering.AddBlockVertices(in x, in y, in z, ref verticesIndex, ref vertices, in BlockRendering.rightFaceSettings);
                        
                        var verticesStartIndex = verticesIndex - 3;
                        BlockRendering.AddBlockTriangles(ref triangleIndex, ref triangles, in verticesStartIndex, in BlockRendering.rightFaceSettings);
                        BlockRendering.AddBlockUvs(ref uvsIndex, ref uvs, in BlockRendering.rightFaceSettings);
                    }
                }
            }
        }
        
        mesh.Clear();

        Array.Resize(ref vertices, verticesIndex + 1);
        mesh.vertices = vertices;
        
        Array.Resize(ref uvs, uvsIndex + 1);
        mesh.uv = uvs;
        
        Array.Resize(ref triangles, triangleIndex + 1);
        mesh.triangles = triangles;

        var duration = Time.realtimeSinceStartupAsDouble - startTime;
        print($"Vertices in the buffer: {verticesIndex + 1}");
        print($"Duration: {(duration * 1000)}ms");
    }
}

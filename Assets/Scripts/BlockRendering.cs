using UnityEngine;

internal struct BlockFaceSettings
{
    public Vector3Int NeighbourOffset;
    public Vector3[] VertexIndices;
    public int[] TriangleIndices;
    public Vector2[] UvIndices;
}

internal enum BlockTypes
{
    Air,
    Solid,
}

public static class BlockRendering
{
    // [L = Left, R = Right] [B = Bottom, T = Top] [B = Back; F = Front]
    
    internal static readonly BlockFaceSettings bottomFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.down,
        VertexIndices = new[] {
            new Vector3(0, 0, 0), // LBB 0
            new Vector3(0, 0, 1), // LBF 1
            new Vector3(1, 0, 1), // RBF 2
            new Vector3(1, 0, 0), // RBB 3
        },
        TriangleIndices = new[] { 0, 3, 1, 3, 2, 1 },
        UvIndices = new[] 
        {
            new Vector2(0.33333f, 1.00f),
            new Vector2(0.33333f, 0.75f),
            new Vector2(0.66666f, 0.75f),
            new Vector2(0.66666f, 1.00f),
        },
    };

    internal static readonly BlockFaceSettings backFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.back,
        VertexIndices = new[] {
            new Vector3(0, 0, 0), // LBB 0
            new Vector3(0, 1, 0), // LTB 1
            new Vector3(1, 1, 0), // RTB 2
            new Vector3(1, 0, 0), // RBB 3
        },
        TriangleIndices = new[] { 0, 1, 3, 3, 1, 2 },
        UvIndices = new[] 
        {
            new Vector2(0.33333f, 0.00f),
            new Vector2(0.33333f, 0.25f),
            new Vector2(0.66666f, 0.25f),
            new Vector2(0.66666f, 0.00f),
        },
    };

    internal static readonly BlockFaceSettings frontFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.forward,
        VertexIndices = new[] {
            new Vector3(0, 0, 1), // LBF 0
            new Vector3(0, 1, 1), // LTF 1
            new Vector3(1, 1, 1), // RTF 2
            new Vector3(1, 0, 1), // RBF 3
        },
        TriangleIndices = new[] { 0, 2, 1, 0, 3, 2 },
        UvIndices = new[] 
        {
            new Vector2(0.33333f, 0.75f),
            new Vector2(0.33333f, 0.50f),
            new Vector2(0.66666f, 0.50f),
            new Vector2(0.66666f, 0.75f),
        },
    };

    internal static readonly BlockFaceSettings topFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.up,
        VertexIndices = new[] {
            new Vector3(0, 1, 0), // LTB 0
            new Vector3(0, 1, 1), // LTF 1
            new Vector3(1, 1, 1), // RTF 2
            new Vector3(1, 1, 0), // RTB 3
        },
        TriangleIndices = new[] { 0, 1, 3, 3, 1, 2 },
        UvIndices = new[] 
        {
            new Vector2(0.33333f, 0.25f),
            new Vector2(0.33333f, 0.50f),
            new Vector2(0.66666f, 0.50f),
            new Vector2(0.66666f, 0.25f),
        },
    };

    internal static readonly BlockFaceSettings leftFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.left,
        VertexIndices = new[] {
            new Vector3(0, 0, 0), // LBB 0
            new Vector3(0, 0, 1), // LBF 1
            new Vector3(0, 1, 1), // LTF 2
            new Vector3(0, 1, 0), // LTB 3
        },
        TriangleIndices = new[] { 0, 1, 2, 0, 2, 3 },
        UvIndices = new[] 
        {
            new Vector2(0.00f, 0.25f),
            new Vector2(0.00f, 0.50f),
            new Vector2(0.33333f, 0.50f),
            new Vector2(0.33333f, 0.25f),
        },
    };

    internal static readonly BlockFaceSettings rightFaceSettings = new BlockFaceSettings
    {
        NeighbourOffset = Vector3Int.right,
        VertexIndices = new[] {
            new Vector3(1, 0, 0), // RBB 0
            new Vector3(1, 0, 1), // RBF 1
            new Vector3(1, 1, 1), // RTF 2
            new Vector3(1, 1, 0), // RTB 3
        },
        TriangleIndices = new[] { 0, 3, 1, 1, 3, 2 },
        UvIndices = new[] 
        {
            new Vector2(1.00f, 0.25f),
            new Vector2(1.00f, 0.50f),
            new Vector2(0.66666f, 0.50f),
            new Vector2(0.66666f, 0.25f),
        },
    };

    internal static void AddBlockVertices(in int x, in int y, in int z, ref int verticesIndex, ref Vector3[] vertices, in BlockFaceSettings blockFaceSettings)
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

    internal static void AddBlockTriangles(ref int triangleIndex, ref int[] triangles, in int verticesStartIndex, in BlockFaceSettings blockFaceSettings)
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
    
    internal static void AddBlockUvs(ref int uvIndex, ref Vector2[] uvs, in BlockFaceSettings blockFaceSettings)
    {
        uvIndex++;
        uvs[uvIndex] = blockFaceSettings.UvIndices[0];
        uvIndex++;
        uvs[uvIndex] = blockFaceSettings.UvIndices[1];
        uvIndex++;
        uvs[uvIndex] = blockFaceSettings.UvIndices[2];
        uvIndex++;
        uvs[uvIndex] = blockFaceSettings.UvIndices[3];
    }
    
}

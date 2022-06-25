using UnityEngine;

enum BlockTypes
{
    Air,
    Solid,
}

public class Chunk : MonoBehaviour
{
    [SerializeField]
    private int width = 1;
    [SerializeField]
    private int depth = 1;
    [SerializeField]
    private int height = 1;

    private int MaxBlockCount => width * depth * height;
    
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    
    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

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
                    
                    // Bottom Face
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 0;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 2;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 1;
                    
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 0;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 3;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 2;
                    
                    // Back Face
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 0;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 4;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 3;
                    
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 4;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 5;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 3;
                    
                    // Front Face
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 1;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 2;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 6;
                    
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 6;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 2;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 7;
                    
                    // Top Face
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 4;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 6;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 5;
                    
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 5;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 6;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 7;
                    
                    // Left Face
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 0;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 1;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 6;
                    
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 0;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 6;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 4;
                    
                    // Right Face
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 3;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 5;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 2;
                    
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 2;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 5;
                    triangleIndex++;
                    triangles[triangleIndex] = verticesStartIndex + 7;
                }
            }
        }
        
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        var duration = Time.realtimeSinceStartupAsDouble - startTime;
        print("Duration: " + duration);
    }
    
    private BlockTypes GetBlock(int x, int y, int z)
    {
        return BlockTypes.Solid;
    }
}

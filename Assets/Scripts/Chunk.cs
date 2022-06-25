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

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    
    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        var startTime = Time.realtimeSinceStartupAsDouble;
        for (var x = 0; x < width; ++x)
        {
            for (var z = 0; z < depth; ++z)
            {
                for (var y = 0; y < height; ++y)
                {
                    // [B = Back; F = Forward] [B = Bottom, T = Top] [L = Left, R = Right]
                    vertices = new[]
                    {
                        // Bottom
                        new Vector3(0, 0, 0), // BBL 0
                        new Vector3(0, 0, 1), // FBL 1
                        new Vector3(1, 0, 1), // FBR 2
                        new Vector3(1, 0, 0), // BBR 3
                        
                        // Top
                        new Vector3(0, 1, 0), // BTL 4
                        new Vector3(1, 1, 0), // BTR 5
                        new Vector3(0, 1, 1), // FTL 6
                        new Vector3(1, 1, 1), // FTR 7
                    };

                    triangles = new[]
                    {
                        // Bottom Face
                        0, 2, 1,
                        0, 3, 2,
                        
                        // Back Face
                        0, 4, 3,
                        4, 5, 3,
                        
                        // Front Face
                        1, 2, 6,
                        6, 2, 7,
                        
                        // Top Face
                        4, 6, 5,
                        5, 6, 7, 
                        
                        // Left Face
                        0, 1, 6,
                        0, 6, 4,
                        
                        // Right Face
                        3, 5, 2,
                        2, 5, 7,
                    };
                    
                    mesh.Clear();

                    mesh.vertices = vertices;
                    mesh.triangles = triangles;
                }
            }
        }

        var duration = Time.realtimeSinceStartupAsDouble - startTime;
        print("Duration: " + duration);
    }
    
    private BlockTypes GetBlock(int x, int y, int z)
    {
        return BlockTypes.Solid;
    }
}

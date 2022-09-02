using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField]
    private int renderDistance = 8;

    [SerializeField]
    private GameObject chunkTemplate;
    
    void Start()
    {
        var center = GetGridPosition(transform.position);

        var halfChunkSize = Chunk.MaxChunkHorizontalSize / 2;
        for (var x = Chunk.MaxChunkHorizontalSize * -renderDistance; x < Chunk.MaxChunkHorizontalSize * renderDistance; x += Chunk.MaxChunkHorizontalSize)
        {
            for (var z = Chunk.MaxChunkHorizontalSize * -renderDistance; z < Chunk.MaxChunkHorizontalSize * renderDistance; z += Chunk.MaxChunkHorizontalSize)
            {
                var newPosition = new Vector3
                (
                    center.x + x,
                    center.y,
                    center.z + z
                );

                newPosition = GetGridPosition(newPosition);

                Instantiate(chunkTemplate, newPosition, Quaternion.identity, transform);
            }
        }
    }

    Vector3 GetGridPosition(Vector3 position)
    {
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);
        position.z = Mathf.Round(position.z);
        
        return position;
    }
}

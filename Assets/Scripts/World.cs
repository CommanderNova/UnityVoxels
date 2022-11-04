using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField]
    private int renderRadius = 8;

    [SerializeField]
    private GameObject chunkTemplate;
    
    void Start()
    {
        var center = GetGridPosition(transform.position);

        for (var x = Chunk.MaxChunkHorizontalSize * -renderRadius; x < Chunk.MaxChunkHorizontalSize * renderRadius; x += Chunk.MaxChunkHorizontalSize)
        {
            for (var z = Chunk.MaxChunkHorizontalSize * -renderRadius; z < Chunk.MaxChunkHorizontalSize * renderRadius; z += Chunk.MaxChunkHorizontalSize)
            {
                var newPosition = new Vector3
                (
                    center.x + x,
                    center.y,
                    center.z + z
                );

                newPosition = GetGridPosition(newPosition);

                var newObject = Instantiate(chunkTemplate, Vector3.zero, Quaternion.identity, transform);
                var chunk = newObject.GetComponent<Chunk>();
                if (chunk)
                {
                    chunk.Initialize(newPosition);
                    chunk.chunkData = Instantiate(chunk.chunkData);
                }
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

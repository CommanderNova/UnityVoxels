using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField]
    private int renderRadius = 8;

    [SerializeField]
    private GameObject chunkTemplate;

    private int Seed;
    
    // TODO: these are only used when pressing "REGENERATE ALL" button
    public float noiseZoom = 1.0f;
    public float baseSurfaceLevel = 100.0f;
    public float noiseMultiplier = 20.0f;

    private readonly List<Chunk> allChunks = new List<Chunk>();

    public void RegenerateWorld()
    {
        var startTime = Time.realtimeSinceStartupAsDouble;

        foreach (var chunk in allChunks)
        {
            chunk.noiseZoom = noiseZoom;
            chunk.baseSurfaceLevel = baseSurfaceLevel;
            chunk.noiseMultiplier = noiseMultiplier;

            chunk.RegenerateChunk();
        }
        
        var duration = Time.realtimeSinceStartupAsDouble - startTime;
        print($"Chunks build: <{allChunks.Count}> in duration: <{duration * 1000}ms> or <{duration}s>");
    }
    
    void Start()
    {
        Seed = 20; //Random.Range(0, 200000);
        print("Seed is: " + Seed);
        
        var center = GetGridPosition(transform.position);
        var startTime = Time.realtimeSinceStartupAsDouble;
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
                    chunk.Initialize(newPosition, Seed);
                    chunk.chunkData = Instantiate(chunk.chunkData);
                    chunk.RegenerateChunk();
                    
                    allChunks.Add(chunk);
                }
            }
        }
        
        var duration = Time.realtimeSinceStartupAsDouble - startTime;
        print($"Chunks build: <{allChunks.Count}> in duration: <{duration * 1000}ms> or <{duration}s>");
    }

    Vector3 GetGridPosition(Vector3 position)
    {
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);
        position.z = Mathf.Round(position.z);
        
        return position;
    }
}

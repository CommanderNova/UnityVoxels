using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

internal enum DebugGenerationTypes
{
    Full,
    Seconds,
    Thirds,
    Fourths,
    Random,
    Procedural,
}

public class DebugViewer : MonoBehaviour
{
    private Chunk notSelectedChunk;

    [SerializeField]
    private RawImage noiseViewer;

    [SerializeField]
    private GameObject noiseUIGroup;

    private GameObject selectedGameObject;
    
    private void Start()
    {
        notSelectedChunk = new Chunk();
        notSelectedChunk.chunkData = new ChunkData
        {
            width = -1,
            depth = -1,
            height = -1,
        };
    }

    private void OnGUI()
    {
        var activeObject = Selection.activeGameObject;
        var isSameObject = selectedGameObject == activeObject;
        selectedGameObject = activeObject;
        
        var activeChunk = activeObject ? activeObject.GetComponent<Chunk>() : null;
        if (isSameObject)
        {
            if (activeChunk)
            {
                OnChunkUpdate(activeChunk);
            }
            else
            {
                GUI.Label(new Rect(25, 25, 200, 30), "Select Chunk to debug");
            }
        }
        else
        {
            OnChunkChanged(activeChunk);
        }
    }

    private void OnChunkChanged(Chunk chunk)
    {
        noiseUIGroup.SetActive(chunk);
        if (!chunk)
        {
            return;
        }
        
        var chunkData = chunk.chunkData;
        chunk.debugNoise = new Texture2D(chunkData.width, chunkData.depth);
        var origin = chunk.Origin;

        for (var i = 0; i < chunkData.width; ++i)
        {
            for (var j = 0; j < chunkData.depth; ++j)
            {
                var x = i + origin.x;
                var z = j + origin.z;
                
                var value = chunk.GetNoise(chunk.Seed, x, z);
                chunk.debugNoise.SetPixel(x, z, new Color(1.0f * value, 1.0f * value, 1.0f * value));
            }
        }
        
        chunk.debugNoise.Apply();
        noiseViewer.texture = chunk.debugNoise;
    }
    
    private void OnChunkUpdate(Chunk chunk)
    {
        if (chunk.dirty)
        {
            return;
        }
        
        var posY = 25;
        var oldValue = 0;
        
        oldValue = chunk.chunkData.width;
        GUI.Label(new Rect(265, posY, 200, 30), $"width: {chunk.chunkData.width}");
        chunk.chunkData.width = (int)GUI.HorizontalSlider(new Rect(25, posY, 200, 25), chunk.chunkData.width, 0, Chunk.MaxChunkHorizontalSize);
        chunk.dirty |= chunk.chunkData.width != oldValue;
        
        oldValue = chunk.chunkData.depth;
        GUI.Label(new Rect(265, posY + 25, 200, 30), $"depth: {chunk.chunkData.depth}");
        chunk.chunkData.depth = (int)GUI.HorizontalSlider(new Rect(25, posY + 25, 200, 25), chunk.chunkData.depth, 0, Chunk.MaxChunkHorizontalSize);
        chunk.dirty |= chunk.chunkData.depth != oldValue;
        
        oldValue = chunk.chunkData.height;
        GUI.Label(new Rect(265, posY + 50, 200, 30), $"height: {chunk.chunkData.height}");
        chunk.chunkData.height = (int)GUI.HorizontalSlider(new Rect(25, posY + 50, 200, 25), chunk.chunkData.height, 0, 256);
        chunk.dirty |= chunk.chunkData.height != oldValue;

        if (GUI.Button(new Rect(350, posY, 100, 30), "Regenerate"))
        {
            chunk.Seed = Random.Range(0, int.MaxValue);
            chunk.dirty = true;
        }

        if (chunk.dirty)
        {
            OnChunkChanged(chunk);
        }

        var i = 0;
        foreach (DebugGenerationTypes genType in Enum.GetValues(typeof(DebugGenerationTypes)))
        {
            posY = 100 + (i * 35);
            
            if (GUI.Button(new Rect(25, posY, 100, 30), genType.ToString()))
            {
                if (genType == DebugGenerationTypes.Random)
                {
                    chunk.Seed = Random.Range(0, int.MaxValue);
                }
                
                chunk.generationType = genType;
                chunk.dirty = true;
            }
            
            i++;
        }
    }
}

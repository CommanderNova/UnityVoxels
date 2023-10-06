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
    [SerializeField]
    private RawImage noiseViewer;

    [HideInInspector]
    public Texture2D generalNoise;
    
    [SerializeField]
    private GameObject noiseUIGroup;

    private GameObject selectedGameObject;

    private void Start()
    {
        UpdateGeneralNoiseImage();
    }

    private void OnGUI()
    {
        var activeObject = Selection.activeGameObject;
        var isSameObject = selectedGameObject == activeObject;
        selectedGameObject = activeObject;
        
        var activeChunk = activeObject ? activeObject.GetComponent<Chunk>() : null;
        if (!isSameObject)
        {
            OnChunkChanged(activeChunk);
        }
        
        if (activeChunk)
        {
            OnChunkUpdate(activeChunk);
        }
        else
        {
            GUI.Label(new Rect(25, 25, 200, 30), "Select Chunk to debug");
            if (GUI.Button(new Rect(25, 25 + 30, 200, 150), "Regenerate All"))
            {
                var world = FindObjectOfType<World>();
                if (world)
                {
                    world.RegenerateWorld();
                }
            }
        }
    }

    private void OnChunkChanged(Chunk chunk)
    {
        if (chunk)
        {
            UpdateSpecificNoiseImage(chunk);
        }
        else
        {
            UpdateGeneralNoiseImage();
        }
    }

    private void UpdateSpecificNoiseImage(Chunk chunk, bool apply = true)
    {
        var chunkData = chunk.chunkData;
        chunk.debugNoise = new Texture2D(chunkData.width, chunkData.depth);
        var origin = chunk.Origin;

        for (var i = 0; i < chunkData.width; ++i)
        {
            for (var j = 0; j < chunkData.depth; ++j)
            {
                var x = i + origin.x;
                var z = j + origin.z;

                // TODO ML: this might not account for changes of the x and z input values from the GetBlock method
                var value = chunk.GetNoise(chunk.Seed, x, z);
                chunk.debugNoise.SetPixel(x, z, new Color(1.0f * value, 1.0f * value, 1.0f * value));
            }
        }

        if (apply)
        {
            chunk.debugNoise.Apply();
            noiseViewer.texture = chunk.debugNoise;
        }
    }

    private void UpdateGeneralNoiseImage()
    {
        var world = FindObjectOfType<World>();
        var allChunks = world.GetAllChunks();
        foreach (var chunk in allChunks)
        {
            UpdateSpecificNoiseImage(chunk, false);
        }

        var renderRadius = world.GetRenderRadius();
        var width = (Chunk.MaxChunkHorizontalSize * renderRadius * 2);
        var depth = width - Chunk.MaxChunkHorizontalSize;
        
        generalNoise = new Texture2D(width, depth);
        var posX = 0;
        var posZ = depth;
        foreach (var chunk in allChunks)
        {
            var debugNoise = chunk.debugNoise;
            for (int i = 0; i < Chunk.MaxChunkHorizontalSize; i++)
            {
                for (int j = 0; j < Chunk.MaxChunkHorizontalSize; j++)
                {
                    var pixel = debugNoise.GetPixel(i, j);
                    generalNoise.SetPixel(posX + i, posZ + j, pixel);
                }
            }
            
            posX += Chunk.MaxChunkHorizontalSize;
            if (posX >= width)
            {
                posX = 0;
                posZ -= Chunk.MaxChunkHorizontalSize;
            }
        }
        
        generalNoise.Apply();
        noiseViewer.texture = generalNoise;
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

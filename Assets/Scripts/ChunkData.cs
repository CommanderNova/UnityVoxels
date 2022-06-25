using UnityEngine;

[CreateAssetMenu(fileName = "ChunkData", menuName = "ChunkData", order = 1)]
public class ChunkData : ScriptableObject
{
	[Min(0)]
	public int width = 1;
	[Min(0)]
	public int depth = 1;
	[Min(0)]
	public int height = 1;
}
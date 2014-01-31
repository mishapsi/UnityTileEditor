using UnityEngine;
using System.Collections.Generic;

public class LayerData : ScriptableObject
{
	public string MapName = "NameOfMap";
	public string TilesetName = "";
	public int Depth = 0;
	public bool visible = true;
	public List<Tile> tiles;
}
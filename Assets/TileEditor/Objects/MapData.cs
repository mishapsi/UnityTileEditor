using UnityEngine;
using System.Collections.Generic;

public class MapData : ScriptableObject {
	public int xSize = 10;
	public int ySize = 10;
	
	public List<Tile> tiles;
	
	public void ResizeLayers()
	{
	}
}

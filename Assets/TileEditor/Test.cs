using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

//Test script for loading maps
//Attach to gameobject in your scene
//Included test tileset, tileset dimensions should be tilesize*10;

	public string mapName = "Map0";
	void Start () {
		MapLoader.LoadMap(mapName);
	}
	
}

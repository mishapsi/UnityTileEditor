using UnityEngine;
using System.Collections.Generic;

public class Tiler : MonoBehaviour {

	public LayerData tileData;
	public float[,] blocks = new float[10,10];
	public static Vector2 tileID = new Vector2(0,0.1f);
}

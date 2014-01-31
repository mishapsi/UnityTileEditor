using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

public class MapRenderer {

	public static float tUnitX = 0.1f;
	public static float tUnitY = 0.125f;
	
	public static Rect editorRect = new Rect(0,0,400,400);
	public static Rect viewRect;
	public static Vector2 scrollPos = new Vector2(0,0);
	
	public static List<Vector3> newVertices = new List<Vector3>();
	public static List<int> newTriangles = new List<int>();
	public static List<Vector2> newUV = new List<Vector2>();
	
	private static GameObject obj;
	private static Mesh mesh;
	private static MeshFilter meshFilter;
	private static Tiler tiler;
	private static int squareCount;
	
	public static List<Vector3> colVertices = new List<Vector3>();
	public static List<int> colTriangles = new List<int>();
	private static int colCount;
	
	private static MeshCollider col;
	
	public static void UpdateSceneLayer() {
		GameObject[] layers = GameObject.FindGameObjectsWithTag("Tiles");
		for (int i=0; i < layers.Length; i++) {
			tiler = layers[i].GetComponent<Tiler> ();
			meshFilter = layers[i].GetComponent<MeshFilter>();
			col = layers[i].GetComponent<MeshCollider> ();
			if(tiler != null)
			{
				BuildMesh ();
				UpdateMesh ();
			}
		}
	}

	public static void BuildMesh(){
		int ti = 0;
		MapData mapdata = (MapData)Resources.Load("Levels/" + EditorPrefs.GetString("CurrentMapName") + "/map_data");
		for (int ty=0; ty < mapdata.ySize; ty++)
		{
			for (int tx=0;tx < mapdata.xSize; tx++)
			{
				if(ti < tiler.tileData.tiles.Count())
				{
					float tid = tiler.tileData.tiles[ti].tileId;
					bool drawCollision = tiler.tileData.tiles[ti].collidable;
					if(drawCollision) GenCollider(tx,ty);
					if(tid != -1)
					{
						float x1 = Mathf.Round ((Mathf.Repeat (tid, 10))) / TileEditorWindows.xTile;
						float y1 = Mathf.Floor ((tid / 100) * 10.0f) / TileEditorWindows.yTile;
						GenSquare(tx,ty,new Vector2(x1,y1));
					}
				}
				ti++;
			}
		}
	}
	
	public static void GenSquare(int x, int y, Vector2 texture){
		
		float z = 0;
		
		newVertices.Add( new Vector3 (x  , y+1, z ));
		newVertices.Add( new Vector3 (x + 1 , y+1, z ));
		newVertices.Add( new Vector3 (x + 1 , y , z ));
		newVertices.Add( new Vector3 (x  , y, z ));
		
		newTriangles.Add(squareCount*4);
		newTriangles.Add((squareCount*4)+1);
		newTriangles.Add((squareCount*4)+3);
		newTriangles.Add((squareCount*4)+1);
		newTriangles.Add((squareCount*4)+2);
		newTriangles.Add((squareCount*4)+3);
		
		newUV.Add(new Vector2 (texture.x,texture.y + tUnitY));
		newUV.Add(new Vector2 (texture.x + tUnitX,texture.y + tUnitY));
		newUV.Add(new Vector2 (texture.x + tUnitX,texture.y));
		newUV.Add(new Vector2 (texture.x,texture.y));
		
		squareCount++;
	}
	
	public static void GenCollider(int x, int y){
		//Top
		if(Block(x,y+1)==false){
			colVertices.Add( new Vector3 (x  , y  +1, 1));
			colVertices.Add( new Vector3 (x + 1 , y  +1, 1));
			colVertices.Add( new Vector3 (x + 1 , y  +1, 0 ));
			colVertices.Add( new Vector3 (x  , y  +1, 0 ));
			
			ColliderTriangles();
			
			colCount++;
		}
		
		//bot
		if(Block(x,y-1)==false){
			colVertices.Add( new Vector3 (x  , y, 0));
			colVertices.Add( new Vector3 (x + 1 , y, 0));
			colVertices.Add( new Vector3 (x + 1 , y, 1 ));
			colVertices.Add( new Vector3 (x  , y, 1 ));
			
			ColliderTriangles();
			colCount++;
		}
		
		//left
		if(Block(x-1,y)==false){
			colVertices.Add( new Vector3 (x  , y , 1));
			colVertices.Add( new Vector3 (x  , y  +1, 1));
			colVertices.Add( new Vector3 (x  , y  +1, 0 ));
			colVertices.Add( new Vector3 (x  , y , 0 ));
			
			ColliderTriangles();
			
			colCount++;
		}
		
		//right
		if(Block(x+1,y)==false){
			colVertices.Add( new Vector3 (x +1 , y +1 , 1));
			colVertices.Add( new Vector3 (x +1 , y  , 1));
			colVertices.Add( new Vector3 (x +1 , y  , 0 ));
			colVertices.Add( new Vector3 (x +1 , y +1 , 0 ));
			
			ColliderTriangles();
			
			colCount++;
		}
	}
	
	public static void ColliderTriangles(){
		colTriangles.Add(colCount*4);
		colTriangles.Add((colCount*4)+1);
		colTriangles.Add((colCount*4)+3);
		colTriangles.Add((colCount*4)+1);
		colTriangles.Add((colCount*4)+2);
		colTriangles.Add((colCount*4)+3);
	}
	
	static bool Block (int x, int y){
		MapData mapdata = (MapData)Resources.Load("Levels/" + EditorPrefs.GetString("CurrentMapName") + "/map_data");
		if(x==-1 || x==mapdata.xSize ||   y<=-1 || y>=mapdata.ySize){
			return true;
		}
		
		return tiler.tileData.tiles[x+(y*mapdata.xSize)].collidable;
	}
	
	public static void UpdateMesh () {
		Mesh newMesh = new Mesh();
		newMesh.vertices = colVertices.ToArray();
		newMesh.triangles = colTriangles.ToArray();
		col.sharedMesh= newMesh;
		
		colVertices.Clear();
		colTriangles.Clear();
		colCount=0;
		
		Mesh m = new Mesh ();
		m.Clear ();
		m.vertices = newVertices.ToArray ();
		m.triangles = newTriangles.ToArray ();
		m.uv = newUV.ToArray ();
		m.Optimize ();
		m.RecalculateNormals ();
		meshFilter.sharedMesh = m;
		squareCount = 0;
		newVertices.Clear ();
		newTriangles.Clear ();
		newUV.Clear ();
	}
}

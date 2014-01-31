using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class TileEditorWindows : EditorWindow
{

	public static string NewLayerString = "";

	public static bool repaint = false;
	public static string layername = "";
	public static string mapName = "";
	public static int xSize = 10;
	public static int ySize = 10;
	
	public static Vector2 scrollPos = new Vector2(0,0);
	public static int tileSize = 16;
	public static int xTile = 0;
	public static int yTile = 0;
	
	public static Texture2D[] tilesetImages = new Texture2D[0];
	public static string[] tilesets;
	public static int tilesetIndex = 0;
	public static float zoomLevel = 1;
	
	public static List<Material> materials = new List<Material>();
	
	public static void EditorTools(int windowID)
	{
		GUIContent[] gui = new GUIContent[3];
		gui[0] = new GUIContent("Paint");
		gui[1] = new GUIContent("Eraser");
		gui[2] = new GUIContent("Collision");
		TileEditor.selectedTool = GUILayout.SelectionGrid(TileEditor.selectedTool,gui,3);
	}
	
	public static int FindLayers()
	{
		GameObject[] objs = Resources.FindObjectsOfTypeAll<GameObject>();
		GameObject[] trueObjs = new GameObject[objs.Length];
		for(int i=0; i < objs.Length; i++)
		{
			if(objs[i].tag == "Tiles" &&  objs[i].name != "TileLayer" || objs[i].tag == "disabled" &&  objs[i].name != "TileLayer")
			{
				trueObjs[i] = objs[i];
			}
		}
		return trueObjs.Length;
	}
	
	public static void MapProperties(int windowID) {
			GameObject currentLayer = TileEditor.GetLayer(TileEditor.layerIndex);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Rebuild Map",TileEditor.listSkin.button))
			{
				TileEditor.RepaintScene();
			}
			if (GUILayout.Button ("Create New Map",TileEditor.listSkin.button)) {
				TileEditor.createMap = true;
			TileEditor.mapIndex = EditorPrefs.GetInt("CurrentMapIndex");
				TileEditor.ReloadMaps();
			}
			GUILayout.EndHorizontal();
			if(TileEditor.maps.Length > 0)
			{
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Map:");
				TileEditor.mapIndex = EditorGUILayout.Popup (TileEditor.mapIndex, TileEditor.maps);
				EditorGUILayout.EndHorizontal ();
				if(EditorPrefs.GetInt("CurrentMapIndex") != TileEditor.mapIndex)
				{
					EditorPrefs.SetInt("CurrentMapIndex", TileEditor.mapIndex);
					EditorPrefs.SetString ("CurrentMapName", TileEditor.maps[TileEditor.mapIndex]);
					TileEditor.ReloadMaps();
					TileEditor.RepaintScene();
				}
				if(currentLayer != null)
				{
					TileEditor.lsPos = EditorGUILayout.BeginScrollView (TileEditor.lsPos);
					TileEditor.layerIndex = GUILayout.SelectionGrid(TileEditor.layerIndex,TileEditor.layers.ToArray(),1,TileEditor.listSkin.textField);
					EditorGUILayout.EndScrollView ();
					if(currentLayer != null)
					{
						LayerData data = currentLayer.GetComponent<Tiler>().tileData;
						data.visible = GUILayout.Toggle(data.visible,"visible");
						if(!data.visible && currentLayer.activeSelf)
						{
							currentLayer.tag = "disabled";
							currentLayer.SetActive(false);
						}
						else if(data.visible && !currentLayer.activeSelf)
						{
							currentLayer.tag = "Tiles";
							currentLayer.SetActive(true);
						}
					}
					GUILayout.BeginHorizontal();
					if(GUILayout.Button("Move Up",TileEditor.listSkin.button))
					{
						if(TileEditor.layerIndex-1 >= 0)
						{
							TileEditor.layers.Insert(TileEditor.layerIndex-1,TileEditor.layers[TileEditor.layerIndex]);
							TileEditor.layers.RemoveAt(TileEditor.layerIndex+1);
							TileEditor.layerIndex = TileEditor.layerIndex-1;
							TileEditor.ResortLayerDepths();
						}
					}
					if(GUILayout.Button("Move Down",TileEditor.listSkin.button))
					{
						if(TileEditor.layerIndex+2 <= TileEditor.layers.Count)
						{
							TileEditor.layers.Insert(TileEditor.layerIndex+2,TileEditor.layers[TileEditor.layerIndex]);
							TileEditor.layers.RemoveAt(TileEditor.layerIndex);
							TileEditor.layerIndex = TileEditor.layerIndex+1;
							TileEditor.ResortLayerDepths();
						}
					}
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					if(GUILayout.Button("Add",TileEditor.listSkin.button))
					{
						TileEditor.createLayer = true;
					}
					if(GUILayout.Button("Delete",TileEditor.listSkin.button))
					{
						if(TileEditor.layers.Count > 1)
						{
							TileEditor.DestroyLayerAt(TileEditor.layerIndex);
						}
					}
					GUILayout.EndHorizontal();
					if (EditorPrefs.GetInt ("CurrentMapIndex") != TileEditor.mapIndex) {
						EditorPrefs.SetInt ("CurrentMapIndex", TileEditor.mapIndex);
						EditorPrefs.SetString ("CurrentMapName", TileEditor.maps [TileEditor.mapIndex]);
						Debug.Log ("Loading Map: " + TileEditor.maps [TileEditor.mapIndex]);
						TileEditor.layerIndex = 0;
						EditorPrefs.SetInt ("CurrentLayerIndex", TileEditor.layerIndex);
						TileEditor.DestroyLayers();
						TileEditor.LoadMap();
					}
					TileEditor.TestSelection();
				}
			}
	}
	
	public static void TilesetCheck()
	{
		xTile = tilesetImages[tilesetIndex].width / tileSize;
		yTile = tilesetImages[tilesetIndex].height/ tileSize;
		MapRenderer.tUnitX = (float)1.0f/xTile;
		MapRenderer.tUnitY = (float)1.0f/yTile;
		if(tilesetImages.Length < Directory.GetFiles("Assets\\Resources\\Tilesets").Length)
		{
			LoadTilesets();
		}
		if(Directory.GetFiles("Assets\\Resources\\Tilesets\\Materials","*.mat").Length != tilesetImages.Length || materials.Count == 0)
		{
			MakeMaterials();
		}
		GameObject layerObj = Selection.activeGameObject;
		if(layerObj != null)
		{
			Tiler tiler = layerObj.GetComponent<Tiler>();
			if(tiler!= null)
			{
				for(int i=0;i<tilesets.Length;i++)
				{
					if(tilesets[i] == tiler.tileData.TilesetName)
					{
						tilesetIndex = i;
					}
				}
			}
		}
	}
	
	public static void MakeMaterials()
	{
		List<string> check = new List<string>(tilesets);
		string[] mats = Directory.GetFiles("Assets\\Resources\\Tilesets\\Materials","*.mat");
		for (int ii=0; ii < mats.Length; ii++) {
			mats[ii] = mats[ii].Replace("Assets\\Resources\\Tilesets\\Materials\\" ,"");
			mats[ii] = mats[ii].Replace(".mat","");
		}
		List<string> matlist = new List<string>(mats);
		for(int i=0; i< tilesets.Length;i++)
		{
			if(!matlist.Contains(tilesetImages[i].name))
			{
				Material mat = new Material(Shader.Find("Unlit/Transparent"));
				mat.mainTexture = tilesetImages[i];
				AssetDatabase.CreateAsset(mat, "Assets/Resources/Tilesets/Materials/" + tilesetImages[i].name + ".mat");
			}
		}
		for(int ii=0; ii < matlist.Count; ii++)
		{
			if(!check.Contains(matlist[ii]))
			{
				AssetDatabase.DeleteAsset("Assets/Resources/Tilesets/Materials/" + matlist[ii] + ".mat");
			}
		}
		AssetDatabase.SaveAssets();
		Material[] matArray = (Material[])Resources.LoadAll<Material>("Tilesets/Materials/");
		materials = new List<Material>(matArray);
	}
	
	public static void LoadTilesets()
	{
		tilesetImages = (Texture2D[])Resources.LoadAll<Texture2D>("Tilesets/");
		tilesets = new string[tilesetImages.Length];
		for(int i=0; i < tilesetImages.Length; i++)
		{
			tilesets[i] = tilesetImages[i].name;
		}
	}
	
	public static void TilePalette(int windowID){
		TilesetCheck();
		GameObject layerObj = TileEditor.GetLayer(TileEditor.layerIndex);
		if(layerObj != null)
		{
			EditorPrefs.SetString("CurrentLayerName",layerObj.name);
			EditorPrefs.SetInt("CurrentLayerIndex",TileEditor.layers.IndexOf(layerObj.name));
			MeshRenderer meshRen = layerObj.GetComponent<MeshRenderer>();
			Tiler tiler = layerObj.GetComponent<Tiler>();
			tilesetIndex = EditorGUILayout.Popup("Tileset", tilesetIndex,tilesets);
			tileSize = EditorGUILayout.IntField("Tile Size", tileSize);
			if(meshRen.sharedMaterial == null || meshRen.sharedMaterial.name != tilesets[tilesetIndex])
			{
				for(int i = 0; i < materials.Count; i++)
				{
					if(materials[i].name == tilesets[tilesetIndex])
					{
						meshRen.sharedMaterial = materials[i];
						tiler.tileData.TilesetName = tilesets[tilesetIndex];
					}
				}
			}
		}
		Rect paletteRect = new Rect(0,60,198,TileEditor.sceneV.position.height-102);
		Rect viewRect = new Rect(0,0,tilesetImages[tilesetIndex].width*zoomLevel, tilesetImages[tilesetIndex].height*zoomLevel);
		scrollPos = GUI.BeginScrollView(paletteRect,scrollPos,viewRect);
		GUIStyle style = new GUIStyle();
		style.normal.background = tilesetImages[tilesetIndex];
		GUI.Label(new Rect (0, 0, tilesetImages[tilesetIndex].width*zoomLevel, tilesetImages[tilesetIndex].height*zoomLevel),"",style);
		Vector2 tilePos;
		if (Event.current.type == EventType.mouseDown) {
			tilePos.x = (Mathf.Floor ((Event.current.mousePosition.x) / (tileSize*zoomLevel)) / xTile);
			tilePos.y = (.9f - (Mathf.Floor ((Event.current.mousePosition.y) / (tileSize*zoomLevel)) / yTile)) ;
			Tiler.tileID = tilePos;
			Debug.Log(tilePos);
		}
		GUI.EndScrollView();
		zoomLevel = GUI.HorizontalSlider(new Rect(100,TileEditor.sceneV.position.height-42,96,20),zoomLevel,1,3);
	}
	
	public static void NewMapWindow(int windowID)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Name");
		mapName = GUILayout.TextField (mapName);
		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal();
		xSize = EditorGUILayout.IntField("Size",xSize);
		GUILayout.Label("x");
		ySize = EditorGUILayout.IntField(ySize);
		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal();
		if (GUILayout.Button ("OK")) {
			if(!Directory.Exists("Assets/Resources/Levels/" + mapName))
			{
				AssetDatabase.CreateFolder("Assets/Resources/Levels", mapName);
				MapData mapData = ScriptableObject.CreateInstance<MapData>();
				mapData.xSize = xSize;
				mapData.ySize = ySize;
				LayerData layer = ScriptableObject.CreateInstance<LayerData>();
				layer.tiles = new List<Tile>();
				layer.MapName = mapName;
				
				int numberOfElements = mapData.xSize * mapData.ySize;
				
				for (int i = 0; i < numberOfElements; i++)
				{
					layer.tiles.Add(new Tile());
				}
				AssetDatabase.CreateAsset(mapData,"Assets/Resources/Levels/" + mapName + "/map_data.asset");
				AssetDatabase.CreateAsset(layer,"Assets/Resources/Levels/" + mapName + "/Layer0.asset");
				AssetDatabase.SaveAssets();
				
				string[] maps = Directory.GetDirectories ("Assets/Resources/Levels/");
				for(int i=0; i<maps.Length; i++)
				{
					if(maps[i]=="Assets/Resources/Levels/" + mapName)
					{
						EditorPrefs.SetInt("CurrentMapIndex",i);
						EditorPrefs.SetString("CurrentMapName", mapName);
					}
				}
				TileEditor.createMap = false;
			}
			else
			{
				Debug.Log("Map Already Exists!");
				TileEditor.createMap = false;
			}
		}
		if(GUILayout.Button("Cancel"))
		{
			TileEditor.createMap = false;
		}
		GUILayout.EndHorizontal();
	}

	public static void NewLayerWindow(int windowID)
	{
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Name");
		NewLayerString = GUILayout.TextField (NewLayerString);
		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("OK")) {
			if(!TileEditor.layers.Contains(NewLayerString))
			{
				MapData mapdata = (MapData)Resources.Load("Levels/" + EditorPrefs.GetString("CurrentMapName") + "/map_data");
				LayerData layer = ScriptableObject.CreateInstance<LayerData>();
				layer.tiles = new List<Tile>();
				int numberOfElements = mapdata.xSize * mapdata.ySize;
	
				for (int i = 0; i < numberOfElements; i++)
				{
					layer.tiles.Add(new Tile());
				}
				AssetDatabase.CreateAsset(layer,"Assets/Resources/Levels/" + EditorPrefs.GetString("CurrentMapName") + "/" + NewLayerString + ".asset");
				AssetDatabase.SaveAssets();
				TileEditor.layerIndex = 0;
				EditorPrefs.SetInt("CurrentLayerIndex",0);
				EditorPrefs.SetString("CurrentLayerName",NewLayerString);
				TileEditor.layers.Insert(0,NewLayerString);
				Selection.activeGameObject = TileEditor.InstantiateLayer(NewLayerString);
				TileEditor.ResortLayerDepths();
				TileEditor.createLayer = false;
				NewLayerString = "";
			}
			else
			{
				Debug.Log("Layer Name Exists!!!");
			}
		}
		if(GUILayout.Button("Cancel"))
		{
			TileEditor.createLayer = false;
		}
		GUILayout.EndHorizontal();
	}
}
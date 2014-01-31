using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[InitializeOnLoad]
public class TileEditor:EditorWindow {
	
	public static GameObject parent;
	public static string[] maps;
	public static int mapIndex = 0;
	public static List<string> layers;
	public static int layerIndex = 0;
	public static int depth = 0;
	public static Vector2 lsPos;
	public static int layersCount = 0;
	
	public static int selectedTool = 0;
	
	public static SceneView sceneV;
	
	public static GUISkin listSkin;
	public static GUISkin editorSkin;
	
	public static bool lastToggle = false;
	public static bool enabled = false;
	public static bool createLayer = false;
	public static bool createMap = false;
	
	static TileEditor()
	{
		listSkin = Resources.Load<GUISkin>("ListSkin");
		editorSkin = Resources.Load<GUISkin>("EditorSkin");
		InstantiateLayersParent();
		SanityCheck();
		ReloadMaps();
		DestroyLayers();
		LoadMap ();
		EditorPrefs.SetString("CurrentMapName",maps[0]);
		EditorPrefs.SetInt("CurrentMapIndex",0);
		EditorPrefs.SetString ("CurrentLayerName",layers[0]);
		EditorPrefs.SetInt("CurrentLayerIndex",0);
		SceneView.onSceneGUIDelegate += OnScene;
	}
	
	static void InstantiateLayersParent()
	{
		GameObject[] parents = GameObject.FindGameObjectsWithTag("LayersParent");
		for(int i=0; i < parents.Length; i++)
		{
			if(parents[i].name == "Layers(Don't Delete!!!)")
			{
				DestroyImmediate(parents[i]);
			}
		}
		Object obj = Resources.Load("LayersPrefab");
		parent = PrefabUtility.InstantiatePrefab(obj) as GameObject;
		parent.name = "Layers(Don't Delete!!!)";
	}
	
	static bool SanityCheck(){
		bool disable = false;
		if(parent == null)
		{
			InstantiateLayersParent();
			RepaintScene();
		}
		if(!Directory.GetDirectories("Assets").Contains("Assets\\Resources"))
		{
			AssetDatabase.CreateFolder("Assets","Resources");
		}
		if(!Directory.GetDirectories("Assets\\Resources").Contains("Assets\\Resources\\Tilesets"))
		{
			AssetDatabase.CreateFolder("Assets/Resources","Tilesets");
		}
		if(!Directory.GetDirectories("Assets\\Resources\\Tilesets").Contains("Assets\\Resources\\Tilesets\\Materials"))
		{
			AssetDatabase.CreateFolder("Assets/Resources/Tilesets","Materials");
		}
		if(!Directory.GetDirectories("Assets\\Resources").Contains("Assets\\Resources\\Levels"))
		{
			AssetDatabase.CreateFolder("Assets/Resources","Levels");
		}
		if(Directory.GetDirectories("Assets\\Resources\\Levels").Length == 0)
		{
			AssetDatabase.CreateFolder("Assets/Resources/Levels","Map0");
			MapData mapData = ScriptableObject.CreateInstance<MapData>();
			LayerData layer = ScriptableObject.CreateInstance<LayerData>();
			layer.tiles = new List<Tile>();
			layer.MapName = "Map0";
			
			int numberOfElements = mapData.xSize * mapData.ySize;
			
			for (int i = 0; i < numberOfElements; i++)
			{
				layer.tiles.Add(new Tile());
			}
			AssetDatabase.CreateAsset(mapData,"Assets/Resources/Levels/Map0/map_data.asset");
			AssetDatabase.CreateAsset(layer,"Assets/Resources/Levels/Map0/Layer0.asset");
			AssetDatabase.SaveAssets();
			EditorPrefs.SetString("CurrentMapName","Map0");
			EditorPrefs.SetInt("CurrentMapIndex",0);
			EditorPrefs.SetString ("CurrentLayerName","Layer0");
			EditorPrefs.SetInt("CurrentLayerIndex",0);
		}
		if(Directory.GetFiles("Assets\\Resources\\Tilesets","*.png").Length == 0)
		{
			Debug.Log ("Tilesets needed in Assets/Resources/Tilesets in order to use Tile Editor");
			disable = true;
		}
		return disable;
	}
	
	static void OnScene(SceneView sceneView) {
		GUI.skin = editorSkin;
		Handles.BeginGUI();
		enabled = GUI.Toggle(new Rect(0,0,100,25),enabled,"Enable Editor");
		if(createLayer && !createMap)
		{
			GUI.Window(3,new Rect(sceneV.position.width/2-100,sceneV.position.height/2, 200,60),TileEditorWindows.NewLayerWindow,"New Layer");
		}
		else if(createMap)
		{
			createLayer = false;
			GUI.Window(4,new Rect(sceneView.position.width/2-150,sceneView.position.height/2, 250,80),TileEditorWindows.NewMapWindow,"New Map");
		}
		Handles.EndGUI();
		if(enabled && SanityCheck() == false)
		{
			lastToggle = true;
			MapData mapdata = (MapData)Resources.Load("Levels/" + EditorPrefs.GetString("CurrentMapName") + "/map_data");
			DefaultHandles.Hidden = true;
			Vector3[]boundsBox = new Vector3[5];
			boundsBox[0] = new Vector3(0,0,0);
			boundsBox[1] = new Vector3(mapdata.xSize,0,0);
			boundsBox[2] = new Vector3(mapdata.xSize,mapdata.ySize,0);
			boundsBox[3] = new Vector3(0,mapdata.ySize,0);
			boundsBox[4] = new Vector3(0,0,0);
			Handles.DrawPolyLine(boundsBox);
			sceneV = sceneView;
			Handles.BeginGUI();
			GUI.Window(2,new Rect(0,40, 200, sceneView.position.height-40),TileEditorWindows.MapProperties,"Map Properties");
			GUI.Window (1,new Rect(sceneView.position.width - 200,20,200,sceneView.position.height),TileEditorWindows.TilePalette,"Tile Palette");
			GUI.Window (0, new Rect(200,sceneView.position.height-50,sceneView.position.width-400,50),TileEditorWindows.EditorTools,"Tools");
			Handles.EndGUI();
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			if(GameObject.Find(EditorPrefs.GetString("CurrentLayerName")) != Selection.activeGameObject) Selection.activeGameObject = GameObject.Find(EditorPrefs.GetString("CurrentLayerName"));
			if(Selection.activeGameObject != null) EditorUtility.SetSelectedWireframeHidden(Selection.activeGameObject.renderer,true);
			if(Event.current.type == EventType.mouseDrag && !createLayer && !createMap)
			{
				Vector2 mousePos = Event.current.mousePosition;
				mousePos.y = Camera.current.pixelHeight - mousePos.y;
				Vector3 position = Camera.current.ScreenPointToRay(mousePos).origin;
				int x = Mathf.FloorToInt (position.x);
				int y = Mathf.FloorToInt (position.y);
				y = y * mapdata.xSize;
				GameObject obj = GameObject.Find(EditorPrefs.GetString("CurrentLayerName"));
				if(obj == null)
				{
					obj = GameObject.Find(layers[0]);
				}
				Tiler tiler = obj.GetComponent<Tiler> ();
				if(x + y >= 0 && x + y < tiler.tileData.tiles.Count)
				{
					if(selectedTool == 0 && Event.current.button == 0)
					{
						Event.current.Use();
						float id = (Tiler.tileID.x * 10) + (Tiler.tileID.y * 100);
						tiler.tileData.tiles[x+y].tileId = (int)Mathf.Round(id);
						EditorUtility.SetDirty (tiler.tileData);
					}
					else if (selectedTool == 1 && Event.current.button == 0)
					{
						Event.current.Use();
						float id = -1;
						tiler.tileData.tiles[x+y].tileId = (int)Mathf.Round(id);
						EditorUtility.SetDirty (tiler.tileData);
					}
					else if (selectedTool == 2)
					{
						if(Event.current.button == 0)
						{
							Event.current.Use();
							tiler.tileData.tiles[x+y].collidable = true;
						}
						else if(Event.current.button == 1)
						{
							Event.current.Use();
							tiler.tileData.tiles[x+y].collidable = false;
						}
						EditorUtility.SetDirty (tiler.tileData);
					}
					SceneView.RepaintAll ();
					MapRenderer.UpdateSceneLayer();
				}
			}
		}
		else
		{
			enabled = false;
			if(lastToggle == true)
			{
				DefaultHandles.Hidden = false;
				HandleUtility.Repaint();
			}
			lastToggle = false;
		}
	}
	
	public static void ReloadMaps() {
		maps = Directory.GetDirectories ("Assets/Resources/Levels/");
		if(maps.Count() > 0)
		{
			for (int i=0; i < maps.Length; i++) {
				maps[i] = maps[i].Replace("Assets/Resources/Levels/","");
			}
			string[] larr = Directory.GetFiles ("Assets/Resources/Levels/" + maps[mapIndex],"*.asset");
			for (int ii=0; ii < larr.Length; ii++) {
				larr[ii] = larr[ii].Replace("Assets/Resources/Levels/" + maps[mapIndex] + "\\" ,"");
				larr[ii] = larr[ii].Replace(".asset","");
			}
			layers = new List<string> (larr);
			layers.Remove("map_data");
			layersCount = layers.Count();
			OrganizeLayers();
		}
	}
	
	public static void OrganizeLayers()
	{
		LayerData[] lds = (LayerData[])Resources.LoadAll<LayerData>("Levels/" + EditorPrefs.GetString("CurrentMapName") + "/");
		if(lds.Length > 0)
		{
			List<LayerData> lds1 = new List<LayerData>(lds);
			List<LayerData> sortedlist =  lds1.OrderBy(go=>go.Depth).ToList();
			for (int ii=0; ii < sortedlist.Count; ii++)
			{
				layers[ii] = sortedlist[ii].name;
			}
		}
	}
	
	public static void RepaintScene(){
		ReloadMaps();
		DestroyLayers();
		LoadMap ();
	}
	
	public static void DestroyLayers()
	{
		Transform[] layersTrans = (Transform[])parent.GetComponentsInChildren<Transform>(true);
		List<GameObject> layersObj = new List<GameObject>();
		for(int i=0;i < layersTrans.Length; i++)
		{
			layersObj.Add(layersTrans[i].gameObject);
		}
		for (int i=0; i < layersObj.Count; i++) {
			if(layersObj[i].name != "Layers(Don't Delete!!!)")
			{
				DestroyImmediate (layersObj[i]);
			}
		}
	}
	
	public static void LoadMap()
	{
		TileEditorWindows.LoadTilesets();
		TileEditorWindows.MakeMaterials();
		for(int ii=0;ii < layers.Count; ii++)
		{
			InstantiateLayer(layers[ii]);
		}
		MapRenderer.UpdateSceneLayer ();
	}
	
	public static GameObject InstantiateLayer(string name)
	{
		object o = Resources.Load("Levels/" + maps[mapIndex] + "/" + name); 
		Object obj = Resources.Load("TileLayer");
		GameObject lay = PrefabUtility.InstantiatePrefab(obj) as GameObject;
		lay.name = name;
		Tiler data = lay.GetComponent<Tiler>();
		data.tileData = o as LayerData;
		MeshRenderer meshRen = lay.GetComponent<MeshRenderer>();
		if(meshRen.sharedMaterial == null)
		{
			for(int i = 0; i < TileEditorWindows.materials.Count; i++)
			{
				if(TileEditorWindows.materials[i].name == data.tileData.TilesetName)
				{
					meshRen.sharedMaterial = TileEditorWindows.materials[i];
				}
			}
		}
		Vector3 pos = lay.transform.position;
		pos.z = data.tileData.Depth;
		lay.transform.position = pos;
		lay.transform.parent = parent.transform;
		return lay;
	}
	
	public static GameObject GetLayer(int index)
	{
		GameObject layer = GameObject.Find(layers[index]);
		if(layer == null)
		{
			GameObject[] objs = (GameObject[])Resources.FindObjectsOfTypeAll<GameObject>();
			for(int i=0; i<objs.Length;i++)
			{
				if(objs[i].name == layers[index])
				{
					layer = objs[i];
				}
			}
		}
		return layer;
	}
	
	public static void ResortLayerDepths()
	{
		for(int ii = 0; ii < layers.Count; ii++)
		{
			GameObject layer = GetLayer (ii);
			Tiler data = layer.GetComponent<Tiler>();
			data.tileData.Depth = ii;
			EditorUtility.SetDirty(data.tileData);
			Vector3 pos = layer.transform.position;
			pos.z = data.tileData.Depth;
			layer.transform.position = pos;
		}
	}
	
	public static void DestroyLayerAt(int index)
	{
		GameObject layer = GetLayer(index);
		AssetDatabase.DeleteAsset("Assets/Resources/Levels/" + maps[mapIndex] + "/" + layer.name +".asset");
		DestroyImmediate(layer);
		layers.RemoveAt(index);
		ResortLayerDepths();
		if(layerIndex >= layers.Count)
		{
			if(layerIndex-1 >= 0)
			{
				layerIndex -=1;
			}
		}
		Selection.activeGameObject = TileEditor.GetLayer(TileEditor.layerIndex);
	}
	
	public static void TestSelection()
	{
		if(Selection.activeGameObject != null && Selection.activeGameObject.name != layers[layerIndex])
		{
			Selection.activeGameObject = GetLayer(layerIndex);
		}
	}
}

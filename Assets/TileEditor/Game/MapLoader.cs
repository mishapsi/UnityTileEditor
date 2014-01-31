using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MapLoader:MonoBehaviour {

	public static string currentMap = "";
	public static List<string> layers;
	public static GameObject layersParent;
	
	public static void LoadMap(string mapName)
	{
		currentMap = mapName;
		layers = new List<string>(Directory.GetFiles("Assets\\Resources\\Levels\\" + mapName + "\\","*.asset"));
		for (int ii=0; ii < layers.Count; ii++) {
			layers[ii] = layers[ii].Replace("Assets\\Resources\\Levels\\" + currentMap + "\\" ,"");
			layers[ii] = layers[ii].Replace(".asset","");
			layers.Remove("map_data");
		}
		InstantiateLayersParent();
		ResourceLoader.LoadMaterials();
		for(int ii=0;ii < layers.Count; ii++)
		{
			InstantiateLayer(layers[ii]);
		}
		LevelRenderer.UpdateSceneLayer ();
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
		UnityEngine.Object obj = Resources.Load("LayersPrefab");
		layersParent = Instantiate(obj) as GameObject;
		layersParent.name = "Layers(Don't Delete!!!)";
		DestroyLayers();
	}
	
	public static void DestroyLayers()
	{
		Transform[] layersTrans = (Transform[])layersParent.GetComponentsInChildren<Transform>(true);
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
	
	public static GameObject InstantiateLayer(string name)
	{
		Object o = Resources.Load("Levels/" + currentMap + "/" + name); 
		Object obj = Resources.Load("TileLayer");
		GameObject lay = GameObject.Instantiate(obj) as GameObject;
		lay.name = name;
		Tiler data = lay.GetComponent<Tiler>();
		data.tileData = o as LayerData;
		MeshRenderer meshRen = lay.GetComponent<MeshRenderer>();
		if(meshRen.sharedMaterial == null)
		{
			for(int i = 0; i < ResourceLoader.materials.Count; i++)
			{
				if(ResourceLoader.materials[i].name == data.tileData.TilesetName)
				{
					meshRen.sharedMaterial = ResourceLoader.materials[i];
				}
			}
		}
		Vector3 pos = lay.transform.position;
		pos.z = data.tileData.Depth;
		lay.transform.position = pos;
		lay.transform.parent = layersParent.transform;
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
			Vector3 pos = layer.transform.position;
			pos.z = data.tileData.Depth;
			layer.transform.position = pos;
		}
	}
	
	public static void DestroyLayerAt(int index)
	{
		GameObject layer = GetLayer(index);
		Destroy(layer);
		layers.RemoveAt(index);
		ResortLayerDepths();
	}
}

using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ResourceLoader {

	public static List<Material> materials;
	public static void LoadMaterials()
	{
		Material[] matArray = (Material[])Resources.LoadAll<Material>("Tilesets/Materials/");
		materials = new List<Material>(matArray);
	}
}

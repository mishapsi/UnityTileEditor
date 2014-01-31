using UnityEngine;

using UnityEditor;

using System.Collections;



public class DummyEditor : EditorWindow
	
{
	
	[MenuItem("Assets/Save Editor Skin")]
	
	static public void SaveEditorSkin()
		
	{
		
		GUISkin skin = ScriptableObject.Instantiate(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector)) as GUISkin;
		
		AssetDatabase.CreateAsset(skin, "Assets/EditorSkin.guiskin");
		
	}
	
}
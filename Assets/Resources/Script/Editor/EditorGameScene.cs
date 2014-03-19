using UnityEngine;
using UnityEditor;
using UIEditor.Node;
[CustomEditor(typeof(GameScene))]
public class EditorGameScene : Editor {
	static bool showDebug = false;
	public override void OnInspectorGUI() {
		showDebug = EditorGUILayout.Toggle("Debug",showDebug);
		//if(showDebug)
		DrawDefaultInspector();
		
		GameScene element = (GameScene)target;
		
		//view.StartView = (View)EditorGUILayout.ObjectField("Start View",view.StartView,typeof(View),true);
	
		EditorGUILayout.Separator();
		if(GUILayout.Button("Calculation zOrder")){
			element.SortZorder();
		}
		if(GUILayout.Button("Delete Player Setting")){
			PlayerPrefs.DeleteAll();
		}
	}
}

using UnityEngine;
using UnityEditor;
using UIEditor.Node;
[CustomEditor(typeof(AutoMoveObject))]

public class EditorAutoMoveObject : Editor {
	string key = "";
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		AutoMoveObject elem = (AutoMoveObject)target;
		MapStringAnimationClip map = elem.AnimationEventClips;

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.BeginVertical("Box");
		EditorGUILayout.LabelField("Animation Index Event");
		for(int i = 0; i < map.Count ; ++i){
			EditorGUILayout.BeginHorizontal();
			map[map.GetKey(i)] = (AnimationClip)EditorGUILayout.ObjectField(map.GetKey(i),map[map.GetKey(i)],typeof(AnimationClip),true);
			if(GUILayout.Button("Del",EditorStyles.miniButton,GUILayout.Width(58))){
				map.Remove(map.GetKey(i));
			}
			EditorGUILayout.EndHorizontal();
		}

		key = EditorGUILayout.TextField("Index",key);
		if(GUILayout.Button("Add",EditorStyles.miniButton,GUILayout.Width(58))){
			map.Add(key,null);
		}
		EditorGUILayout.EndVertical();
		if(EditorGUI.EndChangeCheck()){
			elem.AnimationEventClips = map;
		}


	}

}

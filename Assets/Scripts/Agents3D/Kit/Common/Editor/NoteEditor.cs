using UnityEngine;
using UnityEditor;

namespace Kit
{
	[CustomEditor(typeof(Note))]
	public class NoteEditor : Editor
	{
		SerializedProperty noteProp, typeProp;
		void OnEnable()
		{
			noteProp = serializedObject.FindProperty("note");
			typeProp = serializedObject.FindProperty("type");
		}
		public override void OnInspectorGUI()
		{
			if (string.IsNullOrEmpty(noteProp.stringValue))
				EditorGUILayout.HelpBox("Modify : Ctrl + Shift + A", MessageType.Info);
			else
				EditorGUILayout.HelpBox(noteProp.stringValue, (MessageType)typeProp.enumValueIndex);

			if (!((Component)target).gameObject.activeSelf)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(typeProp);
				noteProp.stringValue = EditorGUILayout.TextArea(noteProp.stringValue, GUILayout.MinHeight(100f), GUILayout.ExpandHeight(true));
				if (EditorGUI.EndChangeCheck())
				{
					serializedObject.ApplyModifiedProperties();
					serializedObject.Update();
				}
			}
		}
	}
}
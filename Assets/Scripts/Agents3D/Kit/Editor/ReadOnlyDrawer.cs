using UnityEngine;
using UnityEditor;

namespace Kit
{
	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			EditorGUI.BeginDisabledGroup(true);
			EditorGUI.PropertyField(position, property, label, true);
			EditorGUI.EndDisabledGroup();
			EditorGUI.EndProperty();
		}
	}
}
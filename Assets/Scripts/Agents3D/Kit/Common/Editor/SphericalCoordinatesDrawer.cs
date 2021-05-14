#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Kit
{
	[CustomPropertyDrawer(typeof(SphericalCoordinates))]
	public class SphericalCoordinatesDrawer : PropertyDrawer
	{
		readonly GUIContent RadiusLabel = new GUIContent("R","Radius");
		readonly GUIContent PolarLabel = new GUIContent("P","Polar");
		readonly GUIContent ElevationLabel = new GUIContent("E","Elevation");
		readonly GUIContent ResetLabel = new GUIContent("Reset", "Reset to zero");
		readonly GUIContent ResetShortLabel = new GUIContent("!", "Reset to zero");
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			Rect
				prefix = EditorGUI.PrefixLabel(position, label),
				field00 = position.Clone(x: prefix.x, width: prefix.width / 4f),
				field01 = field00.GetRectRight(),
				field02 = field01.GetRectRight(),
				field03 = field02.GetRectRight();
			bool compactMode = prefix.width <= 160f;
			int _indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			EditorGUIUtility.labelWidth = 14f;
			EditorGUI.BeginChangeCheck();
			float
				polar = property.FindPropertyRelative("_polar").floatValue * Mathf.Rad2Deg,
				elevation = property.FindPropertyRelative("_elevation").floatValue * Mathf.Rad2Deg,
				radius = property.FindPropertyRelative("_radius").floatValue,
				newPolar = EditorGUI.FloatField(field00, PolarLabel, polar),
				newElevation = EditorGUI.FloatField(field01, ElevationLabel, elevation),
				newRadius = EditorGUI.FloatField(field02, RadiusLabel, radius);

			if (EditorGUI.EndChangeCheck())
			{
				property.FindPropertyRelative("_polar").floatValue = Mathf.Repeat(newPolar, 360f) * Mathf.Deg2Rad;
				property.FindPropertyRelative("_elevation").floatValue = Mathf.Repeat(newElevation, 360f) * Mathf.Deg2Rad;
				property.FindPropertyRelative("_radius").floatValue = newRadius;
			}

			if (GUI.Button(field03, (compactMode) ? ResetShortLabel : ResetLabel))
			{
				property.FindPropertyRelative("_polar").floatValue = 0f;
				property.FindPropertyRelative("_elevation").floatValue = 0f;
				property.FindPropertyRelative("_radius").floatValue = 0f;
			}
			EditorGUI.indentLevel = _indent;
			EditorGUI.EndProperty();
		}
	}
}
#endif



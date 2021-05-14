using UnityEngine;
using UnityEditor;

namespace Kit
{
	[CustomPropertyDrawer(typeof(RectRangeAttribute))]
	public class RectRangeDrawer : PropertyDrawer
	{
		RectRangeAttribute rangeAttribute { get { return ((RectRangeAttribute)attribute); } }

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			if (property.propertyType == SerializedPropertyType.AnimationCurve)
			{
				EditorGUI.CurveField(EditorGUI.PrefixLabel(position, label), property, Color.green, rangeAttribute.range, GUIContent.none);
			}
			else
				EditorGUI.LabelField(position, label, "RectRange allow to with { AnimationCurve }.");
			// TODO: Vector3 + AnimationCurve
			// EditorGUIUtility.DrawCurveSwatch(position, AnimationCurve.Linear(0f, 1f, 1f, 0f), property, Color.yellow, Color.clear, rangeAttribute.range);
			// EditorGUIUtility.DrawCurveSwatch(position, property.animationCurveValue, property, Color.green, Color.grey, rangeAttribute.range);
			// EditorGUIUtility.DrawCurveSwatch(position, AnimationCurve.Linear(0f, .5f, .5f, 0f), property, Color.red, Color.clear, rangeAttribute.range);

			EditorGUI.EndProperty();
		}
	}
}
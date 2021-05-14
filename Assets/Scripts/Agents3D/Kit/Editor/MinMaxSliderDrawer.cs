using UnityEngine;
using UnityEditor;
using Kit;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
class MinMaxSliderDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (property.propertyType == SerializedPropertyType.Vector2)
		{
			Vector2 range = property.vector2Value;
			float min = range.x;
			float max = range.y;
			MinMaxSliderAttribute attr = (MinMaxSliderAttribute)attribute;
			EditorGUI.BeginChangeCheck();

			Rect context = EditorGUI.PrefixLabel(position, label);
			Rect[] cols = context.SplitLeft(50f);
			min = EditorGUI.DelayedFloatField(cols[0], min);
			cols = cols[1].SplitRight(50f);
			EditorGUI.MinMaxSlider(cols[0], ref min, ref max, attr.min, attr.max);
			max = EditorGUI.DelayedFloatField(cols[1], max);
			
			if (EditorGUI.EndChangeCheck())
			{
				range.x = min;
				range.y = max;
				property.vector2Value = range;
			}
		}
		else
		{
			EditorGUI.LabelField(position, label, "Use only with Vector2");
		}
	}
}
using UnityEngine;
using UnityEditor;

namespace Kit
{
	[CustomPropertyDrawer(typeof(TagFieldAttribute))]
	public class TagFieldDrawer : PropertyDrawer
	{
		TagFieldAttribute tagFieldAttribute { get { return (TagFieldAttribute)attribute; } }

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			if (property.propertyType == SerializedPropertyType.String)
			{
				EditorExtend.TagField(position, property, label);
			}
			else
			{
				EditorGUI.LabelField(position, label, typeof(TagFieldAttribute).Name + " only allow to use with { String }.");
			}
			EditorGUI.EndProperty();
		}
	}
}
using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Kit
{
	[CustomPropertyDrawer(typeof(ContextButtonAttribute))]
	public sealed class ContextButtonDrawer : PropertyDrawer
	{
		ContextButtonAttribute buttonAttribute { get { return (ContextButtonAttribute)attribute; } }

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			if (property.propertyType == SerializedPropertyType.Boolean)
			{
				if (GUI.Button(position, label))
				{
					// Component comp = (Component)property.serializedObject.targetObject;
					// comp.SendMessage(buttonAttribute.Callback); // not work for disable gameobject

					Type type = property.serializedObject.targetObject.GetType();
					MethodInfo methodInfo = type.GetMethod(buttonAttribute.Callback);
					if (methodInfo != null)
						methodInfo.Invoke(property.serializedObject.targetObject, null);
					else
						throw new NullReferenceException("That method not exist.");
					property.boolValue = false;
				}
			}
			else
			{
				EditorGUI.LabelField(position, label, "ContextButton only allow to use on Boolean.");
			}
			EditorGUI.EndProperty();
		}
	}
}
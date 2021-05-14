using UnityEngine;
using UnityEditor;
using Kit;

namespace Kit
{
	[CustomPropertyDrawer(typeof(CircleRangeAttribute))]
	public class CircleRangeDrawer : PropertyDrawer
	{
		const float halfSize = 55f;
		const float controlHalfSize = 5f;
		const float radius = halfSize - controlHalfSize;
		const float sqrRadius = radius * radius;
		static readonly Vector2 halfRange = Vector2.one * halfSize;

		private enum eState
		{
			Idle = 0,
			Drag,
			DragEnd,
		}
		private eState m_State = eState.Idle;

		CircleRangeAttribute rangeAttribute { get { return (CircleRangeAttribute)attribute; } }
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.Vector2)
			{
				EditorGUI.BeginProperty(position, label, property);
				Rect line = position.Clone(width: halfSize * 2f, height: halfSize * 2f).Adjust(y: 5f);
				Event evt = Event.current;
				
				// identiry mouse event.
				if (evt.type == EventType.MouseDown && line.Contains(evt.mousePosition, false))
				{
					m_State = eState.Drag;
				}
				else if (evt.type == EventType.MouseUp && m_State == eState.Drag)
				{
					m_State = eState.DragEnd;
				}

				// data source location
				Vector2 inputCircle;
				if (m_State != eState.Idle)
				{
					inputCircle = new Vector2(
						Mathf.Clamp(evt.mousePosition.x - (line.x + halfSize), -halfSize, halfSize),
						Mathf.Clamp(evt.mousePosition.y - (line.y + halfSize), -halfSize, halfSize)
						);
					if (inputCircle.sqrMagnitude > sqrRadius)
						inputCircle = inputCircle.normalized * (halfSize - controlHalfSize);
				}
				else
				{
					inputCircle = property.vector2Value.ConvertSquareToCircle().Scale(-1f, 1f, -radius, radius);
				}
				Vector2 square01 = inputCircle.Scale(-radius, radius, -1f, 1f).ConvertCircleToSquare();

				// UI
				GUI.BeginClip(line);
				Handles.BeginGUI();
				Handles.color = Color.grey;
				Handles.DrawSolidDisc(halfRange, Vector3.forward, halfSize);
				Handles.color = Color.black;
				Handles.DrawWireDisc(halfRange, Vector3.forward, halfSize);
				Handles.DrawSolidDisc(halfRange + inputCircle, Vector3.forward, controlHalfSize);
				Handles.EndGUI();
				GUI.EndClip();
				
				// Vector2 field
				line = line.GetRectBottom(height: 20f).Adjust(y: 5f);
				EditorGUI.BeginChangeCheck();
				Vector2 tmp = EditorGUI.Vector2Field(line, GUIContent.none, square01);
				if (EditorGUI.EndChangeCheck())
				{
					tmp.x = Mathf.Clamp(tmp.x, -1f, 1f);
					tmp.y = Mathf.Clamp(tmp.y, -1f, 1f);
					tmp = tmp.ConvertSquareToCircle();
					inputCircle = tmp.Scale(-1f, 1f, -radius, radius);
					if (inputCircle.sqrMagnitude > sqrRadius)
						inputCircle = inputCircle.normalized * radius;
					square01 = inputCircle.Scale(-radius, radius, -1f, 1f).ConvertCircleToSquare();
					m_State = eState.DragEnd;
				}

				// State & apply change
				if (m_State == eState.DragEnd)
				{
					m_State = eState.Idle;
					property.vector2Value = square01;
					property.serializedObject.ApplyModifiedProperties();
				}
				else if (m_State == eState.Drag || !(evt.type == EventType.Repaint || evt.type == EventType.Layout))
				{
					// lower update rate.
					EditorUtility.SetDirty(property.serializedObject.targetObject);
				}
				EditorGUI.EndProperty();
			}
			else
			{
				EditorGUI.BeginProperty(position, label, property);
				EditorGUI.HelpBox(position, typeof(CircleRangeAttribute).ToString() + " is for " + typeof(Vector2).ToString() + ".", MessageType.Error);
				EditorGUI.EndProperty();
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return halfSize + halfSize + 30f;
		}
	}
}
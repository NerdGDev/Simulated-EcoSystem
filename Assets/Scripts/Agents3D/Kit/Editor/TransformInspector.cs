using UnityEngine;
using UnityEditor;

namespace Kit
{
	[CustomEditor(typeof(Transform)), CanEditMultipleObjects]
	public class TransformInspector : Editor
	{
		const string TRANSFORM_CHANGE = "Transform Change";

		static readonly GUIContent
			labelWorldFold = new GUIContent("World Transform"),
			labelLocalFold = new GUIContent("Local Transform");
		static readonly GUIContent l_Position = new GUIContent("Position", "The world position of this Game Object.");
		static readonly GUIContent l_Rotation = new GUIContent("Rotation", "The world rotation of this Game Object.");
		static readonly GUIContent l_Scale = new GUIContent("Scale", "The world scaling of this Game Object.");
		static readonly GUIContent l_localPosition = new GUIContent("Local Position", "The local position of this Game Object relative to the parent.");
		static readonly GUIContent l_localRotation = new GUIContent("Local Rotation", "The local rotation of this Game Object relative to the parent.");
		static readonly GUIContent l_localScale = new GUIContent("Local Scale", "The local scaling of this Game Object relative to the parent.");
		
		static bool
			showWorld = true,
			showLocal = true;
		static readonly Vector3 V3Zero = new Vector3(0f, 0f, 0f); // optimize call

		private SerializedProperty localPositionProperty, localRotationProperty, localScaleProperty;
		private void OnEnable()
		{
			localPositionProperty = serializedObject.FindProperty("m_LocalPosition");
			localRotationProperty = serializedObject.FindProperty("m_LocalRotation");
			localScaleProperty = serializedObject.FindProperty("m_LocalScale");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			showWorld = EditorGUILayout.Foldout(showWorld, labelWorldFold);
			if (showWorld) OnDrawWorldTransform();

			showLocal = EditorGUILayout.Foldout(showLocal, labelLocalFold);
			if (showLocal) OnDrawLocalTransform();
		}

		private void OnDrawWorldTransform()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			EditorGUI.BeginChangeCheck();
			Transform firstTarget = ((Transform)target);
			Vector3 position = EditorGUILayout.Vector3Field(l_Position, firstTarget.position);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObjects(targets, "Transform Change");
				foreach (Transform obj in targets)
				{
					obj.position = FixIfNaN(position);
				}
				serializedObject.ApplyModifiedProperties();
			}
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.Vector3Field(l_Rotation, firstTarget.eulerAngles);
			EditorGUILayout.Vector3Field(l_Scale, firstTarget.lossyScale);
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndVertical();

			// if (GUILayout.Button("P", GUILayout.Width(20f))) position = Vector3.zero;
			if (GUILayout.Button("P", GUILayout.Width(20f)))
			{
				Undo.RecordObjects(targets, TRANSFORM_CHANGE);
				foreach (Transform obj in targets)
				{
					obj.position = V3Zero;
				}
				serializedObject.ApplyModifiedProperties();
			}
			EditorGUILayout.EndHorizontal();
		}

		private void OnDrawLocalTransform()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(localPositionProperty, l_localPosition);
			RotationPropertyField(localRotationProperty, l_localRotation);
			EditorGUILayout.PropertyField(localScaleProperty, l_localScale);
			if (EditorGUI.EndChangeCheck())
			{
				//Undo.RecordObjects(targets, TRANSFORM_CHANGE);
				serializedObject.ApplyModifiedProperties();
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(20f));
			if (GUILayout.Button("P"))
			{
				Undo.RecordObjects(targets, TRANSFORM_CHANGE);
				foreach (Transform obj in targets)
				{
					obj.localPosition = V3Zero;
				}
				serializedObject.ApplyModifiedProperties();
			}
			if (GUILayout.Button("R"))
			{
				Undo.RecordObjects(targets, TRANSFORM_CHANGE);
				foreach (Transform obj in targets)
				{
					obj.localRotation = Quaternion.identity;
				}
				serializedObject.ApplyModifiedProperties();
			}
			if (GUILayout.Button("S"))
			{
				Undo.RecordObjects(targets, TRANSFORM_CHANGE);
				foreach (Transform obj in targets)
				{
					obj.localScale = Vector3.one;
				}
				serializedObject.ApplyModifiedProperties();
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			
		}

		#region Utils
		private void RotationPropertyField(SerializedProperty rotationProperty, GUIContent content)
		{
			Transform transform = (Transform)this.targets[0];
			Quaternion localRotation = transform.localRotation;
			int cnt = targets.Length;
			for (int i = 0; i < cnt; i++)
			{
				if (!SameRotation(localRotation, ((Transform)targets[i]).localRotation))
				{
					EditorGUI.showMixedValue = true;
					break;
				}
			}
			
			EditorGUI.BeginChangeCheck();

			Vector3 eulerAngles = Repeat(EditorGUILayout.Vector3Field(content, localRotation.eulerAngles));

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObjects(this.targets, TRANSFORM_CHANGE);
				for (int i=0; i<cnt; i++)
				{
					((Transform)targets[i]).localEulerAngles = eulerAngles;
				}
				rotationProperty.serializedObject.SetIsDifferentCacheDirty();
			}

			EditorGUI.showMixedValue = false;
		}
		private Vector3 FixIfNaN(Vector3 v)
		{
			if (float.IsNaN(v.x)) { v.x = 0f; }
			if (float.IsNaN(v.y)) { v.y = 0f; }
			if (float.IsNaN(v.z)) { v.z = 0f; }
			return v;
		}
		private Vector3 Repeat(Vector3 v)
		{
			v.x = Mathf.Repeat(v.x, 360f);
			v.y = Mathf.Repeat(v.y, 360f);
			v.z = Mathf.Repeat(v.z, 360f);
			return v;
		}
		private bool SameRotation(Quaternion rot1, Quaternion rot2)
		{
			return rot1.w == rot2.w && rot1.y == rot2.y && rot1.x == rot2.x && rot1.z == rot2.z;
		}
		#endregion
	}
}
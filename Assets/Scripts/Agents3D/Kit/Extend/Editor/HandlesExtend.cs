#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Kit
{
	public static class HandlesExtend
	{
		public static Vector3 PositionHandle(
			Vector3 pos,
			Quaternion rotation,
			string controlName = "handle")
		{
			if (rotation == default(Quaternion) || Tools.pivotRotation == PivotRotation.Global)
				rotation = Quaternion.identity;

			EditorGUI.BeginChangeCheck();
			GUI.SetNextControlName(controlName);
			pos = Handles.DoPositionHandle(pos, rotation);
			if (EditorGUI.EndChangeCheck())
			{
				GUI.FocusControl(controlName);
			}
			return pos;
		}

		public static void DrawLabel(Vector3 position, string text, SceneView sceneView, GUIStyle style = default(GUIStyle), Color color = default(Color))
		{
			Transform camTransform;
			if (sceneView != null)
				camTransform = sceneView.camera.transform; // Scene View
			else if (Application.isPlaying && Camera.main != null)
				camTransform = Camera.main.transform; // Only Game View
			else
				return;

			if (Vector3.Dot(camTransform.forward, position - camTransform.position) > 0)
			{
				style = (style == default(GUIStyle)) ? GUI.skin.textArea : style;
				if (color != default(Color)) style.normal.textColor = color;
				Handles.Label(position, text, style);
			}
		}
	}
}
#endif
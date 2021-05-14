using UnityEngine;
using UnityEditor;
using Kit;
using System;
using System.Reflection;

[CustomPropertyDrawer(typeof(TransformCache)), CanEditMultipleObjects]
public class TransformCacheDrawer : PropertyDrawer
{
	static readonly GUIContent
		l_recordButton = new GUIContent("Record"),
		l_AssignButton = new GUIContent("Load"),
		l_Detail = new GUIContent("Detail"),
		l_LocalRotation = new GUIContent("Local Rotation");
	// l_ToggleArrow = new GUIContent("\u25BA");
	static float singleLine = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

	struct VariablesGroup
	{
		public readonly SerializedProperty
			localPositionProp, localRotationProp, localScaleProp,
			tranRefProp,
			anchorMinProp, anchorMaxProp, sizeDeltaProp, pivotProp;

		public VariablesGroup(SerializedProperty _property)
		{
			localPositionProp = _property.FindPropertyRelative("m_LocalPosition");
			localRotationProp = _property.FindPropertyRelative("m_LocalRotation");
			localScaleProp = _property.FindPropertyRelative("m_LocalScale");
			anchorMinProp = _property.FindPropertyRelative("m_AnchorMin");
			anchorMaxProp = _property.FindPropertyRelative("m_AnchorMax");
			sizeDeltaProp = _property.FindPropertyRelative("m_SizeDelta");
			pivotProp = _property.FindPropertyRelative("m_Pivot");
			tranRefProp = _property.FindPropertyRelative("m_TransformRef");
		}
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		property.serializedObject.UpdateIfRequiredOrScript();
		EditorGUI.BeginProperty(position, label, property);
		Rect line = position.Clone(height: EditorGUIUtility.singleLineHeight + singleLine);
		Rect[] left = line.SplitLeft(80f);
		Rect[] cols = left[1].SplitLeft(80f);
		Color orgColor = GUI.color;

		GUI.color = Color.yellow;
		EditorGUI.BeginDisabledGroup(Application.isPlaying);
		EditorGUI.BeginChangeCheck();
		if (GUI.Button(left[0], l_recordButton))
		{
			// We know the propertyPath, let's apply on multiple targets
			string _propertyPath = property.propertyPath;
			for (int i = 0; i < property.serializedObject.targetObjects.Length; i++)
			{
				Component _component = (Component)property.serializedObject.targetObjects[i];
				SerializedObject _serializedObject = new SerializedObject(_component);
				SerializedProperty _property = _serializedObject.FindProperty(_propertyPath);
				VariablesGroup _data = new VariablesGroup(_property);
				Undo.RecordObjects(new[] { _component, _component.transform }, "Record Transform");
				_data.localPositionProp.vector3Value = _component.transform.localPosition;
				if (_component.transform is RectTransform)
				{
					RectTransform obj = (RectTransform)_component.transform;
					_data.anchorMinProp.vector2Value = obj.anchorMin;
					_data.anchorMaxProp.vector2Value = obj.anchorMax;
					_data.sizeDeltaProp.vector2Value = obj.sizeDelta;
					_data.pivotProp.vector2Value = obj.pivot;
					_data.localPositionProp.vector3Value = obj.anchoredPosition3D;
				}
				_data.localRotationProp.quaternionValue = _component.transform.localRotation;
				_data.localScaleProp.vector3Value = _component.transform.localScale;
				_property.serializedObject.ApplyModifiedProperties();

				InvokeMethod(
					_component,
					TransformCache.Editor_Record_Callback,
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
					new[] { _propertyPath });
			}
		}
		GUI.color = Color.green;
		if (GUI.Button(cols[0], l_AssignButton))
		{
			// We know the propertyPath, let's apply on multiple targets
			string _propertyPath = property.propertyPath;
			for (int i = 0; i < property.serializedObject.targetObjects.Length; i++)
			{
				Component _component = (Component)property.serializedObject.targetObjects[i];
				SerializedObject _serializedObject = new SerializedObject(_component);
				SerializedProperty _property = _serializedObject.FindProperty(_propertyPath);
				TransformCache cache = CloneTransformCache(new VariablesGroup(_property));

				Undo.RecordObjects(new[] { _component, _component.transform }, "Load Transform");
				cache.AssignTo(_component.transform);
				_property.serializedObject.ApplyModifiedProperties();

				InvokeMethod(
					_component,
					TransformCache.Editor_Load_Callback,
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
					new[] { _propertyPath });
			}
		}
		if (EditorGUI.EndChangeCheck() && !Application.isPlaying)
		{
			EditorUtility.SetDirty(property.serializedObject.targetObject);
		}
		EditorGUI.EndDisabledGroup();

		VariablesGroup data = new VariablesGroup(property);
		Component component = (Component)property.serializedObject.targetObject;

		GUI.color = orgColor;
		property.isExpanded = EditorGUI.Foldout(cols[1].Adjust(x: 15f).Clone(width: 50f), property.isExpanded, l_Detail, true);
		TransformCache.eTransformRef tranRef = (TransformCache.eTransformRef)data.tranRefProp.enumValueIndex;

		if (property.isExpanded)
		{
			if (component.transform is RectTransform)
			{
				RectTransform rectTran = (RectTransform)component.transform;
				line = line.GetRectBottom(height: singleLine);
				DrawRectButton(line, data.tranRefProp,
					TransformCache.eTransformRef.AnchorMin, data.anchorMinProp,
					() => { data.anchorMinProp.vector2Value = rectTran.anchorMin; });

				line = line.GetRectBottom(height: singleLine);
				DrawRectButton(line, data.tranRefProp,
					TransformCache.eTransformRef.AnchorMax, data.anchorMaxProp,
					() => { data.anchorMaxProp.vector2Value = rectTran.anchorMax; });

				line = line.GetRectBottom(height: singleLine);
				DrawRectButton(line, data.tranRefProp,
					TransformCache.eTransformRef.SizeDelta, data.sizeDeltaProp,
					() => { data.sizeDeltaProp.vector2Value = rectTran.sizeDelta; });


				line = line.GetRectBottom(height: singleLine);
				DrawRectButton(line, data.tranRefProp,
					TransformCache.eTransformRef.Pivot, data.pivotProp,
					() => { data.pivotProp.vector2Value = rectTran.pivot; });
			}

			// position
			line = line.GetRectBottom(height: singleLine);
			DrawRectButton(line, data.tranRefProp,
				TransformCache.eTransformRef.LocalPosition, data.localPositionProp,
				() => {
					if (component.transform is RectTransform)
						data.localPositionProp.vector3Value = ((RectTransform)component.transform).anchoredPosition3D;
					else
						data.localPositionProp.vector3Value = component.transform.localPosition;
				});

			// rotation
			line = line.GetRectBottom(height: singleLine);
			DrawRectButton(line, data.tranRefProp,
				TransformCache.eTransformRef.LocalRotation, data.localRotationProp,
				() => { data.localRotationProp.quaternionValue = component.transform.localRotation; },
				DrawQuaternionParam);
			// TODO: 
			// Vector3 rotate = EditorGUI.Vector3Field(cols[1], l_LocalRotation, data.localRotationProp.quaternionValue.eulerAngles);
			//	data.localRotationProp.quaternionValue = Quaternion.Euler(rotate);

			// scale
			line = line.GetRectBottom(height: singleLine);
			DrawRectButton(line, data.tranRefProp,
				TransformCache.eTransformRef.LocalScale, data.localScaleProp,
				() => { data.localScaleProp.vector3Value = component.transform.localScale; });
		}

		EditorGUI.EndProperty();
	}

	private void DrawRectButton(Rect line,
		SerializedProperty tranRefProp,
		TransformCache.eTransformRef param,
		SerializedProperty relatedProp,
		Callback btnCallback,
		ToggleFunc toggleSession)
	{
		Rect[] cols = line.SplitLeft(50f);
		Rect recBtn = cols[0];
		if (GUI.Button(recBtn, l_recordButton))
		{
			// Undo.RecordObjects(new[] { component, component.transform }, "Record Transform");
			btnCallback();
			EditorUtility.SetDirty(relatedProp.serializedObject.targetObject);
		}

		toggleSession(cols[1], tranRefProp, param, relatedProp);
	}

	private void DrawRectButton(Rect line,
		SerializedProperty tranRefProp,
		TransformCache.eTransformRef param,
		SerializedProperty relatedProp,
		Callback btnCallback)
	{
		DrawRectButton(line, tranRefProp, param, relatedProp, btnCallback, DrawDefaultParam);
	}

	delegate void ToggleFunc(Rect line, SerializedProperty tranRefProp, TransformCache.eTransformRef param, SerializedProperty relatedProp);

	private void DrawDefaultParam(
		Rect line,
		SerializedProperty tranRefProp,
		TransformCache.eTransformRef param,
		SerializedProperty relatedProp)
	{
		TransformCache.eTransformRef tranRef = (TransformCache.eTransformRef)tranRefProp.intValue;
		bool org = (tranRef & param) != 0;
		Rect[] cols = line.SplitLeft(20f);

		EditorGUI.BeginChangeCheck();
		bool after = EditorGUI.Toggle(cols[0], GUIContent.none, org);
		if (EditorGUI.EndChangeCheck() && after != org)
		{
			tranRef ^= param;
			int val = (int)tranRef;
			tranRefProp.intValue = val;
			EditorUtility.SetDirty(tranRefProp.serializedObject.targetObject);
		}
		EditorGUI.BeginDisabledGroup(!after);
		EditorGUI.PropertyField(cols[1], relatedProp);
		EditorGUI.EndDisabledGroup();
	}

	private void DrawQuaternionParam(
		Rect line,
		SerializedProperty tranRefProp,
		TransformCache.eTransformRef param,
		SerializedProperty relatedProp)
	{
		TransformCache.eTransformRef tranRef = (TransformCache.eTransformRef)tranRefProp.intValue;
		bool org = (tranRef & param) != 0;
		Rect[] cols = line.SplitLeft(20f);
		EditorGUI.BeginChangeCheck();
		bool after = EditorGUI.Toggle(cols[0], GUIContent.none, org);
		if (EditorGUI.EndChangeCheck() && after != org)
		{
			tranRef ^= param;
			int val = (int)tranRef;
			tranRefProp.intValue = val;
			EditorUtility.SetDirty(tranRefProp.serializedObject.targetObject);
		}

		EditorGUI.BeginDisabledGroup(!after);

		EditorGUI.BeginChangeCheck();
		Vector3 rotate = EditorGUI.Vector3Field(cols[1], l_LocalRotation, relatedProp.quaternionValue.eulerAngles);
		if (EditorGUI.EndChangeCheck())
		{
			relatedProp.quaternionValue = Quaternion.Euler(rotate);
		}
		// EditorGUI.PropertyField(cols[1], relatedProp);
		EditorGUI.EndDisabledGroup();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return GetPropertyHeight(property);
	}

	public static float GetPropertyHeight(SerializedProperty property)
	{
		if (!property.isExpanded)
			return 2f * singleLine;
		else
		{
			Component component = (Component)property.serializedObject.targetObject;
			if (component.transform is RectTransform)
				return 9f * singleLine;
			else
				return 5f * singleLine;
		}
	}

	private TransformCache CloneTransformCache(VariablesGroup data)
	{
		TransformCache rst = new TransformCache();
		rst.m_LocalPosition = data.localPositionProp.vector3Value;
		rst.m_LocalRotation = data.localRotationProp.quaternionValue;
		rst.m_LocalScale = data.localScaleProp.vector3Value;
		rst.m_AnchorMin = data.anchorMinProp.vector2Value;
		rst.m_AnchorMax = data.anchorMaxProp.vector2Value;
		rst.m_SizeDelta = data.sizeDeltaProp.vector2Value;
		rst.m_Pivot = data.pivotProp.vector2Value;
		rst.m_TransformRef = (TransformCache.eTransformRef)data.tranRefProp.intValue;
		return rst;
	}

	/// <summary>Use reflection to invoke function by Name</summary>
	/// <param name="obj">This object</param>
	/// <param name="functionName">function name in string</param>
	/// <param name="bindingFlags"><see cref="BindingFlags"/></param>
	/// <param name="args">The values you wanted to pass, will trim out if destination params less than provider.</param>
	/// <returns></returns>
	public bool InvokeMethod(object obj, string functionName, BindingFlags bindingFlags, params object[] args)
	{
		Type type = obj.GetType();
		MethodInfo method = type.GetMethod(functionName, bindingFlags);
		if (method != null)
		{
			int length = method.GetParameters().Length;
			if (length > args.Length)
			{
				throw new ArgumentOutOfRangeException("Destination parameter(s) are required " + length + ", but system provided " + args.Length);
			}
			else
			{
				object[] trimArgs = new object[length];
				Array.Copy(args, trimArgs, length);
				method.Invoke(obj, trimArgs);
				return true;
			}
		}
		return false;
	}
}

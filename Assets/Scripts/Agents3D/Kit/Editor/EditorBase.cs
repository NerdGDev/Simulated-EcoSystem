using UnityEditor;

public abstract class EditorBase : Editor
{
	SerializedProperty scriptProp;
	protected virtual void OnEnable()
	{
		scriptProp = this.serializedObject.FindProperty("m_Script");
	}

	public sealed override void OnInspectorGUI()
	{
		serializedObject.UpdateIfRequiredOrScript();
		SerializedProperty iter = serializedObject.GetIterator();
		iter.NextVisible(true); // enter children.

		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.PropertyField(scriptProp, includeChildren: true);
		EditorGUI.EndDisabledGroup();

		EditorGUI.BeginChangeCheck(); // ---- change check [optional]
		OnBeforeDrawGUI();

		do
		{
			if (scriptProp != null && iter.propertyPath == scriptProp.propertyPath)
			{
				// skip
			}
			else
			{
				OnDrawProperty(iter);
			}
		}
		while (iter.NextVisible(false));

		OnAfterDrawGUI();
		if (EditorGUI.EndChangeCheck()) // ---- change check [optional]
			serializedObject.ApplyModifiedProperties();
	}

	protected virtual void OnBeforeDrawGUI() { }
	protected virtual void OnAfterDrawGUI() { }

	protected virtual void OnDrawProperty(SerializedProperty property)
	{
		EditorGUILayout.PropertyField(property, includeChildren: true);
	}
}
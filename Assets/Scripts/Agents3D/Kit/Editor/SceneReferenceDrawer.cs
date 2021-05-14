using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Kit
{
	/// <summary>Get scene name from build setting. and put it back to string field.</summary>
	[CustomPropertyDrawer(typeof(SceneReferenceAttribute))]
	public class SceneReferenceDrawer : PropertyDrawer
	{
		SceneReferenceAttribute sceneReferenceAttribute { get { return (SceneReferenceAttribute)attribute; } }

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			Rect line = position;
			if (property.propertyType != SerializedPropertyType.String)
			{
				EditorGUI.LabelField(line, "Only for string type property.");
			}
			else if (EditorSceneManager.sceneCountInBuildSettings == 0)
			{
				EditorGUI.LabelField(line, "Please add scene in build setting.");
			}
			else if (EditorSceneManager.sceneCountInBuildSettings > 0)
			{
				/// <see cref="http://answers.unity3d.com/questions/33263/how-to-get-names-of-all-available-levels.html"/>
				string[] sceneList = new string[EditorSceneManager.sceneCountInBuildSettings + 1];
				sceneList[0] = "-None-";
				string oldName = property.stringValue;
				int selected = 0;
				for (int i = 1; i < sceneList.Length; ++i)
				{
					string sceneName = EditorBuildSettings.scenes[i - 1].path;
					// sceneList[i] = sceneName.Substring(0, sceneName.Length - 6).Substring(sceneName.LastIndexOf('/') + 1);
					sceneList[i] = System.IO.Path.GetFileNameWithoutExtension(sceneName);
					if (sceneList[i] == oldName)
						selected = i;
				}

				if (sceneReferenceAttribute.IsShowLabel)
					line = EditorGUI.PrefixLabel(line, new GUIContent(property.displayName));

				EditorGUI.BeginChangeCheck();
				selected = EditorGUI.Popup(line, selected, sceneList);
				if (EditorGUI.EndChangeCheck())
				{
					property.stringValue = selected == 0 ? string.Empty : sceneList[selected];
					property.serializedObject.ApplyModifiedProperties();
				}
			}
			EditorGUI.EndProperty();
		}
	}
}
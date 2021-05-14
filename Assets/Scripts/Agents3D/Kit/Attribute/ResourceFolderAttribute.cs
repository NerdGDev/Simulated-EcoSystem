#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace Kit
{
	/// <summary>Resource folder attribute.</summary>
	[Serializable]
	public class ResourceFolderAttribute : PropertyAttribute
	{
		public readonly string title, defaultName, helpMessage;

		/// <summary>Initializes a new instance of the <see cref="Kit.Resource.ResourceFolderAttribute"/> class.</summary>
		/// <param name="title">Title of popup screen.</param>
		/// <param name="defaultName">Default name to search.</param>
		/// <param name="helpMessage">Help message, when not matching.</param>
		public ResourceFolderAttribute(string title, string defaultName, string helpMessage)
		{
			this.title = title;
			this.defaultName = defaultName;
			this.helpMessage = helpMessage;
		}
		public ResourceFolderAttribute()
			: this("Select Resource Folder", "", "Folder must put in \"Resources\" folder")
		{ }
	}
	[CustomPropertyDrawer(typeof(ResourceFolderAttribute))]
	public class ResourceDrawer : PropertyDrawer
	{
		const string RESOURCE_FOLDER = "resources/";

		const float
			BUTTON_HEIGHT = 16f,
			HELP_HEIGHT = 30f;

		// Provide easy access to the RegexAttribute for reading information from it.
		// ResourceFolderAttribute resourceFolder { get { return ((ResourceFolderAttribute)attribute); } }
		ResourceFolderAttribute resourceFolder { get { return (ResourceFolderAttribute)attribute; } }

		public override void OnGUI(UnityEngine.Rect position, SerializedProperty property, UnityEngine.GUIContent label)
		{
			Rect buttonPosition = EditorGUI.PrefixLabel(position, label);
			buttonPosition.height = BUTTON_HEIGHT;
			string _btn = "Select Folder";
			if (IsResourseFolder(property))
				_btn = "Resources:/ " + property.stringValue;

			if (GUI.Button(buttonPosition, _btn))
			{
				string _string = EditorUtility.OpenFolderPanel(resourceFolder.title, "", resourceFolder.defaultName);
				if (!string.IsNullOrEmpty(_string))
				{
					bool _check = _string.ToLower().IndexOf(RESOURCE_FOLDER) > 0 && !_string.ToLower().EndsWith(RESOURCE_FOLDER);
					if (_check)
					{
						property.stringValue = GetShortPath(_string);
					}
					else
					{
						property.stringValue = string.Empty;
					}
				}
			}

			Rect helpPosition = EditorGUI.IndentedRect(position);
			helpPosition.y += buttonPosition.height;
			helpPosition.height = 30f;
			if (!IsResourseFolder(property))
			{
				EditorGUI.HelpBox(helpPosition, resourceFolder.helpMessage, MessageType.Warning);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (IsResourseFolder(property))
				return base.GetPropertyHeight(property, label);
			else
				return base.GetPropertyHeight(property, label) + HELP_HEIGHT;
		}

		bool IsResourseFolder(SerializedProperty _prop)
		{
			return !string.IsNullOrEmpty(_prop.stringValue);
		}

		string GetShortPath(string fullPath)
		{
			return fullPath.Substring(fullPath.ToLower().IndexOf(RESOURCE_FOLDER) + RESOURCE_FOLDER.Length);
		}
	}
}
#endif
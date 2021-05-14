#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using InternalEditorUtility = UnityEditorInternal.InternalEditorUtility;
using Type = System.Type;
using Path = System.IO.Path;
using File = System.IO.File;
using PropertyInfo = System.Reflection.PropertyInfo;
using BindingFlags = System.Reflection.BindingFlags;

namespace Kit
{
    public sealed class EditorExtend
    {
        #region SortingLayer
        public static string[] GetSortingLayerNames()
        {
            Type internalEditorUtilityType = typeof(InternalEditorUtility);
            PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
            return (string[])sortingLayersProperty.GetValue(null, new object[0]);
        }
        public static int[] GetSortingLayerUniqueIDs()
        {
            Type internalEditorUtilityType = typeof(InternalEditorUtility);
            PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
            return (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
        }
        public static int SortingLayerField(Rect position, SerializedProperty property)
        {
            return SortingLayerField(position, property, property.displayName);
        }
        public static int SortingLayerField(Rect position, SerializedProperty property, string label)
        {
            int selectedIndex = property.intValue;
            string[] values = GetSortingLayerNames();
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUI.Popup(position, label, selectedIndex, values);
            if (selectedIndex >= values.Length)
            {
                selectedIndex = 0; // hotfix
                property.intValue = selectedIndex;
            }
            if(EditorGUI.EndChangeCheck())
            {
                property.intValue = selectedIndex;
            }
            return selectedIndex;
        }
		#endregion

		#region Tag
		/// <summary>lazy override for string, <see cref="TagFieldDrawer"/></summary>
		/// <param name="position"></param>
		/// <param name="property"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static string TagField(Rect position, SerializedProperty property, GUIContent label)
		{
			string layerName = property.stringValue;
			EditorGUI.BeginChangeCheck();
			if (string.IsNullOrEmpty(layerName))
			{
				layerName = "Untagged";
				property.stringValue = layerName;
			}
			layerName = EditorGUI.TagField(position, label, layerName);
			if (EditorGUI.EndChangeCheck())
			{
				property.stringValue = layerName;
			}
			return layerName;
		}
		#endregion

		#region LayerField
		/// <summary>Inspector drawer for single layerMask selection.</summary>
		/// <param name="selected"></param>
		/// <returns></returns>
		public static int LayerFieldSingle(LayerMask selected)
		{
			System.Collections.ArrayList layers = new System.Collections.ArrayList();
			System.Collections.ArrayList layerNumbers = new System.Collections.ArrayList();
			string name = "";
			for (int i = 0; i < 32; i++)
			{
				string layerName = LayerMask.LayerToName(i);
				if (!string.IsNullOrEmpty(layerName))
				{
					if (selected == (selected | (1 << i)))
					{
						layers.Add("> " + layerName);
						name += layerName + ", ";
					}
					else
					{
						layers.Add(layerName);
					}
					layerNumbers.Add(i);
				}
			}
			
			int[] LayerNumbers = layerNumbers.ToArray(typeof(int)) as int[];
			EditorGUI.BeginChangeCheck();
			int newSelected = EditorGUILayout.Popup("Mask", -1, layers.ToArray(typeof(string)) as string[], EditorStyles.layerMaskField);
			if (EditorGUI.EndChangeCheck())
			{
				if (selected == (selected | (1 << LayerNumbers[newSelected])))
				{
					selected = ~(1 << LayerNumbers[newSelected]);
					Debug.Log("Set Layer " + LayerMask.LayerToName(LayerNumbers[newSelected]) + " To False " + selected.value);
				}
				else
				{
					Debug.Log("Set Layer " + LayerMask.LayerToName(LayerNumbers[newSelected]) + " To True " + selected.value);
					selected = selected | (1 << LayerNumbers[newSelected]);
				}
			}
			return selected;
		}
		#endregion

		#region Text AutoComplete
		/// <summary>The internal struct used for AutoComplete (Editor)</summary>
		private struct EditorAutoCompleteParams
		{
			public const string FieldTag = "AutoCompleteField";
			public static readonly Color FancyColor = new Color(.6f, .6f, .7f);
			public static readonly float optionHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			public const int fuzzyMatchBias = 3; // input length smaller then this letter, will not trigger fuzzy checking.
			public static List<string> CacheCheckList = null;
			public static string lastInput;
			public static string focusTag = "";
			public static string lastTag = ""; // Never null, optimize for length check.
			public static int selectedOption = -1; // record current selected option.
			public static Vector2 mouseDown;

			public static void CleanUpAndBlur()
			{
				selectedOption = -1;
				GUI.FocusControl("");
			}
		}

		/// <summary>A textField to popup a matching popup, based on developers input values.</summary>
		/// <param name="input">string input.</param>
		/// <param name="source">the data of all possible values (string).</param>
		/// <param name="maxShownCount">the amount to display result.</param>
		/// <param name="levenshteinDistance">
		/// value between 0f ~ 1f, (percent)
		/// - more then 0f will enable the fuzzy matching
		/// - 1f = 100% error threshold = anything thing is okay.
		/// - 0f = 000% error threshold = require full match to the reference
		/// - recommend 0.4f ~ 0.7f
		/// </param>
		/// <returns>output string.</returns>
		public static string TextFieldAutoComplete(string input, string[] source, int maxShownCount = 5, float levenshteinDistance = 0.5f)
		{
			return TextFieldAutoComplete(EditorGUILayout.GetControlRect(), input, source, maxShownCount, levenshteinDistance);
		}

		/// <summary>A textField to popup a matching popup, based on developers input values.</summary>
		/// <param name="position">EditorGUI position</param>
		/// <param name="input">string input.</param>
		/// <param name="source">the data of all possible values (string).</param>
		/// <param name="maxShownCount">the amount to display result.</param>
		/// <param name="levenshteinDistance">
		/// value between 0f ~ 1f, (percent)
		/// - more then 0f will enable the fuzzy matching
		/// - 1f = 100% error threshold = everything is okay.
		/// - 0f = 000% error threshold = require full match to the reference
		/// - recommend 0.4f ~ 0.7f
		/// </param>
		/// <returns>output string.</returns>
		public static string TextFieldAutoComplete(Rect position, string input, string[] source, int maxShownCount = 5, float levenshteinDistance = 0.5f)
		{
			// Text field
			int controlId = GUIUtility.GetControlID(FocusType.Passive);
			string tag = EditorAutoCompleteParams.FieldTag + controlId;
			GUI.SetNextControlName(tag);
			string rst = EditorGUI.TextField(position, input, EditorStyles.popup);

			// Matching with giving source
			if (input.Length > 0 && // have input
				(EditorAutoCompleteParams.lastTag.Length == 0 || EditorAutoCompleteParams.lastTag == tag) && // one frame delay for process click event.
				GUI.GetNameOfFocusedControl() == tag) // focus this control
			{
				// Matching
				if (EditorAutoCompleteParams.lastInput != input || // input changed
					EditorAutoCompleteParams.focusTag != tag) // switch focus from another field.
				{
					// Update cache
					EditorAutoCompleteParams.focusTag = tag;
					EditorAutoCompleteParams.lastInput = input;

					List<string> uniqueSrc = new List<string>(new HashSet<string>(source)); // remove duplicate
					int srcCnt = uniqueSrc.Count;
					EditorAutoCompleteParams.CacheCheckList = new List<string>(System.Math.Min(maxShownCount, srcCnt)); // optimize memory alloc
					// Start with - slow
					for (int i = 0; i < srcCnt && EditorAutoCompleteParams.CacheCheckList.Count < maxShownCount; i++)
					{
						if (uniqueSrc[i].ToLower().StartsWith(input.ToLower()))
						{
							EditorAutoCompleteParams.CacheCheckList.Add(uniqueSrc[i]);
							uniqueSrc.RemoveAt(i);
							srcCnt--;
							i--;
						}
					}

					// Contains - very slow
					if (EditorAutoCompleteParams.CacheCheckList.Count == 0)
					{
						for (int i = 0; i < srcCnt && EditorAutoCompleteParams.CacheCheckList.Count < maxShownCount; i++)
						{
							if (uniqueSrc[i].ToLower().Contains(input.ToLower()))
							{
								EditorAutoCompleteParams.CacheCheckList.Add(uniqueSrc[i]);
								uniqueSrc.RemoveAt(i);
								srcCnt--;
								i--;
							}
						}
					}

					// Levenshtein Distance - very very slow.
					if (levenshteinDistance > 0f && // only developer request
						input.Length > EditorAutoCompleteParams.fuzzyMatchBias && // bias on input, hidden value to avoid doing it too early.
						EditorAutoCompleteParams.CacheCheckList.Count < maxShownCount) // have some empty space for matching.
					{
						levenshteinDistance = Mathf.Clamp01(levenshteinDistance);
						string keywords = input.ToLower();
						for (int i = 0; i < srcCnt && EditorAutoCompleteParams.CacheCheckList.Count < maxShownCount; i++)
						{
							int distance = Kit.StringExtend.LevenshteinDistance(uniqueSrc[i], keywords, caseSensitive: false);
							bool closeEnough = (int)(levenshteinDistance * uniqueSrc[i].Length) > distance;
							if (closeEnough)
							{
								EditorAutoCompleteParams.CacheCheckList.Add(uniqueSrc[i]);
								uniqueSrc.RemoveAt(i);
								srcCnt--;
								i--;
							}
						}
					}
				}

				// Draw recommend keyward(s)
				if (EditorAutoCompleteParams.CacheCheckList.Count > 0)
				{
					Event evt = Event.current;
					int cnt = EditorAutoCompleteParams.CacheCheckList.Count;
					float height = cnt * EditorAutoCompleteParams.optionHeight;
					Rect area = new Rect(position.x, position.y - height, position.width, height);
					
					// Fancy color UI
					EditorGUI.BeginDisabledGroup(true);
					EditorGUI.DrawRect(area, EditorAutoCompleteParams.FancyColor);
					GUI.Label(area, GUIContent.none, GUI.skin.button);
					EditorGUI.EndDisabledGroup();

					// Click event hack - part 1
					// cached data for click event hack.
					if (evt.type == EventType.Repaint)
					{
						// Draw option(s), if we have one.
						// in repaint cycle, we only handle display.
						Rect line = new Rect(area.x, area.y, area.width, EditorAutoCompleteParams.optionHeight);
						EditorGUI.indentLevel++;
						for (int i = 0; i < cnt; i++)
						{
							EditorGUI.ToggleLeft(line, GUIContent.none, (input == EditorAutoCompleteParams.CacheCheckList[i]));
							Rect indented = EditorGUI.IndentedRect(line);
							if (line.Contains(evt.mousePosition))
							{
								// hover style
								EditorGUI.LabelField(indented, EditorAutoCompleteParams.CacheCheckList[i], GUI.skin.textArea);
								EditorAutoCompleteParams.selectedOption = i;

								GUIUtility.hotControl = controlId; // required for Cursor skin. (AddCursorRect)
								EditorGUIUtility.AddCursorRect(area, MouseCursor.ArrowPlus);
							}
							else if (EditorAutoCompleteParams.selectedOption == i)
							{
								// hover style
								EditorGUI.LabelField(indented, EditorAutoCompleteParams.CacheCheckList[i], GUI.skin.textArea);
							}
							else
							{
								EditorGUI.LabelField(indented, EditorAutoCompleteParams.CacheCheckList[i], EditorStyles.label);
							}
							line.y += line.height;
						}
						EditorGUI.indentLevel--;

						// when hover popup, record this as the last usein tag.
						if (area.Contains(evt.mousePosition) && EditorAutoCompleteParams.lastTag != tag)
						{
							// Debug.Log("->" + tag + " Enter popup: " + area);
							// used to trigger the clicked checking later.
							EditorAutoCompleteParams.lastTag = tag;
						}
					}
					else if (evt.type == EventType.MouseDown)
					{
						if (area.Contains(evt.mousePosition) || position.Contains(evt.mousePosition))
						{
							EditorAutoCompleteParams.mouseDown = evt.mousePosition;
						}
						else
						{
							// click outside popup area, deselected - blur.
							EditorAutoCompleteParams.CleanUpAndBlur();
						}
					}
					else if (evt.type == EventType.MouseUp)
					{
						if (position.Contains(evt.mousePosition))
						{
							// common case click on textfield.
							return rst;
						}
						else if (area.Contains(evt.mousePosition))
						{
							if (Vector2.Distance(EditorAutoCompleteParams.mouseDown, evt.mousePosition) >= 3f)
							{
								// Debug.Log("Click and drag out the area.");
								return rst;
							}
							else
							{
								// Click event hack - part 3
								// for some reason, this session only run when popup display on inspector empty space.
								// when any selectable field behind of the popup list, Unity3D can't reaching this session.
								_AutoCompleteClickhandle(position, ref rst);
								EditorAutoCompleteParams.focusTag = string.Empty; // Clean up
								EditorAutoCompleteParams.lastTag = string.Empty; // Clean up
							}
						}
						else
						{
							// click outside popup area, deselected - blur.
							EditorAutoCompleteParams.CleanUpAndBlur();
						}
						return rst;
					}
					else if (evt.isKey && evt.type == EventType.KeyUp)
					{
						switch (evt.keyCode)
						{
							case KeyCode.PageUp:
							case KeyCode.UpArrow:
								EditorAutoCompleteParams.selectedOption--;
								if (EditorAutoCompleteParams.selectedOption < 0)
									EditorAutoCompleteParams.selectedOption = EditorAutoCompleteParams.CacheCheckList.Count - 1;
								break;
							case KeyCode.PageDown:
							case KeyCode.DownArrow:
								EditorAutoCompleteParams.selectedOption++;
								if (EditorAutoCompleteParams.selectedOption >= EditorAutoCompleteParams.CacheCheckList.Count)
									EditorAutoCompleteParams.selectedOption = 0;
								break;

							case KeyCode.KeypadEnter:
							case KeyCode.Return:
								if (EditorAutoCompleteParams.selectedOption != -1)
								{
									_AutoCompleteClickhandle(position, ref rst);
									EditorAutoCompleteParams.focusTag = string.Empty; // Clean up
									EditorAutoCompleteParams.lastTag = string.Empty; // Clean up
								}
								else
								{
									EditorAutoCompleteParams.CleanUpAndBlur();
								}
								break;

							case KeyCode.Escape:
								EditorAutoCompleteParams.CleanUpAndBlur();
								break;

							default:
								// hit any other key(s), assume typing, avoid override by Enter;
								EditorAutoCompleteParams.selectedOption = -1;
								break;
						}
					}
				}
			}
			else if (EditorAutoCompleteParams.lastTag == tag &&
				GUI.GetNameOfFocusedControl() != tag)
			{
				// Click event hack - part 2
				// catching mouse click on blur
				_AutoCompleteClickhandle(position, ref rst);
				EditorAutoCompleteParams.lastTag = string.Empty; // reset
			}

			return rst;
		}

		/// <summary>calculate auto complete select option location, and select it.
		/// within area, and we display option in "Vertical" style.
		/// which line is what we care.
		/// </summary>
		/// <param name="rst">input string, may overrided</param>
		/// <param name="cnt"></param>
		/// <param name="area"></param>
		/// <param name="mouseY"></param>
		private static void _AutoCompleteClickhandle(Rect position, ref string rst)
		{
			int index = EditorAutoCompleteParams.selectedOption;
			Vector2 pos = EditorAutoCompleteParams.mouseDown; // hack: assume mouse are stay in click position (1 frame behind).

			if (0 <= index && index < EditorAutoCompleteParams.CacheCheckList.Count)
			{
				rst = EditorAutoCompleteParams.CacheCheckList[index];
				GUI.changed = true;
				// Debug.Log("Selecting index (" + EditorAutoCompleteParams.selectedOption + ") "+ rst);
			}
			else
			{
				// Fail safe, when selectedOption failure
				
				int cnt = EditorAutoCompleteParams.CacheCheckList.Count;
				float height = cnt * EditorAutoCompleteParams.optionHeight;
				Rect area = new Rect(position.x, position.y - height, position.width, height);
				if (!area.Contains(pos))
					return; // return early.

				float lineY = area.y;
				for (int i = 0; i < cnt; i++)
				{
					if (lineY < pos.y && pos.y < lineY + EditorAutoCompleteParams.optionHeight)
					{
						rst = EditorAutoCompleteParams.CacheCheckList[i];
						Debug.LogError("Fail to select on \"" + EditorAutoCompleteParams.lastTag + "\" selected = " + rst + "\ncalculate by mouse position.");
						GUI.changed = true;
						break;
					}
					lineY += EditorAutoCompleteParams.optionHeight;
				}
			}

			EditorAutoCompleteParams.CleanUpAndBlur();
		}
		#endregion

		#region Object Field for Project files
		/// <summary>Keep reference to target "extension" files.</summary>
		/// <param name="ObjectProp">The serializedProperty from UnityEngine.Object.</param>
		/// <param name="extension">The file extension, within project folder.</param>
		/// <param name="title">The message wanted to display when developer click on file panel button.</param>
		/// <param name="OnBecomeNull">The callback while ObjectField become Null.</param>
		/// <param name="OnSuccess">The callback while ObjectField assign correct file type.</param>
		/// <param name="OnSuccessReadText">
		/// The callback while ObjectField assign correct file type.
		/// this will try to read all text from target file.
		/// </param>
		/// <param name="OnSuccessReadBytes">
		/// The callback while ObjectField assign correct file type.
		/// this will try to read all data as bytes[] from target file.
		/// </param>
		public static void ProjectFileField(SerializedProperty ObjectProp,
			string extension = "txt",
			string title = "",
			System.Action OnBecomeNull = null,
			System.Action OnSuccess = null,
			System.Action<string> OnSuccessReadText = null,
			System.Action<byte[]> OnSuccessReadBytes = null)
		{
			if (ObjectProp.propertyType != SerializedPropertyType.ObjectReference)
			{
				EditorGUILayout.HelpBox("Only available for Object field.", MessageType.Error);
				return;
			}
			// necessary infos. 
			string oldAssetPath = ObjectProp.objectReferenceValue ? AssetDatabase.GetAssetPath(ObjectProp.objectReferenceValue) : null;
			extension = extension.TrimStart('.').ToLower();

			// Editor draw
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(ObjectProp, true);
			if (GUILayout.Button("*."+extension, GUILayout.Width(80f)))
			{
				// Locate file by file panel.
				title = string.IsNullOrEmpty(title) ?
						"Open *." + extension + " file" :
						title;
				oldAssetPath = string.IsNullOrEmpty(oldAssetPath) ?
						Application.dataPath :
						Path.GetDirectoryName(oldAssetPath);

				string path = EditorUtility.OpenFilePanel(title, oldAssetPath, extension);
				string assetPath = string.IsNullOrEmpty(path) ? null : FileUtil.GetProjectRelativePath(path);
				if (!string.IsNullOrEmpty(assetPath))
				{
					Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
					if (obj == null)
						throw new System.InvalidProgramException();
					ObjectProp.objectReferenceValue = obj;
					ObjectProp.serializedObject.ApplyModifiedProperties();
				}
				// else cancel
			}
			EditorGUILayout.EndHorizontal();

			// Change check.
			if (EditorGUI.EndChangeCheck())
			{
				Object obj = ObjectProp.objectReferenceValue;
				string assetPath = obj ? AssetDatabase.GetAssetPath(obj) : null;
				string fileExt = string.IsNullOrEmpty(assetPath) ? null : Path.GetExtension(assetPath);
				
				// we got things, so what is that ?
				bool match = obj != null;

				// vaild path
				if (match)
				{
					match &= !string.IsNullOrEmpty(assetPath);
					if (!match)
						throw new System.InvalidProgramException("Error: " + obj + " have invalid path.");
				}

				// vaild extension
				if (match)
				{
					match &= fileExt.TrimStart('.').ToLower() == extension;
					if (!match)
						EditorUtility.DisplayDialog("Wrong file type",
							"Wrong file assigned !" +
							"\n\t"+ObjectProp.serializedObject.targetObject.name + " > " + ObjectProp.displayName +
							"\n\tCan only accept [*." + extension + "] asset.",
							"Ok !");
						// Debug.LogError("Wrong file type, only accept [*." + extension + "] asset.", ObjectProp.serializedObject.targetObject);
				}

				if (match)
				{
					// seem like we got what we needed.
					ObjectProp.serializedObject.ApplyModifiedProperties();
					if (OnSuccess != null)
						OnSuccess();
					if (OnSuccessReadText != null)
					{
						string txt = File.ReadAllText(assetPath);
						OnSuccessReadText(txt);
					}
					if (OnSuccessReadBytes!=null)
					{
						byte[] bytes = File.ReadAllBytes(assetPath);
						OnSuccessReadBytes(bytes);
					}
				}
				else
				{
					ObjectProp.objectReferenceValue = null;
					ObjectProp.serializedObject.ApplyModifiedProperties();
					if (OnBecomeNull != null)
						OnBecomeNull();
					return;
				}
			}
		}
		#endregion

		/// <summary><see cref="http://stackoverflow.com/questions/720157/finding-all-classes-with-a-particular-attribute"/></summary>
		/// <typeparam name="TAttribute"></typeparam>
		/// <param name="inherit"></param>
		/// <returns></returns>
		public static IEnumerable<Type> GetTypesWith<TAttribute>(bool inherit)
			where TAttribute : System.Attribute
		{
			return from assem in System.AppDomain.CurrentDomain.GetAssemblies()
				   from type in assem.GetTypes()
				   where type.IsDefined(typeof(TAttribute), inherit)
				   select type;
		}
	}
}
#endif
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Array = System.Array;

namespace Kit
{
	public class ReorderableListTemplate : System.IDisposable, IList<SerializedProperty>
	{
		#region variable
		protected SerializedObject serializedObject;
		protected SerializedProperty property;
		protected string propertyName;

		List<float> elementHeights;
		protected ReorderableList orderList;
		int selectedIndex;

		Texture2D backgroundImage;
		public event System.Action OnChange;
		#endregion

		#region System
		/// <summary>Overrider this method for child class.</summary>
		/// <example>
		/// public Your_Class_Here(SerializedObject serializedObject, string propertyName, bool dragable = true, bool displayHeader = true, bool displayAddButton = true, bool displayRemoveButton = true)
		///		: base(serializedObject, propertyName, dragable, displayHeader, displayAddButton, displayRemoveButton) { }
		/// </example>
		/// <param name="serializedObject"></param>
		/// <param name="propertyName"></param>
		/// <param name="dragable"></param>
		/// <param name="displayHeader"></param>
		/// <param name="displayAddButton"></param>
		/// <param name="displayRemoveButton"></param>
		public ReorderableListTemplate(SerializedObject serializedObject, string propertyName,
			bool dragable = true, bool displayHeader = true, bool displayAddButton = true, bool displayRemoveButton = true)
		{
			this.propertyName = propertyName;
			this.serializedObject = serializedObject;
			this.property = serializedObject.FindProperty(this.propertyName);
			Init(dragable, displayHeader, displayAddButton, displayRemoveButton);
		}

		public ReorderableListTemplate(SerializedObject serializedObject, SerializedProperty property,
			bool dragable = true, bool displayHeader = true, bool displayAddButton = true, bool displayRemoveButton = true)
		{
			this.serializedObject = serializedObject;
			this.property = property;
			this.propertyName = property.name;
			Init(dragable, displayHeader, displayAddButton, displayRemoveButton);
		}

		protected virtual void Init(bool dragable = true, bool displayHeader = true, bool displayAddButton = true, bool displayRemoveButton = true)
		{
			elementHeights = new List<float>(property.arraySize);
			SetHightLightBackgroundImage();

			orderList = new ReorderableList(serializedObject, property, dragable, displayHeader, displayAddButton, displayRemoveButton);
			orderList.onAddCallback += _OnAdd;
			orderList.onSelectCallback += _OnSelect;
			orderList.onRemoveCallback += _OnRemove;
			orderList.onReorderCallback += _OnReorder;
			orderList.drawHeaderCallback += _OnDrawHeader;
			orderList.drawElementCallback += _OnDrawElement;
			orderList.drawElementBackgroundCallback += _OnDrawElementBackground;
			orderList.elementHeightCallback += _OnCalculateItemHeight;
			orderList.onChangedCallback += _OnChangedCallback;
		}
		#endregion

		#region API
		public virtual void SetHightLightBackgroundImage()
		{
			backgroundImage = new Texture2D(2, 1);
			backgroundImage.SetPixel(0, 0, new Color(.6f, .6f, .6f));
			backgroundImage.SetPixel(1, 0, new Color(.9f, .9f, .9f));
			backgroundImage.hideFlags = HideFlags.DontSave;
			backgroundImage.wrapMode = TextureWrapMode.Clamp;
			backgroundImage.Apply();
		}

		public void DoLayoutList()
		{
			orderList.DoLayoutList();
		}

		public void DoList(Rect rect)
		{
			orderList.DoList(rect);
		}

		public void ApplyModifiedProperties()
		{
			serializedObject.ApplyModifiedProperties();
		}

		public SerializedProperty GetProperty()
		{
			return orderList.serializedProperty;
		}

		public int Count()
		{
			return GetProperty().arraySize;
		}

		public SerializedProperty this[int index]
		{
			get { return orderList.serializedProperty.GetArrayElementAtIndex(index); }
			set { throw new System.NotImplementedException("Read-only."); }
		}

		public SerializedProperty[] GetPropertyArray()
		{
			int cnt = orderList.serializedProperty.arraySize;
			List<SerializedProperty> tmp = new List<SerializedProperty>(cnt);
			for (int i = 0; i < cnt; i++)
			{
				tmp.Add(this[i]);
			}
			return tmp.ToArray();
		}

		public IEnumerator<SerializedProperty> GetEnumerator()
		{
			for (int i = 0; i < orderList.serializedProperty.arraySize; i++)
				yield return orderList.serializedProperty.GetArrayElementAtIndex(i);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		// Not implemented
		public int IndexOf(SerializedProperty item) { throw new System.NotImplementedException(); }
		public void Insert(int index, SerializedProperty item) { throw new System.NotImplementedException(); }
		public void RemoveAt(int index) { throw new System.NotImplementedException(); }
		public void Clear() { throw new System.NotImplementedException(); }
		public bool IsReadOnly { get { throw new System.NotImplementedException(); } }
		public void CopyTo(SerializedProperty[] array, int arrayIndex) { throw new System.NotImplementedException(); }

		int ICollection<SerializedProperty>.Count { get { return GetProperty().arraySize; } }
		bool ICollection<SerializedProperty>.Remove(SerializedProperty item) { throw new System.NotImplementedException(); }
		public void Add(SerializedProperty item) { throw new System.NotImplementedException(); }
		public bool Contains(SerializedProperty item)
		{
			for (int i = 0; i < this.Count(); i++)
			{
				if (this[i] == item)
					return true;
			}
			return false;
		}
		#endregion

		#region listener
		protected virtual void _OnDrawHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, property.displayName);
		}

		private void _OnAdd(ReorderableList list)
		{
			OnBeforeAdd(list);
		}

		private void _OnRemove(ReorderableList list)
		{
			SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.index);
			OnRemove(list, element);
			if (OnChange != null)
				OnChange();
		}

		private void _OnReorder(ReorderableList list)
		{
			OnReorder(list, selectedIndex, list.index);
			selectedIndex = list.index;
			if (OnChange != null)
				OnChange();
		}

		private void _OnSelect(ReorderableList list)
		{
			selectedIndex = list.index;
			SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.index);
			OnSelect(list, element);
		}

		private void _OnChangedCallback(ReorderableList list)
		{
			if (list.index >= 0)
			{
				SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.index);
				OnChangedCallback(list, element);
			}
			else
			{
				OnChangedCallback(list, null);
			}
			if (OnChange != null)
				OnChange();
		}

		private void _OnDrawElement(Rect rect, int index, bool active, bool focused)
		{
			if (property != null && index >= 0 && index < property.arraySize)
			{
				SerializedProperty element = property.GetArrayElementAtIndex(index);
				if (element == null) throw new System.NullReferenceException();
				const float leftMargine = 20f;
				rect = new Rect(rect.x + leftMargine, rect.y, rect.width - leftMargine, EditorGUI.GetPropertyHeight(element) + EditorGUIUtility.standardVerticalSpacing);
				float heightCache = rect.height; // in case override by OnDrawElement

				int orgIndentLevel = EditorGUI.indentLevel;
				OnDrawElement(rect, index, active, focused, element, ref heightCache); // Draw here.
				EditorGUI.indentLevel = orgIndentLevel;

				RenewElementHeight(index, heightCache);
			}
		}
		private void _OnDrawElementBackground(Rect rect, int index, bool active, bool focused)
		{
			if (active && backgroundImage != null &&
				property != null && index >= 0 && index < property.arraySize)
			{
				SerializedProperty element = property.GetArrayElementAtIndex(index);
				if (element == null) throw new System.NullReferenceException();
				rect = new Rect(rect.x + 2, rect.y, rect.width - 4, elementHeights[index]);
				OnDrawElementBackground(rect, index, active, focused, element);
			}
		}
		#endregion

		#region Template
		protected virtual void OnBeforeAdd(ReorderableList list)
		{
			int index = list.serializedProperty.arraySize;
			list.serializedProperty.arraySize++;
			list.index = index;
			SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
			OnAdd(list, element);
		}
		protected virtual void OnAdd(ReorderableList list, SerializedProperty newElement) { }
		protected virtual void OnSelect(ReorderableList list, SerializedProperty selectedElement) { }
		protected virtual void OnRemove(ReorderableList list, SerializedProperty deleteElement)
		{
			if (EditorUtility.DisplayDialog(
				"Warning !",
				"Are you sure you want to delete:\n\r[ " + deleteElement.displayName + " ] ?",
				"Yes", "No"))
			{
				ReorderableList.defaultBehaviours.DoRemoveButton(list);
			}
		}
		protected virtual void OnReorder(ReorderableList list, int fromIndex, int toIndex) { }
		protected virtual void OnDrawElement(Rect rect, int index, bool active, bool focused, SerializedProperty element, ref float overriderHeight)
		{
			EditorGUI.PropertyField(rect, element, true);
		}
		protected virtual void OnDrawElementBackground(Rect rect, int index, bool active, bool focused, SerializedProperty element)
		{
			EditorGUI.DrawTextureTransparent(rect, backgroundImage, ScaleMode.ScaleAndCrop);
		}
		protected virtual void OnChangedCallback(ReorderableList list, SerializedProperty element)
		{
		}
		#endregion

		#region height hotfix
		private void RenewElementHeight(int index, float height)
		{
			try
			{
				elementHeights[index] = height;
			}
			catch
			{
			}
			finally
			{
				ElementListOverflowFix();
			}
		}
		private float _OnCalculateItemHeight(int index)
		{
			float height = 0f;
			try
			{
				if (height != elementHeights[index])
				{
					height = elementHeights[index];
					EditorUtility.SetDirty(serializedObject.targetObject);
				}
			}
			catch
			{
			}
			finally
			{
				ElementListOverflowFix();
				if (OnChange != null)
					OnChange();
			}
			return height;
		}
		private void ElementListOverflowFix()
		{
			if (property.arraySize != elementHeights.Count)
			{
				float[] floats = elementHeights.ToArray();
				Array.Resize(ref floats, property.arraySize);
				elementHeights = floats.ToList();
				EditorUtility.SetDirty(serializedObject.targetObject);
			}
		}
		#endregion

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					orderList.onAddCallback -= _OnAdd;
					orderList.onSelectCallback -= _OnSelect;
					orderList.onRemoveCallback -= _OnRemove;
					orderList.drawHeaderCallback -= _OnDrawHeader;
					orderList.drawElementCallback -= _OnDrawElement;
					orderList.drawElementBackgroundCallback -= _OnDrawElementBackground;
					orderList.elementHeightCallback -= _OnCalculateItemHeight;
					backgroundImage = null;
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~ReorderableListExtend() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}


		#endregion
	}

	public static class ReorderableListUtility
	{
		/// <summary>keep property's parameters only one can be selected, specially for boolean toggle.</summary>
		/// <param name="list">ReorderableListTemplate</param>
		/// <param name="relativeName">FindPropertyRelative(string)</param>
		/// <param name="selectedIndex">cached selected index</param>
		public static void MaintainUniqueBoolen(ReorderableListTemplate list, string relativeName, ref int selectedIndex)
		{
			for (int index = 0; index < list.Count(); index++)
			{
				SerializedProperty autoOpenProp = list[index].FindPropertyRelative(relativeName);
				if (autoOpenProp.boolValue && selectedIndex != index)
				{
					selectedIndex = index;

					for (int i = 0; i < list.Count(); i++)
					{
						list[i].FindPropertyRelative(relativeName).boolValue = (i == selectedIndex);
					}
					list.GetProperty().serializedObject.ApplyModifiedProperties();
					break;
				}
			}
		}
	}
}
#endif
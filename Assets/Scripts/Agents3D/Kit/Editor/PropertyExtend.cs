#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace Kit
{
	/// <summary>For Property Drawer</summary>
	/// <see cref="https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBaseEditor/EditorHelper.cs"/>
	public static class PropertyExtend
	{
		/// <summary>Get current object of provided SerializedProperty</summary>
		/// <param name="prop"><see cref="SerializedProperty"/></param>
		/// <returns>system object</returns>
		public static object GetCurrent(this SerializedProperty prop)
		{
			int index;
			return GetCurrent(prop, out index);
		}

		/// <summary>Get current object of provided SerializedProperty</summary>
		/// <param name="prop"><see cref="SerializedProperty"/></param>
		/// <param name="index">current object index in property path.</param>
		/// <returns>system object</returns>
		public static object GetCurrent(this SerializedProperty prop, out int index)
		{
			string lastName;
			return GetObjectLevel(prop, 0, out index, out lastName);
		}

		/// <summary>Get SerializedProperty's parent object</summary>
		/// <param name="prop"><see cref="SerializedProperty"/></param>
		/// <returns>system object</returns>
		public static object GetParent(this SerializedProperty prop)
		{
			int i;
			string n;
			return GetObjectLevel(prop, 1, out i, out n);
		}

		/// <summary>Performance Warning !! Get target type, based on SerializedProperty, based on reflection.</summary>
		/// <typeparam name="T">system type we need.</typeparam>
		/// <param name="prop"><see cref="SerializedProperty"/></param>
		/// <returns>instance of giving type, or null</returns>
		/// <example>
		/// Path = ".Array.data[0].MyEnum"
		/// called GetBaseProperty<MyEnum>(); will return the enum.
		/// However wrong type will return null.
		/// </example>
		public static T GetBaseProperty<T>(this SerializedProperty prop)
		{
			string[] separatedPaths = prop.propertyPath.Split('.');
			// Go down to the root of this serialized property
			System.Object reflectionTarget = prop.serializedObject.targetObject as object;
			System.Type type = reflectionTarget.GetType();
			// Walk down the path to get the target object
			for (int i=0; i<separatedPaths.Length; i++)
			{
				reflectionTarget = type.GetField(separatedPaths[i]).GetValue(reflectionTarget);
			}
			return (T)reflectionTarget;
		}

		/// <summary>To identify property is one of the array child, even it's grandchildren.</summary>
		/// <param name="prop"><see cref="SerializedProperty"/></param>
		/// <returns>true, when it's a child of something.</returns>
		public static bool IsArrayChild(this SerializedProperty prop)
		{
			return prop.propertyPath.IndexOf(".Array.data[") >= 0;
		}

		/// <summary>Try to get the last array's variable name from propertyPath.</summary>
		/// <param name="prop"><see cref="SerializedProperty"/></param>
		/// <returns>string or null</returns>
		public static string TryGetLastArrayElementName(this SerializedProperty prop)
		{
			string path = prop.propertyPath.Replace(".Array.data[", ".");
			path = path.Replace("]", ".");
			string[] arr = path.Split(new char[] { '.' });
			int pt = arr.Length, tmp;
			while(pt--> 0)
			{
				if(!string.IsNullOrEmpty(arr[pt]) && !int.TryParse(arr[pt], out tmp))
				{
					return arr[pt];
				}
			}
			return null;
		}

		/// <summary>Get last array index based on propertyPath.</summary>
		/// <param name="prop"><see cref="SerializedProperty"/></param>
		/// <returns>-1, or index</returns>
		public static int LastArrayElementIndex(this SerializedProperty prop)
		{
			int
				start = prop.propertyPath.IndexOf("["),
				end = prop.propertyPath.IndexOf("]");

			if (start < 0)
				return -1;

			start++; // not include first letter

			string numStr = prop.propertyPath.Substring(start, end - start);
			return int.Parse(numStr);
		}

		/// <summary>Performance Warning.</summary>
		/// <param name="prop"></param>
		/// <param name="level"></param>
		/// <param name="lastIndex"></param>
		/// <param name="lastName"></param>
		/// <returns></returns>
		private static object GetObjectLevel(SerializedProperty prop, int level, out int lastIndex, out string lastName)
		{
			string path = prop.propertyPath.Replace(".Array.data[", "[");
			object obj = prop.serializedObject.targetObject;
			string[] elements = path.Split('.');
			lastName = null;
			lastIndex = -1;
			foreach (string element in elements.Take(elements.Length - level))
			{
				if (element.Contains("["))
				{
					string elementName = element.Substring(0, element.IndexOf("["));
					int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
					obj = GetArrayValue(obj, elementName, index);
					lastIndex = index;
					lastName = elementName;
				}
				else
				{
					obj = GetValue(obj, element);
					lastName = element;
					lastIndex = -1;
				}
			}
			return obj;
		}

		/// <summary>Try Get value based on giving name. try get Field, and then try get property.</summary>
		/// <param name="source"></param>
		/// <param name="name">name of field / name of property</param>
		/// <returns>return object / null</returns>
		public static object GetValue(object source, string name)
		{
			if (source == null)
				return null;
			Type type = source.GetType();
			FieldInfo fieldInfo = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			if (fieldInfo == null)
			{
				PropertyInfo propInfo = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (propInfo == null)
					return null;
				return propInfo.GetValue(source, null);
			}
			return fieldInfo.GetValue(source);
		}

		/// <summary>Try Get value based on giving name and index, used in array</summary>
		/// <param name="source"></param>
		/// <param name="name">array name</param>
		/// <param name="index">level index</param>
		/// <returns></returns>
		private static object GetArrayValue(object source, string name, int index)
		{
			IEnumerable enumerable = GetValue(source, name) as IEnumerable;
			IEnumerator enm = enumerable.GetEnumerator();
			while (index-- >= 0)
				enm.MoveNext();
			try
			{
				return enm.Current;
			}
			catch
			{
				/* Error fix
				InvalidOperationException: Operation is not valid due to the current state of the object
				error operation : add array element and GetValue() before construct.
				*/
				return null;
			}
		}
	}
}
#endif
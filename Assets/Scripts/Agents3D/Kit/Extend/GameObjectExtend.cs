using UnityEngine;
#if UNITY_EDITOR
using PrefabUtility = UnityEditor.PrefabUtility;
using PrefabType = UnityEditor.PrefabType;
using EditorUtility = UnityEditor.EditorUtility;
#endif

namespace Kit
{
	public static class GameObjectExtend
	{
		public static bool IsPrefabInstance(this GameObject self)
		{
#if UNITY_EDITOR
			return self != null && PrefabUtility.GetPrefabType(self) == PrefabType.PrefabInstance;
#else
			return IsPrefab(self); // fallback
#endif
		}

		public static bool IsPrefabOriginal(this GameObject self)
		{
#if UNITY_EDITOR
			return self != null && PrefabUtility.GetPrefabType(self) == PrefabType.Prefab;
#else
			return IsPrefab(self); // fallback
#endif
		}

		public static bool IsPrefabDisconnected(this GameObject self)
		{
#if UNITY_EDITOR
			return self != null && PrefabUtility.GetPrefabType(self) == PrefabType.DisconnectedPrefabInstance;
#else
			return IsPrefab(self); // fallback
#endif
		}

		/// <summary>Check target is prefab in play mode</summary>
		/// <see cref="http://forum.unity3d.com/threads/i-found-the-solution-for-checking-if-a-gameobject-is-prefab-ghost-or-not.272958/"/>
		/// <param name="self"></param>
		/// <returns></returns>
		private static bool IsPrefab(GameObject self)
		{
#if UNITY_EDITOR
			return EditorUtility.IsPersistent(self);
#else
			return self.scene.rootCount == 0;
			// return self.scene.buildIndex < 0;
#endif
		}

		/// <summary>Recursive allocate target GameObject in hierarchy ,depend on giving GameObject</summary>
		/// <remarks><see cref="http://forum.unity3d.com/threads/transform-find-doesnt-work.12949/"/></remarks>
		/// <param name="self"></param>
		/// <param name="name"></param>
		/// <returns>Null or GameObject</returns>
		public static GameObject FindInChildren(this GameObject self, string name)
		{
			Transform transform = self.transform;
			Transform child = transform.FindInChildren(name);
			return child != null ? child.gameObject : null;
		}

		/// <summary>Wrapper for Application & EditorApplication</summary>
		/// <param name="self"></param>
		/// <returns></returns>
		/// <remarks>OnValidate will called when developer hit play mode button. even it's in project & without modifiy</remarks>
		public static bool IsPlayingOrWillChangePlaymode(this GameObject self)
		{
#if UNITY_EDITOR
			return UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#else
			return Application.isPlaying;
#endif
		}
	}
}
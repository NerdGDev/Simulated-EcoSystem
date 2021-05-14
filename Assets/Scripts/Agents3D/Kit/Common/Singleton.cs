//#define SHOW_WARNING
using UnityEngine;

namespace Kit
{
	/// <summary>SingleTon extend methods</summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="WhenDuplicates"><see cref="DoNothing"/>, <see cref="RemoveLateComer"/>, <see cref="RemoveExisting"/></typeparam>
	/// <typeparam name="InstanceBehavior"><see cref="Manually"/>, <see cref="SearchHierarchy"/>, <see cref="AutoCreate"/></typeparam>
	public class Singleton<T, WhenDuplicates, InstanceBehavior> : SingletonBase
		where T : Singleton<T, WhenDuplicates, InstanceBehavior>
		where WhenDuplicates : DuplicateAction, new()
		where InstanceBehavior : InstanceBehaviorAction, new()
	{
		private static T _instance = null;
		
		/// <summary>When instance is null, depend on the instance behavior template this will TRY to return singleton instance.
		/// <see cref="Singleton{T, WhenDuplicates, InstanceBehavior}"/>, <seealso cref="InstanceBehavior"/></summary>
		/// <remarks>bad perfromance while instance is null</remarks>
		public static T GetInstance()
		{
			if (_instance == null)
			{
				if ((new InstanceBehavior()).Action == (new SearchHierarchy()).Action)
				{
					T searchObject = FindObjectOfType<T>();
					if (searchObject == null)
						new GameObject(typeof(T).Name + " (singleton)", typeof(T));
				}
				else if ((new InstanceBehavior()).Action == (new Manually()).Action)
				{
					throw new System.NullReferenceException(typeof(T).Name + " : Singleton without instance.");
				}
				else if ((new InstanceBehavior()).Action == (new AutoCreate()).Action)
				{
					new GameObject(typeof(T).Name + " (singleton)", typeof(T));
				}
			}
			return _instance;
		}

		/// <summary>Standard normal instance, without any magic feature, only return instance, even it's null.</summary>
		public static T Instance { get { return _instance; } }
		protected virtual void Awake()
		{
			if (_instance != null)
			{
				if (_instance.GetInstanceID() != this.GetInstanceID())
				{
					// when duplicate instance detected
					if ((new WhenDuplicates()).Action == (new RemoveLateComer()).Action)
					{
#if SHOW_WARNING
						Debug.LogWarning("Destroying late singleton: "+this, this);
#endif
						enabled = false;
						Destroy(gameObject);
					}
					else if ((new WhenDuplicates()).Action == (new RemoveExisting()).Action)
					{
#if SHOW_WARNING
						Debug.LogWarning("Destroying existing singleton: "+_instance, this);
#endif
						_instance.enabled = false;
						Destroy(_instance.gameObject);
						_instance = (T)this;
					}
				}
			}
			else
			{
				_instance = (T)this;
			}
		}
		protected virtual void OnDestroy()
		{
			// unless the instance refers to a different object, set to null.
			// (NOTE: checking if (_instance == this) doesn't work, since Unity
			// will play tricks with destroyed objects.)
			if (_instance == null || _instance == this)
			{
#if SHOW_WARNING
				Debug.LogWarning("Destroying singleton: "+this, this);
#endif
				_instance = null;
			}
		}
	}

	/// <summary>SingleTon extend methods</summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="WhenDuplicates"><see cref="DoNothing"/>, <see cref="RemoveLateComer"/>, <see cref="RemoveExisting"/></typeparam>
	public class Singleton<T, WhenDuplicates> : Singleton<T, WhenDuplicates, Manually>
		where T : Singleton<T, WhenDuplicates>
		where WhenDuplicates : DuplicateAction, new()
	{ }

	/// <summary>SingleTon extend methods</summary>
	/// <typeparam name="T"></typeparam>
	public class Singleton<T> : Singleton<T, DoNothing, Manually> where T : Singleton<T>
	{ }
	
	/// <summary>Helper class, so singletons can be found with GetComponent<Singleton<T>>, without knowing their specific type.</summary>
	public abstract class SingletonBase : MonoBehaviour
	{ }

	public abstract class DuplicateAction { abstract public int Action { get; } }
	public class DoNothing : DuplicateAction { public override int Action { get { return 1; } } }
	public class RemoveLateComer : DuplicateAction { public override int Action { get { return 2; } } }
	public class RemoveExisting : DuplicateAction { public override int Action { get { return 3; } } }

	public abstract class InstanceBehaviorAction { abstract public int Action { get; } }
	public class Manually : InstanceBehaviorAction { public override int Action { get { return 1; } } }
	public class SearchHierarchy : InstanceBehaviorAction { public override int Action { get { return 2; } } }
	public class AutoCreate : InstanceBehaviorAction { public override int Action { get { return 3; } } }
}
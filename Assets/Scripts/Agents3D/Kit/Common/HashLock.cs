using System.Collections.Generic;
using UnityEngine;
using Action = System.Action;

namespace Kit
{
	public class HashLock<T>
	{
		private bool m_Strict;
		public HashLock(bool strict)
		{
			m_Strict = strict;
		}
		private HashSet<T> m_LockOwners = new HashSet<T>();
		public event Action Locked, Released;

		public bool IsLocked { get { return m_LockOwners.Count > 0; } }
		public bool IsLockedBy(T obj)
		{
			return m_LockOwners.Contains(obj);
		}
		public int LockedCount { get { return m_LockOwners.Count; } }
		public void AquireLock(T caller)
		{
			if (!m_LockOwners.Contains(caller))
			{
				m_LockOwners.Add(caller);
				if (Locked != null && m_LockOwners.Count == 1)
					Locked();
			}
			else if (m_Strict)
				throw new System.InvalidOperationException("requesting double lock from " + caller);
			else
				Debug.Log("requesting double lock from " + caller);
		}

		public void ReleaseLock(T caller)
		{
			if (m_LockOwners.Contains(caller))
			{
				m_LockOwners.Remove(caller);
				if (Released != null && m_LockOwners.Count == 0)
					Released();
			}
			else if (m_Strict)
				throw new System.InvalidOperationException("trying to release the non-exist lock from " + caller);
			else
				Debug.Log("trying to release the non-exist lock from " + caller);
		}

		public override string ToString()
		{
			return "[" + GetType().Name + " :" + (IsLocked ? "Locked" : "Unlock") + ", count = " + m_LockOwners.Count + "]";
		}

		public string ToString(bool detail)
		{
			if (detail)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.AppendFormat("{0}\n\n\r", ToString());
				foreach(T owner in m_LockOwners)
				{
					sb.AppendFormat("- {0}\n", owner.ToString());
				}
				sb.Append("\n\n");
				return sb.ToString();
			}
			return ToString();
		}
	}
}
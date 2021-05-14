using UnityEngine;

namespace Kit
{
	/// <summary>
	/// "Semaphore" lock
	/// <see cref="https://en.wikipedia.org/wiki/Semaphore_(programming)"/>
	/// </summary>
	public class IntLock
	{
		private bool m_Locked = false;
		private bool m_Strict;
		private System.Func<int> GetLockValue = null;
		private System.Action<int> SetLockValue = null;

		/// <summary>handle integer lock</summary>
		/// <param name="getLockDelegate"></param>
		/// <param name="setLockDelegate"></param>
		/// <param name="strict"></param>
		/// <example>
		/// int m_LockCount = 0;
		/// var locker = new IntLock(() => m_LockCount, (value) => m_LockCount = value);
		/// and define your own instance on each class, but share the same integer reference (delegate),
		/// therefore each lock can only be lock/release by itself, but remain the counter on source correctly.
		/// </example>
		public IntLock(System.Func<int> getLockDelegate, System.Action<int> setLockDelegate, bool strict = true)
		{
			GetLockValue = getLockDelegate;
			SetLockValue = setLockDelegate;
			m_Strict = strict;
		}

		public void AcquireLock()
		{
			if (m_Strict && m_Locked)
			{
				Debug.LogError("Trying to acquire lock object that's already locked(in strict mode), logic errors?");
			}
			else
			{
				SetLockValue(GetLockValue() + 1);
				m_Locked = true;
			}
		}

		public void ReleaseLock()
		{
			if (m_Strict && !m_Locked)
			{
				Debug.LogError("Trying to release lock object that's already unlocked(in strict mode), logic errors?");
			}
			else
			{
				// do whatever you want, just don't less that zero.
				SetLockValue(Mathf.Max(0, GetLockValue() - 1));
				m_Locked = false;
			}
		}
	}
}
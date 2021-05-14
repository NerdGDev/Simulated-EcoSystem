using UnityEngine;
using System.Collections.Generic;

namespace Kit
{
	public class ExpiringLock
	{
		/// <summary>The unlock time for this lock</summary>
		private float m_EndTime;
		
		/// <summary>To identify this lock are ignore timeScale or not</summary>
		private bool m_IgnoreTimeScale;

		/// <summary>To construct the expiring lock</summary>
		/// <param name="duration"></param>
		public ExpiringLock(float duration, bool ignoreTimeScale = false)
		{
			m_IgnoreTimeScale = ignoreTimeScale;
			m_EndTime = m_IgnoreTimeScale ? Time.realtimeSinceStartup : Time.timeSinceLevelLoad;
			m_EndTime += duration;
		}

		/// <summary>To check if this lock are expired</summary>
		/// <returns></returns>
		public bool IsExpired()
		{
			return m_IgnoreTimeScale ?
				Time.realtimeSinceStartup > m_EndTime :
				Time.timeSinceLevelLoad > m_EndTime;
		}

		/// <summary>
		/// Expire this instance, to avoid accidentally reused with its old value.
		/// Therefore <see cref="CreateWhenLater(ExpiringLock, float)"/> can not keep the old value anymore
		/// </summary>
		public void ForceExpire()
		{
			m_EndTime = 0f;
		}

		/// <summary>Create or reuse the expirable lock, depend on which one will expire later.</summary>
		/// <param name="priorLock">reference expiringlock.</param>
		/// <param name="duration">start from now, to define when to expire the lock.</param>
		/// <returns></returns>
		public static ExpiringLock CreateWhenLater(ExpiringLock priorLock, float duration)
		{
			if (priorLock == null || Time.timeSinceLevelLoad + duration > priorLock.m_EndTime)
				return new ExpiringLock(duration);
			else
				return priorLock;
		}

		/// <summary>To check if all ExpiringLock are expired.</summary>
		/// <param name="locks"></param>
		/// <returns>true = all expired, fail when one of them are locked.</returns>
		public static bool IsAllExpired(ICollection<ExpiringLock> locks)
		{
			float timeScaleNow = Time.timeSinceLevelLoad;
			float ignoreScaleNow = Time.realtimeSinceStartup;
			foreach (ExpiringLock _lock in locks)
			{
				/// faster for not using <see cref="IsExpired"/> check.
				if (_lock.m_IgnoreTimeScale)
				{
					if (timeScaleNow <= _lock.m_EndTime)
						return false;
				}
				else
				{
					if (ignoreScaleNow <= _lock.m_EndTime)
						return false;
				}
			}
			return true;
		}
	}
}
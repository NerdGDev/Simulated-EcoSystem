using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace FlyAgent.Navigation
{
	public abstract class ObstacleBase :
		System.IEquatable<GameObject>,
		System.IEquatable<Transform>,
		System.IEquatable<Collider>,
		System.IEquatable<ObstacleBase>
	{
		public readonly GameObject gameObject;
		public readonly Transform transform;
		public readonly Collider collider;
		public Bounds bounds { get { return IsValid? collider.bounds : Error; } }

		public bool IsValid { get { return collider != null || !collider.isTrigger; } }

		private static readonly Bounds Error = new Bounds(new Vector3(0f, -65535f, 0f), Vector3.zero);

		public ObstacleBase(Collider collider)
		{
			this.collider = collider;
			this.gameObject = collider.gameObject;
			this.transform = collider.transform;
			transform.hasChanged = false;
		}

		public bool Equals(Transform other) { return transform == other; }
		public bool Equals(GameObject other) { return Equals(other.transform); }
		public bool Equals(Collider other) { return Equals(other.transform); }
		public bool Equals(ObstacleBase other) { return Equals(other.transform); }
		
		public override bool Equals(object obj)
		{
			if (obj is ObstacleBase)
				return Equals((ObstacleBase)obj);
			else if (obj is Collider)
				return Equals((Collider)obj);
			else if (obj is Transform)
				return Equals((Transform)obj);
			else if (obj is GameObject)
				return Equals((GameObject)obj);
			else
				return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return collider.GetHashCode();
		}
	}

	public sealed class Obstacle : ObstacleBase
	{
		private const float m_PeriodMin = 0.3f;
		private const float m_PeriodMax = 3f;
		/// <summary> used to record the unchnage period
		/// within m_PeriodMin ~ m_PeriodMax,
		/// when it's too long didn't change the state, we consider it's Inactive
		/// </summary>
		private float m_PeriodicUpdate;
		/// <summary>Next update since level loaded, optimize for performace reason</summary>
		private float m_NextUpdateTime;

		private Vector3 m_LastPosition;
		private Quaternion m_LastRotation;
		private Vector3 m_LastScale;

		public static event System.Action<Obstacle> EVENT_Removed;
		public static event System.Action<Obstacle> EVENT_Updated;
		
		public Obstacle(Collider collider) : base(collider)
		{
			m_PeriodicUpdate = m_PeriodMin;
			m_NextUpdateTime = Time.timeSinceLevelLoad + m_PeriodicUpdate;
			m_LastPosition = transform.position;
			m_LastRotation = transform.rotation;
			m_LastScale = transform.localScale;
		}

		/// <summary>Call to schedule update latest information.</summary>
		/// <param name="timeSinceLevelLoad">Time.timeSinceLevelLoad</param>
		public void StateCheck(float timeSinceLevelLoad)
		{
			if (!IsValid)
			{
				if (EVENT_Removed != null)
					EVENT_Removed(this);
				return;
			}

			if (gameObject.isStatic || timeSinceLevelLoad <= m_NextUpdateTime)
				return;

			Profiler.BeginSample("ObstaclePositionCheck");
			// for optiomize, we trust unity handle the transform well, and the user not manually override this flag.
			bool isChanged = transform.hasChanged;
			if (!transform.hasChanged && m_PeriodicUpdate >= m_PeriodMax)
			{
				// do the slow check by matrix;
				isChanged = m_LastPosition == transform.position && m_LastRotation == transform.rotation && m_LastScale == transform.localScale;
			}

			if (isChanged)
			{
				transform.hasChanged = false; // reset this flag
				m_LastPosition = transform.position;
				m_LastRotation = transform.rotation;
				m_LastScale = transform.localScale;

				// reduce the time, until we reach the m_PeriodMin;
				if (m_PeriodicUpdate > m_PeriodMin)
					m_PeriodicUpdate = Mathf.Clamp(m_PeriodicUpdate * 0.5f, m_PeriodMin, m_PeriodMax);
				if (EVENT_Updated != null)
					EVENT_Updated(this);
			}
			else
			{
				// nothing changed.
				// double the time, until we reach the m_PeriodMax;
				if (m_PeriodicUpdate < m_PeriodMax)
					m_PeriodicUpdate = Mathf.Clamp(m_PeriodicUpdate * 2f, m_PeriodMin, m_PeriodMax);
			}
			m_NextUpdateTime = timeSinceLevelLoad + m_PeriodicUpdate;
			Profiler.EndSample();
		}
	}
}
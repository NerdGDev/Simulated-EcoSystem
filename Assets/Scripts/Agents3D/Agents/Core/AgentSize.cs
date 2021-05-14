using UnityEngine;

namespace FlyAgent.Agents
{
	/// <summary>A data structure to define the agent size,
	/// and cache those information.</summary>
	public struct AgentSize
	{
		public readonly FlyAgent m_Agent;
		private int m_NextSizeUpdateFrameLock;
		private int m_PhysicalPerformanceFrameLock;
		/// <summary>Capsule cast usage</summary>
		private Vector3[] m_LocalCapsule;
		/// <summary>Cached Capsule points in world space.</summary>
		private Vector3[] m_GlobalCapsule;
		private Bounds m_GlobalBounds;

		public AgentSize(FlyAgent agent)
		{
			m_Agent = agent;
			m_PhysicalPerformanceFrameLock = m_NextSizeUpdateFrameLock = 0;
			m_LocalCapsule = new Vector3[4] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
			m_GlobalCapsule = new Vector3[4] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
			m_GlobalBounds = new Bounds();
			CalculateAgentSize();
		}

		/// <summary>To reject the physical raycast request, based on developer setting.
		/// 1~60 frames delay based on <see cref="eQuality"/> setting.</summary>
		/// <returns></returns>
		private int GetFrameCountDelay()
		{
			if (!Application.isPlaying)
				return 0;
			else
			{
				switch (m_Agent.m_Quality)
				{
					case FlyAgent.eQuality.HighQuality:
						return 1;
					case FlyAgent.eQuality.GoodQuality:
						return 10;
					case FlyAgent.eQuality.MediumQuality:
						return 25;
					case FlyAgent.eQuality.LowQuality:
						return 45;
					default:
					case FlyAgent.eQuality.None:
						return 60;
				}
			}
		}

		/// <summary>To all physical can trigger in this frame or not.</summary>
		/// <returns>true = allow to trigger raycast test.</returns>
		public bool AllowPhysical()
		{
			if (m_PhysicalPerformanceFrameLock >= Time.frameCount)
				return false;

			m_PhysicalPerformanceFrameLock = Time.frameCount + GetFrameCountDelay();
			return true;
		}

		/// <summary>Get agents Capsule's points in world space.</summary>
		/// <returns>Cached or recalculate agents size.</returns>
		public Vector3[] GetCapsule(bool isLocal = false)
		{
			if (Time.frameCount >= m_NextSizeUpdateFrameLock)
			{
				m_NextSizeUpdateFrameLock = Time.frameCount + GetFrameCountDelay();
				CalculateAgentSize();
			}
			return isLocal ? m_LocalCapsule : m_GlobalCapsule;
		}

		public Bounds GetBounds(bool isLocal = false)
		{
			GetCapsule(isLocal);
			return m_GlobalBounds;
		}

		/// <summary>Calculate agent size in local or world, based on developer setting.</summary>
		private void CalculateAgentSize()
		{
			FlyAgent.eDirection direction = m_Agent.m_Direction;
			float radius = m_Agent.m_Radius;
			float offset = m_Agent.m_Offset;
			float halfLength = m_Agent.m_Length * 0.5f;

			m_LocalCapsule[0] = m_LocalCapsule[1] = Vector3.zero;
			if (direction == FlyAgent.eDirection.ZAxis)
			{
				m_LocalCapsule[0].z += halfLength + offset;
				m_LocalCapsule[1].z -= halfLength - offset;
				m_LocalCapsule[2].z = m_LocalCapsule[0].z + radius;
				m_LocalCapsule[3].z = m_LocalCapsule[1].z - radius;
			}
			else if (direction == FlyAgent.eDirection.YAxis)
			{
				m_LocalCapsule[0].y += halfLength + offset;
				m_LocalCapsule[1].y -= halfLength - offset;
				m_LocalCapsule[2].y = m_LocalCapsule[0].y + radius;
				m_LocalCapsule[3].y = m_LocalCapsule[1].y - radius;
			}
			else // if (direction == eDirection.XAxis)
			{
				m_LocalCapsule[0].x += halfLength + offset;
				m_LocalCapsule[1].x -= halfLength - offset;
				m_LocalCapsule[2].x = m_LocalCapsule[0].x + radius;
				m_LocalCapsule[3].x = m_LocalCapsule[1].x - radius;
			}
			m_GlobalCapsule[0] = m_Agent.transform.TransformPoint(m_LocalCapsule[0]);
			m_GlobalCapsule[1] = m_Agent.transform.TransformPoint(m_LocalCapsule[1]);
			m_GlobalBounds.center = m_GlobalCapsule[0];
			m_GlobalBounds.extents = Vector3.one * radius;
			m_GlobalBounds.Encapsulate(new Bounds(m_GlobalCapsule[1], m_GlobalBounds.extents));
		}
	}
}

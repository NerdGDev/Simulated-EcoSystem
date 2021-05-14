using UnityEngine;

namespace FlyAgent.Agents
{
	/// <summary>Used to share the common factor across function during each frame.
	/// Optimize for getting position magnitude</summary>
	public class CurrentFrameCache
	{
		public Vector3 velocity { get; private set; }
		public Vector3 velocityNormalize { get; private set; }
		public float velocitySqrMagnitude { get; private set; }
		public float velocityMagnitude { get; private set; }
		public Vector3 position { get; private set; }
		public Vector3 forward { get; private set; }
		public Vector3 lastFramePosition { get; private set; }
		/// <summary>The delta position movement from last frame to current frame.</summary>
		public Vector3 deltaPosition { get; private set; }
		public float deltaPositionSqrMagnitude { get; private set; }
		public float deltaPositionMagnitude { get; private set; }
		public float arrivedDistanceSqr { get; private set; }
		public bool IsGettingSuck { get; private set; } // when stuck in same position within 2 frames.

		private readonly Rigidbody m_Rigidbody;
		private readonly FlyAgent m_Agent;
		private readonly Vehicle m_Pilot;

		public CurrentFrameCache(FlyAgent agent, Vehicle pilot, Rigidbody rigidbody)
		{
			m_Agent = agent;
			m_Pilot = pilot;
			m_Rigidbody = rigidbody;
			deltaPosition = Vector3.zero;
			position = lastFramePosition = m_Rigidbody.position;
			forward = m_Rigidbody.transform.forward;
		}

		public void Update()
		{
			deltaPosition = m_Rigidbody.position - lastFramePosition;
			lastFramePosition = m_Rigidbody.position;
			deltaPositionSqrMagnitude = deltaPosition.sqrMagnitude;
			deltaPositionMagnitude = Mathf.Sqrt(deltaPositionSqrMagnitude);
			arrivedDistanceSqr = m_Agent.m_ArrivedDistance * m_Agent.m_ArrivedDistance;

			IsGettingSuck = m_Agent.HasDestination() && m_Pilot.DriveTime > 0.1f && deltaPositionMagnitude < 0.01f;

			velocity = m_Rigidbody.velocity;
			velocityNormalize = velocity.normalized;
			velocitySqrMagnitude = velocity.sqrMagnitude;
			velocityMagnitude = Mathf.Sqrt(velocitySqrMagnitude);
			position = m_Rigidbody.position;
			forward = m_Rigidbody.transform.forward;
		}
	}
}

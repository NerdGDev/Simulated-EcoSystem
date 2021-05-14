/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kit;

namespace FlyAgent.Agents
{
	public partial class BoidAgent : FlyAgent
	{
		#region Magic numbers
		public const int MAX_TAG_COUNT = 20;
		private static readonly Vector2 TAG_INTERVAL_RANGE = new Vector2(.8f, 3.2f);
		private const float TAG_ANGLE_DIFF_BIAS = .5f; // +/-45 degree diff
		private const float TAG_SPEED_DIFF_BIAS = 10f;
		#endregion

		#region variables
		public static event Callback<BoidAgent> Event_BoidAgentDisable;
		private List<BoidAgent> m_TaggedBoids = new List<BoidAgent>(MAX_TAG_COUNT);
		private float m_LastTagTime = 0f;
		private FlockData m_FlockData;
		#endregion

		#region System
		protected override void OnValidate()
		{
			base.OnValidate();
		}
		
		protected override void OnEnable()
		{
			base.OnEnable();
			Event_BoidAgentDisable += OnBoidAgentDisable;

			// Common cases : spawn agent usually have same position & rotation, delay tag time.
			m_LastTagTime = Time.timeSinceLevelLoad + TAG_INTERVAL_RANGE.y;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			Event_BoidAgentDisable -= OnBoidAgentDisable;

			if (Event_BoidAgentDisable != null)
				Event_BoidAgentDisable(this);
		}
		#endregion

		#region internal
		protected override void OnAgentInit()
		{
			base.OnAgentInit();
			m_FlockData = new FlockData();
			m_SteeringFactors.Add(new Separation(this, m_FlockData));
			m_SteeringFactors.Add(new Alignment(this, m_FlockData));
			m_SteeringFactors.Add(new Cohesion(this, m_FlockData));
			
			if (m_Behaviour == null)
				throw new System.NullReferenceException("Boid missing <" + typeof(BoidsBehaviour).Name + ">!");
		}

		protected override void OnAgentUpdate(float fixedDeltaTime)
		{
			base.OnAgentUpdate(fixedDeltaTime);
			TagNeighboursByDistance();
			if (m_TaggedBoids.Count > 0 && HasDestination())
			{
				Quaternion rotate = Quaternion.Slerp(m_Rigidbody.rotation, m_FlockData.m_AlignFaceDirection, fixedDeltaTime);
				m_Rigidbody.MoveRotation(rotate);
			}
		}

		protected override void AgentReset()
		{
			base.AgentReset();
			m_TaggedBoids.Clear();
		}
		#endregion

		#region Neighbours
		private int TagNeighboursByDistance()
		{
			if (Time.timeSinceLevelLoad > m_LastTagTime)
			{
				// delay for performance issue
				m_LastTagTime = Time.timeSinceLevelLoad + Random.Range(TAG_INTERVAL_RANGE.x, TAG_INTERVAL_RANGE.y);
				m_TaggedBoids.Clear();
				// using transform instead of Raycast check to reduce the physics call.
				float sqrNeighborDistance = ((BoidsBehaviour)m_Behaviour).m_NeighborDistance; sqrNeighborDistance *= sqrNeighborDistance;
				foreach (FlyAgentBase agent in m_ActiveFlightAgent)
				{
					if (agent == this)
						continue;

					BoidAgent boid = (agent is BoidAgent) ? (BoidAgent)agent : null;
					if (boid == null)
						continue;


					Vector3 relatedDirection = boid.m_CurrentFrameCache.position - m_CurrentFrameCache.position;
					float sqrRelatedDistance = relatedDirection.sqrMagnitude;
					// when they are close enough to check.
					if (sqrRelatedDistance < sqrNeighborDistance)
					{
						// quick check to identify are they facing + moving toward to same direction with similar speed.
						if (Vector3.Dot(m_CurrentFrameCache.forward, boid.m_CurrentFrameCache.forward) > TAG_ANGLE_DIFF_BIAS && // check facing angle diff
							Vector3.Dot(m_CurrentFrameCache.velocityNormalize, boid.m_CurrentFrameCache.velocityNormalize) > TAG_ANGLE_DIFF_BIAS && // check velocity angle diff
							Mathf.Abs(m_CurrentFrameCache.velocityMagnitude - boid.m_CurrentFrameCache.velocityMagnitude) < TAG_SPEED_DIFF_BIAS // check movement speed diff
							)
						{
							m_TaggedBoids.Add(boid);
							if (m_TaggedBoids.Count >= MAX_TAG_COUNT)
								break;
						}
					}
				}
			}
			return m_TaggedBoids.Count;
		}
		
		private void OnBoidAgentDisable(BoidAgent boid)
		{
			if (m_TaggedBoids.Contains(boid))
				m_TaggedBoids.Remove(boid);
		}
		#endregion
	}
}
*/
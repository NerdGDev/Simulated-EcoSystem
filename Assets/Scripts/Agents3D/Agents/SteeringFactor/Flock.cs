/*
using UnityEngine;
using Kit;
using FlyAgent.Utilities;

namespace FlyAgent.Agents
{
	[System.Serializable]
	public class FlockDebug
	{
		public Color m_SeparationColor = Color.magenta;
		public Color m_AlignmentColor = Color.yellow;
		public Color m_CohesionColor = Color.blue;
		public Color m_CentreOfMassColor = new Color(0f, 1f, .3f, 0.25f);
		public Color m_TaggedNeighboursColor = new Color(.3f, .3f, .3f, 0.5f);
	}

	public partial class BoidAgent : FlyAgent
	{
		/// <summary>
		/// Collapse all 3 basic behaviour in to one function call,
		/// and used frame lock to guarantee 1 call per frame.
		/// </summary>
		public class FlockData
		{
			public WeightVector m_SeparationForce;
			public WeightVector m_AlignmentForce;
			public Quaternion m_AlignFaceDirection;
			public WeightVector m_CohesionForce;

			/// <summary>Frame lock</summary>
			public int m_FrameCount;

			/// <summary>Frame lock</summary>
			public int m_GizmosCount;

			/// <summary>The centroid of the flock</summary>
			public Vector3 m_CenterOfMass;
		}

		private abstract class Flock : ISteeringFactor
		{
			protected readonly BoidAgent m_Agent;
			protected readonly FlockData m_ShareData;
			public Flock(BoidAgent _agent, FlockData _data) { m_Agent = _agent; m_ShareData = _data; }

			public void DrawGizmos()
			{
				if (m_ShareData.m_GizmosCount == Time.frameCount)
					return;
				m_ShareData.m_GizmosCount = Time.frameCount;

				if (m_Agent.m_Behaviour == null ||
					!(m_Agent.m_Behaviour is BoidsBehaviour))
					return;

				FlockDebug debug = ((BoidsBehaviour)m_Agent.m_Behaviour).m_Flock;
				Vector3 pos = m_Agent.m_CurrentFrameCache.position;
				GizmosExtend.DrawLine(pos, pos + m_ShareData.m_SeparationForce.vector, debug.m_SeparationColor);
				GizmosExtend.DrawLine(pos, pos + m_ShareData.m_AlignmentForce.vector, debug.m_AlignmentColor);
				GizmosExtend.DrawLine(pos, pos + m_ShareData.m_CohesionForce.vector, debug.m_CohesionColor);
				GizmosExtend.DrawSphere(m_ShareData.m_CenterOfMass, .3f, debug.m_CentreOfMassColor);
				foreach (BoidAgent agent in m_Agent.m_TaggedBoids)
				{
					GizmosExtend.DrawLine(pos, agent.transform.position, debug.m_TaggedNeighboursColor);
				}
			}

			public abstract WeightVector GetWeightVector();
			protected void CalculateFlockFactor()
			{
				if (m_ShareData.m_FrameCount == Time.frameCount)
					return;
				m_ShareData.m_FrameCount = Time.frameCount;

				Vector3 _separation = Vector3Zero;
				Vector3 _alignment = Vector3Zero;
				Quaternion _alignmentRotation = m_Agent.m_Rigidbody.rotation;
				Vector3 _cohesion = Vector3Zero;
				Vector3 _centreOfMass = Vector3Zero;
				BoidsBehaviour boidsBehaviour = (BoidsBehaviour)m_Agent.m_Behaviour;
				int taggedCount = 0;
				// combine 3 steering factor at once,
				// since they both required to loop via all m_TaggedBoids.
				int cnt = m_Agent.m_TaggedBoids.Count;
				for (int i = 0; i < cnt; i++)
				{
					BoidAgent boid = m_Agent.m_TaggedBoids[i];
					// Separation part 1
					{
						Vector3 reverseDirection = m_Agent.transform.position - boid.transform.position;
						_separation += reverseDirection.normalized / reverseDirection.magnitude;
					}

					if (boid == m_Agent)
						continue;

					taggedCount++;
					// Alignment part 1
					_alignment += boid.transform.forward;
					_alignmentRotation = Quaternion.Lerp(_alignmentRotation, boid.transform.rotation, .5f);

					// Cohesion part 1
					_centreOfMass += boid.transform.position;
				}

				// Separation part 2
				m_ShareData.m_SeparationForce = new WeightVector(_separation, boidsBehaviour.m_SeparationWeight);

				// Alignment part 2
				if (taggedCount > 0)
				{
					_alignment /= (float)taggedCount;
					_alignment -= m_Agent.transform.forward;
				}
				m_ShareData.m_AlignmentForce = new WeightVector(_alignment, boidsBehaviour.m_AlignmentWeight);
				m_ShareData.m_AlignFaceDirection = Quaternion.Lerp(m_Agent.m_Rigidbody.rotation, _alignmentRotation, boidsBehaviour.m_AlignmentWeight);

				// Cohesion part 2
				if (taggedCount > 0)
				{
					_centreOfMass /= (float)taggedCount;
					if (_centreOfMass.sqrMagnitude > 0f)
					{
						_cohesion = _centreOfMass - m_Agent.transform.position;
						if (_cohesion.sqrMagnitude < m_Agent.m_Radius * m_Agent.m_Radius)
						{
							// Nope we are too close.
							_cohesion = Vector3Zero;
						}
					}
				}
				m_ShareData.m_CenterOfMass = _centreOfMass;
				m_ShareData.m_CohesionForce = new WeightVector(_cohesion, boidsBehaviour.m_CohesionWeight);
			}
		}
	}
}
*/
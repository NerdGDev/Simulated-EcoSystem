using UnityEngine;
using System.Collections.Generic;
using Kit;

namespace FlyAgent.Agents
{
	[CreateAssetMenu]
	public class BoidsBehaviour : PilotBehaviour
	{
		[Header("Flocking:")]
		[Range(0f, 2f)] public float m_SeparationWeight = .5f;
		[Range(0f, 2f)] public float m_AlignmentWeight = .5f;
		[Range(0f, 2f)] public float m_CohesionWeight = .5f;
		public float m_NeighborDistance = 3f;

		[Header("Debug:")]
		public float m_GizmosMultiply = 1f;
		// public FlockDebug m_Flock;

		protected override void OnValidate()
		{
			base.OnValidate();
			if (m_NeighborDistance < 0f)
				m_NeighborDistance = 0f;
		}

	}
}
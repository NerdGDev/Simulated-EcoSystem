using UnityEngine;
using FlyAgent.Utilities;

namespace FlyAgent.Agents
{
	[System.Serializable]
	public class StaticObstacleFleeConfig : AgentConfigBase
	{
		public bool m_Enable = true;
		public override bool IsFeatureEnable() { return m_Enable; }
		public override bool IsShowReport() { return false; }

		public float m_InSightDistanceBias = 5f;
		public float m_SafeDistanceBias = 0.1f;
		public Color m_FleeColor = new Color(1f, 1f, 0f, .1f);

		public override void OnValidate()
		{
			if (m_InSightDistanceBias < 0f)
				m_InSightDistanceBias = 0f;
			if (m_SafeDistanceBias < 0f)
				m_SafeDistanceBias = 0f;
		}
	}

	/// <summary>
	/// When obstacle are not moving and agent are moving toward this flee,
	/// the weight of vector will become higher & stronger in opposite direction.
	/// so it depend on agent current postiion, direction & speed.
	/// </summary>
	public class StaticObstacleFlee : ISteeringFactor
	{
		public static readonly System.Type CONFIG_TYPE = typeof(StaticObstacleFleeConfig);
		private const float SOF_MOVING_IN = 0.75f;
		private const float SOF_MOVING_OUT = 0.25f;
		private const float MIN_SPEED = 0.01f;
		public readonly Vector3 m_Position;
		public readonly float m_Radius;
		public readonly float m_RadiusSqr;
		public readonly FlyAgent m_Agent;
		private StaticObstacleFleeConfig _Config = null;
		private StaticObstacleFleeConfig m_Config
		{
			get
			{
				var obj = m_Agent.Behaviour.GetConfig(CONFIG_TYPE);
				if (!ReferenceEquals(obj, _Config))
				{
					if (obj is StaticObstacleFleeConfig)
						_Config = (StaticObstacleFleeConfig)obj;
					else
						_Config = null;
				}
				return _Config;
			}
		}
		public StaticObstacleFlee(FlyAgent agent, Vector3 position, float radius, StaticObstacleFleeConfig config)
		{
			m_Agent = agent;
			_Config = config;

			m_Position = position;
			m_Radius = radius;
			m_RadiusSqr = m_Radius * m_Radius;
		}

		public WeightVector GetWeightVector()
		{
			var obj = m_Agent.Behaviour.GetConfig(CONFIG_TYPE);
			if (!obj.IsFeatureEnable())
				return WeightVector.zero;
			StaticObstacleFleeConfig config = (StaticObstacleFleeConfig)obj;

			float agentSpeed = m_Agent.CurrentFrameCache.deltaPositionMagnitude;
			if (agentSpeed < MIN_SPEED)
				return WeightVector.zero;
			
			Vector3 agentDirection = m_Agent.CurrentFrameCache.deltaPosition;
			Vector3 relatedDirecton = m_Position - m_Agent.CurrentFrameCache.position;
			float relatedDistanceSqr = relatedDirecton.sqrMagnitude;
			float inSightDistanceBias = config.m_InSightDistanceBias;
			float safeDistance = config.m_SafeDistanceBias + m_Radius;
			float dotAngle = Vector3.Dot(agentDirection, relatedDirecton);
			if (relatedDistanceSqr < safeDistance * safeDistance)
			{
				// too close
				Vector3 inverseDirection = relatedDirecton.normalized * -safeDistance;
				return new WeightVector(inverseDirection, SOF_MOVING_IN);
			}
			else if (dotAngle > 0.5f)
			{
				float weight = (relatedDistanceSqr < m_RadiusSqr) ?
					Mathf.Lerp(SOF_MOVING_IN, SOF_MOVING_OUT, relatedDistanceSqr / m_RadiusSqr) :
					0f;
				// moving toward. within 45 degree deviation.
				Vector3 inverseDirection = relatedDirecton.normalized * -safeDistance;
				return new WeightVector(inverseDirection, weight);
			}
			else
			{
				// agent are heading somewhere else.
				return WeightVector.zero;
			}
		}

		public bool CanRemove()
		{
			Vector3 relatedDirecton = m_Position - m_Agent.CurrentFrameCache.position;
			float relatedDistanceSqr = relatedDirecton.sqrMagnitude;
			return relatedDistanceSqr > m_RadiusSqr;
		}

		public void DrawGizmos()
		{
			if (m_Config == null || !m_Config.IsFeatureEnable())
				return;

			if (m_Config.m_FleeColor.a > 0.0039f) // 1/255 = 0.003921...f
			{
				Gizmos.color = m_Config.m_FleeColor;
				Gizmos.DrawRay(m_Position, GetWeightVector().vector);
				// DebugExtend.DrawRay(m_Position, GetWeightVector().vector, Gizmos.color, 0.3f, false);
			}
		}

		public void DebugReport(System.Text.StringBuilder sb) { }
	}
}
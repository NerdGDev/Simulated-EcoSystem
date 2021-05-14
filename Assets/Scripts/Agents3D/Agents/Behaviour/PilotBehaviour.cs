using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kit;
using Type = System.Type;
using Exception = System.Exception;

namespace FlyAgent.Agents
{
	[CreateAssetMenu]
	public class PilotBehaviour : BehaviourBase, IAgentBehaviour
	{
		public bool m_DebugDraw = false;
		public override bool DebugDraw { get { return m_DebugDraw; } }
		public bool m_DebugLabel = false;
		public override bool DebugLabel { get { return m_DebugLabel; } }

		private Dictionary<Type, AgentConfigBase> m_Features = null;
		public override AgentConfigBase GetConfig(Type type)
		{
			if (m_Features == null)
			{
				Init();
			}

			AgentConfigBase rst;
			if (!m_Features.TryGetValue(type, out rst))
				rst = DefaultBehaviour.NullConfig;
			return rst;
		}
		
		public EngineConfig m_EngineConfig;
		public PilotConfig m_DestinationConfig;
		public StaticObstacleFleeConfig m_StaticObstacleFleeConfig;

		private void Init()
		{
			m_Features = new Dictionary<Type, AgentConfigBase>();
			m_Features.Add(typeof(EngineConfig), m_EngineConfig);
			m_Features.Add(typeof(PilotConfig), m_DestinationConfig);
			m_Features.Add(typeof(StaticObstacleFleeConfig), m_StaticObstacleFleeConfig);
		}

		protected override void OnValidate()
		{
			m_EngineConfig.OnValidate();
			m_DestinationConfig.OnValidate();
			m_StaticObstacleFleeConfig.OnValidate();
		}
	}
}
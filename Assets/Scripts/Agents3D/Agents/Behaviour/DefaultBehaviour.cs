using UnityEngine;

namespace FlyAgent.Agents
{
	/// <summary>A default config class for <see cref="FlyAgent"/>
	/// only provide the basic movement feature if the behaviour element was not setted.
	/// 
	/// any other request will only get the <see cref="DefaultNullConfig"/>,
	/// which will disable the related feature on agent.
	/// </summary>
	public sealed class DefaultBehaviour : IAgentBehaviour
	{
		public static readonly EngineConfig EngineConfig = new EngineConfig();
		public static readonly PilotConfig DestinationConfig = new PilotConfig();

		public static readonly DefaultNullConfig NullConfig = new DefaultNullConfig();
		public class DefaultNullConfig : AgentConfigBase {
			public override bool IsFeatureEnable() { return false; }
			public override bool IsShowReport(){ return false; }
			public override void OnValidate() { }
		}

		public bool DebugDraw { get { return false; } }
		public bool DebugLabel { get { return false; } }
		public AgentConfigBase GetConfig(System.Type type)
		{
			if (type == typeof(PilotConfig))
				return DestinationConfig;
			else if (type == Engine.CONFIG_TYPE)
				return EngineConfig;
			return NullConfig;
		}
	}

	/// <summary>An interface to guid how to wrote an agent behaviour</summary>
	public interface IAgentBehaviour
	{
		bool DebugDraw { get; }
		bool DebugLabel { get; }
		AgentConfigBase GetConfig(System.Type type);
	}

	/// <summary>A scriptable class implement the interfac</summary>
	public abstract class BehaviourBase : ScriptableObject, IAgentBehaviour
	{
		public abstract bool DebugDraw { get; }
		public abstract bool DebugLabel { get; }
		public abstract AgentConfigBase GetConfig(System.Type type);
		protected abstract void OnValidate();
	}

	/// <summary>A base class for all agent features</summary>
	public abstract class AgentConfigBase
	{
		public abstract bool IsFeatureEnable();
		public abstract bool IsShowReport();
		public abstract void OnValidate();
	}
}
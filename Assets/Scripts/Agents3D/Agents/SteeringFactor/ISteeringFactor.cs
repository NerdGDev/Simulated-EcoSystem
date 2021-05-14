using FlyAgent.Utilities;

namespace FlyAgent.Agents
{
	public interface ISteeringFactor
	{
		void DebugReport(System.Text.StringBuilder sb);
		void DrawGizmos();
		WeightVector GetWeightVector();
	}
}
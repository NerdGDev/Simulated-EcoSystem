/*
using FlyAgent.Utilities;

namespace FlyAgent.Agents
{
	public partial class BoidAgent : FlyAgent
	{
		/// <summary>Try to align with group main direction</summary>
		private class Alignment : Flock
		{
			public Alignment(BoidAgent _agent, FlockData _data) : base(_agent, _data) { }
			public override WeightVector GetWeightVector()
			{
				CalculateFlockFactor();
				return m_ShareData.m_AlignmentForce;
			}
		}
	}
}
*/
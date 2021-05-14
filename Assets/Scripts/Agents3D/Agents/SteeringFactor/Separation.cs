/*
using FlyAgent.Utilities;

namespace FlyAgent.Agents
{
	public partial class BoidAgent : FlyAgent
	{
		/// <summary>Try to remain the safe distance between each others.</summary>
		private class Separation : Flock
		{
			public Separation(BoidAgent _agent, FlockData _data) : base(_agent, _data) { }
			public override WeightVector GetWeightVector()
			{
				CalculateFlockFactor();
				return m_ShareData.m_SeparationForce;
			}
		}
	}
}
*/
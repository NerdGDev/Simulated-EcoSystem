/*
using FlyAgent.Utilities;

namespace FlyAgent.Agents
{
	public partial class BoidAgent : FlyAgent
	{
		/// <summary>Group movement by it's centre of mass.</summary>
		private class Cohesion : Flock
		{
			public Cohesion(BoidAgent _agent, FlockData _data) : base(_agent, _data) { }
			public override WeightVector GetWeightVector()
			{
				CalculateFlockFactor();
				return m_ShareData.m_CohesionForce;
			}
		}
	}
}
*/
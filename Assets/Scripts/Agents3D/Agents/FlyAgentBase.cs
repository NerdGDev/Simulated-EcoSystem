using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kit;
using FlyAgent.Utilities;

namespace FlyAgent.Agents
{
	[RequireComponent(typeof(Rigidbody))]
	public abstract class FlyAgentBase : MonoBehaviour
	{
		[SerializeField] protected Rigidbody m_Rigidbody;

		public static List<FlyAgentBase> m_ActiveFlightAgent = new List<FlyAgentBase>(1000);
		protected List<ISteeringFactor> m_SteeringFactors = new List<ISteeringFactor>();

		#region System
		protected virtual void Reset()
		{
			if (m_Rigidbody == null)
				m_Rigidbody = GetComponent<Rigidbody>();
		}

		protected virtual void OnValidate()
		{
			if (m_Rigidbody == null)
				m_Rigidbody = GetComponent<Rigidbody>();
		}

		private void Awake()
		{
			OnAgentInit();
		}

		private void FixedUpdate()
		{
			OnAgentUpdate(Time.fixedDeltaTime);
		}

		protected virtual void OnEnable()
		{
			m_ActiveFlightAgent.Add(this);
		}

		protected virtual void OnDisable()
		{
			m_ActiveFlightAgent.Remove(this);
		}
		#endregion

		#region internal API
		/// <summary>Register all steering force factor on Awake</summary>
		protected abstract void OnAgentInit();

		protected abstract void AgentReset();

		/// <summary>Process all <see cref="m_SteeringForceFactors"/> within fixedUpdate.</summary>
		/// <param name="fixedDeltaTime">FixedUpdate delta time.</param>
		protected virtual void OnAgentUpdate(float fixedDeltaTime)
		{
			Vector3 desiredVector = CalculateSteeringFactors();
			if (!desiredVector.IsNaN())
			{
				ToDestination(fixedDeltaTime, desiredVector);
				// Debug.Log("Invalid force detected, during OnAgentUpdate.", this);
			}
		}

		/// <summary>Based on register factor to calculate final destination</summary>
		/// <returns>A Vector pointed to destination</returns>
		private Vector3 CalculateSteeringFactors()
		{
			WeightVector steeringForceWeight = WeightVector.zero;
			
			int cnt = m_SteeringFactors.Count;
			for (int i=0; i< cnt; i++)
			{
				steeringForceWeight += m_SteeringFactors[i].GetWeightVector();
			}

			return steeringForceWeight.centroid;
		}
		protected abstract void ToDestination(float fixedDeltaTime, Vector3 desiredVector);
		#endregion

		#region public API
		/// <summary>Sets or updates the destination thus triggering the calculation for a new path.</summary>
		/// <param name="target">The target point to navigate to.</param>
		/// <returns>bool True if the destination was requested successfully, otherwise false.</returns>
		public abstract bool SetDestination(Vector3 target);

		protected static readonly Vector3 Vector3Zero = Vector3.zero;
		protected static readonly Vector3 Vector3Forward = Vector3.forward;
		protected static readonly Vector3 Vector3Up = Vector3.up;
		protected static readonly Vector3 Vector3Up45 = Quaternion.AngleAxis(-45f, Vector3.right) * Vector3.forward;
		protected static readonly Vector3 Vector3Down45 = Quaternion.AngleAxis(45f, Vector3.right) * Vector3.forward;
		protected static readonly Vector3 Vector3Right45 = Quaternion.AngleAxis(45f, Vector3.up) * Vector3.forward;
		protected static readonly Vector3 Vector3Left45 = Quaternion.AngleAxis(-45f, Vector3.up) * Vector3.forward;
		#endregion
	}
}
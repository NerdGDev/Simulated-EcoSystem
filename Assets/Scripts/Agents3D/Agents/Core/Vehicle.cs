using UnityEngine;

namespace FlyAgent.Agents
{
	/// <summary>A function to control <see cref="Engine"/> and <see cref="Brake"/>
	/// manage the movement of the vehicle.<seealso cref="FlyAgent"/></summary>
	public class Vehicle : System.IDisposable
	{
		public const HideFlags hideFlags = HideFlags.HideAndDontSave;
		private const float BRAKE_RELEASE_BIAS = .1f;

		public float DriveTime;
		public float Speed;
		public float Brake;
		public Rigidbody m_Rigidbody { get; private set; }
		public Engine m_Engine { get; private set; }
		public Brake m_Brake { get; private set; }
		private bool isDisposed = false;

		public Vehicle(Rigidbody _vehicle, System.Func<Engine> engineParts, System.Func<Brake> brakeParts) :
			this(_vehicle, engineParts(), brakeParts())
		{ }

		public Vehicle(Rigidbody _vehicle, Engine _engine, Brake _brake)
		{
			DriveTime = Speed = Brake = 0;
			m_Rigidbody = _vehicle;
			m_Engine = _engine;
			m_Brake = _brake;
		}

		~Vehicle()
		{
			Dispose();
		}

		/// <summary>Drive to target local position.</summary>
		/// <param name="externalForce"></param>
		/// <param name="rotationBias"></param>
		public void Drive(Vector3 externalForce, Quaternion rotationBias)
		{
			m_Engine.Speed = m_Rigidbody.mass + Speed;
			m_Brake.SetFriction(Brake, BRAKE_RELEASE_BIAS);
			m_Engine.SetPositionAndRotation(m_Rigidbody.position + externalForce, rotationBias);
		}

		/// <summary>StopDrive, mean reset drive time & no brake handle,
		/// but still have physical connection.</summary>
		/// <param name="externalForce"></param>
		/// <param name="externalRotation"></param>
		public void StopDrive(Vector3 externalForce, Quaternion externalRotation)
		{
			m_Engine.Speed = m_Rigidbody.mass + Speed;
			DriveTime = 0f;
			m_Brake.SetFriction(0f);
			m_Engine.SetPositionAndRotation(m_Rigidbody.position + externalForce, externalRotation);
		}

		public void Online()
		{
			if (IsOffline)
			{
				m_Brake.gameObject.SetActive(false);
				m_Engine.StartUp();
			}
		}

		public void Offline()
		{
			if (!IsOffline)
			{
				m_Engine.Shutdown();
				m_Brake.gameObject.SetActive(false);
			}
		}

		public bool IsOffline { get { return !m_Engine.gameObject.activeSelf; } }

		public void Report(System.Text.StringBuilder sb)
		{
			sb.AppendFormat("Speed:{0:F1}\nBrake:{1:F1}\n", Speed, Brake);
		}

		#region IDisposable Support
		public void Dispose()
		{
			if (!isDisposed)
			{
				m_Engine.Dispose();
				m_Brake.Dispose();
				isDisposed = true;
			}
		}
		#endregion
	}
}
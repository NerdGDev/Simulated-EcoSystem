using UnityEngine;
using Kit;

namespace FlyAgent.Agents
{
	public sealed class AirBrake : Brake
	{
		public Rigidbody Vehicle { get; private set; }
		private FixedJoint m_Joint;
		private Rigidbody m_friction;
		float m_TimeAnchor = 0f;
		private bool isDisposed = false;
		
		private void OnDestroy()
		{
			Dispose();
		}

		public static AirBrake Create(Rigidbody vehicle)
		{
			GameObject obj = new GameObject(typeof(AirBrake).Name + "_" + vehicle.GetInstanceID());
			obj.hideFlags = Agents.Vehicle.hideFlags;
			AirBrake brake = obj.AddComponent<AirBrake>();
			brake.Init(vehicle);
			return brake;
		}

		public void Init(Rigidbody vehicle)
		{
			if (isDisposed)
				throw new System.InvalidProgramException();
			Vehicle = vehicle;
			m_friction = gameObject.GetOrAddComponent<Rigidbody>();
			m_friction.mass = float.Epsilon;
			m_friction.drag = 0f;
			m_friction.angularDrag = 0f;
			m_friction.useGravity = false;
			m_friction.isKinematic = false;
			m_Joint = gameObject.AddComponent<FixedJoint>();
			m_Joint.enableCollision = false;
			m_Joint.enablePreprocessing = false;
			Disconnect();
		}

		public override void SetFriction(float amount, float duration = 0f)
		{
			if (isDisposed)
				return;

			if (amount > 0f)
			{
				if (!IsConnected())
					Connect();
				
				m_friction.drag = amount;
				m_friction.angularDrag = amount;
				m_friction.mass = Vehicle.mass - float.Epsilon;
				m_TimeAnchor = Time.timeSinceLevelLoad + duration;
			}
		}
		
		private void FixedUpdate()
		{
			if (isDisposed || m_TimeAnchor <= 0f)
				return;
			if (Vehicle == null)
			{
				Dispose();
				return;
			}

			m_friction.mass = Vehicle.mass - float.Epsilon;
			if (Time.timeSinceLevelLoad > m_TimeAnchor && IsConnected())
			{
				Disconnect();
			}
		}

		private bool IsConnected()
		{
			return m_Joint.connectedBody != null;
		}

		private void Connect()
		{
			if (!IsConnected())
			{
				transform.SetPositionAndRotation(Vehicle.position, Vehicle.rotation);
				m_Joint.connectedBody = Vehicle;
				gameObject.SetActive(true);
			}
			else
				throw new System.InvalidProgramException("Connect() called twice.");
		}

		private void Disconnect()
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(false);
				m_Joint.connectedBody = null;
			}
			else
				throw new System.InvalidProgramException("Disconnect() called twice.");
		}

		#region IDisposable Support
		public sealed override void Dispose()
		{
			if (!isDisposed)
			{
				StopAllCoroutines();
				Destroy(gameObject);
				isDisposed = true;
			}
		}
		#endregion
	}
}
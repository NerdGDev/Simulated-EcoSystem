using System.Collections;
using UnityEngine;
using Kit;

namespace FlyAgent.Agents
{
	/// <summary>The config of <see cref="Engine"/></summary>
	[System.Serializable]
	public sealed class EngineConfig : AgentConfigBase
	{
		public override bool IsFeatureEnable() { return true; }
		public bool m_Report = true;
		public override bool IsShowReport() { return m_Report; }
		public Color m_ColorEngine = Color.white.CloneAlpha(.4f);
		public Color m_ColorVelocity = Color.green.CloneAlpha(.4f);
		public override void OnValidate()
		{
		}
	}

	/// <summary>
	/// A function to control the <see cref="FlyAgent"/> move toward the desired delta position.
	/// also control the rotation of vehicle.
	/// </summary>
	public sealed class Engine : MonoBehaviour, System.IDisposable
	{
		public static readonly System.Type CONFIG_TYPE = typeof(EngineConfig);
		public FlyAgent m_Agent;
		private EngineConfig _Config = null;
		private EngineConfig m_Config
		{
			get
			{
				var obj = m_Agent.Behaviour.GetConfig(CONFIG_TYPE);
				if (!ReferenceEquals(obj, _Config))
				{
					if (obj is EngineConfig)
						_Config = (EngineConfig)obj;
					else
						_Config = null;
				}
				return _Config;
			}
		}

		[SerializeField] int m_CoreCount;
		public int CoreCount { get { return m_CoreCount; } private set { m_CoreCount = value; } }
		public Rigidbody m_CoreRigid;
		private Rigidbody m_VehicleRigid;
		[SerializeField] ConfigurableJoint[] m_Cores;
		public ConfigurableJoint[] Cores { get { return m_Cores; } private set { m_Cores = value; } }

		private float m_Spring = 20f;
		public float Speed { get { return m_Spring; } set { m_Spring = Mathf.Max(0f, value); } }
		private float m_AngularAngleSpeed = 1f;
		public float AngularAngleSpeed { get { return m_AngularAngleSpeed; } set { m_AngularAngleSpeed = Mathf.Max(0, value); } }

		private float m_Damper = 0f;
		public float Stabilizer { get { return m_Damper; } set { m_Damper = Mathf.Max(0f, value); } }
		private bool isDisposed = false;

		private Coroutine m_PeriodicCoroutine = null;
		private Vector3 m_TargetPos;
		private Quaternion m_TargetQuaternion;

		public enum eActiveState
		{
			Init = 0,
			Inited = 1,

			StartUp = 2,
			Enable = 3,
			Shutdown = 4,
			Disable = 5,
		}
		public eActiveState m_ActiveState { get; private set; }

		private static readonly SoftJointLimit LinearLimit = new SoftJointLimit()
		{
			limit = 0.001f,
			bounciness = 0f,
			contactDistance = 0f,
		};

		public static Engine Create(FlyAgent agent, Rigidbody vehicle, int core)
		{
			GameObject obj = new GameObject("Engine_" + vehicle.GetInstanceID());
			obj.transform.SetPositionAndRotation(vehicle.position, vehicle.rotation);
			obj.hideFlags = Vehicle.hideFlags;
			obj.SetActive(false);
			Engine engine = obj.AddComponent<Engine>();
			engine.Init(agent, vehicle, core);
			return engine;
		}

		private void OnDestroy()
		{
			Dispose();
		}

		private void OnValidate()
		{
			if (m_Spring < 0f)
				m_Spring = 0f;
			if (m_Damper < 0f)
				m_Damper = 0f;
			if (m_CoreCount < 0)
				m_CoreCount = 0;

			if (!Application.isPlaying && m_CoreCount > 0)
			{
				ApplySetting(new SoftJointLimitSpring()
				{
					spring = m_Spring,
					damper = m_Damper,
				});
			}
		}

		/// <summary>Should ONLY allow Enable gameobject,
		/// by calling <see cref="StartUp"/></summary>
		private void OnEnable()
		{
			switch (m_ActiveState)
			{
				case eActiveState.Init:
				throw new System.InvalidProgramException(ToString() + ":= require Init() before Awake.");
				case eActiveState.StartUp:
				m_ActiveState = eActiveState.Enable; // normal case
				break;
				case eActiveState.Shutdown:
				throw new System.InvalidProgramException();
				case eActiveState.Disable:
				throw new System.InvalidProgramException(ToString() + " : require correction before active this game object, otherwise displacement issue might happen.");
				/// <see cref="StartUp"/>
				default:
				throw new System.NotImplementedException();
			}
		}

		public void StartUp()
		{
			if (m_ActiveState == eActiveState.Init)
				throw new System.InvalidProgramException(ToString() + ":= require Init() before Startup.");
			else if (m_ActiveState == eActiveState.Disable && m_CoreRigid != null)
			{
				m_ActiveState = eActiveState.StartUp;

				// remove force
				if (m_Cores.Length == 0 || m_Cores[0] == null)
					throw new System.NullReferenceException("Engine should always belong to vehicle.");
				for (int i = 0; i < m_CoreCount; i++)
				{
					m_Cores[i].targetVelocity = Vector3.zero;
					m_Cores[i].targetAngularVelocity = Vector3.zero;
					m_Cores[i].targetPosition = Vector3.zero;
					m_Cores[i].targetRotation = Quaternion.identity;
				}

				// Re-enable - step 1
				m_CoreRigid.velocity = Vector3.zero;
				m_CoreRigid.angularVelocity = Vector3.zero;
				m_TargetPos = m_VehicleRigid.position;
				m_TargetQuaternion = m_VehicleRigid.rotation;
				m_CoreRigid.transform.SetPositionAndRotation(m_TargetPos, m_TargetQuaternion);

				// Re-enable - step 2
				gameObject.SetActive(true); // after Set pos & rotation.

				// Re-enable - step 3
				m_CoreRigid.WakeUp();
				// Debug.Log("Re-Enable " + name + "  (" + Time.frameCount + ") ", this);
			}
			else if (m_ActiveState == eActiveState.StartUp)
				throw new System.InvalidProgramException(ToString() + ":= Startup twice.");
			else if (m_ActiveState == eActiveState.Shutdown)
				throw new System.InvalidProgramException(ToString() + ":= Logic flow error, Missing 'Disable' after 'Shutdown'.");
			else
				throw new System.NotImplementedException();
		}

		/// <summary>Should ONLY allow Disable gameobject,
		/// by calling <see cref="Shutdown"/></summary>
		private void OnDisable()
		{
			// Debug.Log("Disable--" + name + "  (" + Time.frameCount + ") ", this);
			if (m_ActiveState == eActiveState.Shutdown)
				m_ActiveState = eActiveState.Disable;
			else
				throw new System.InvalidProgramException(ToString() + ":= can't disable engine before shutdown.");
		}

		public void Shutdown()
		{
			if (m_ActiveState == eActiveState.Enable)
			{
				m_ActiveState = eActiveState.Shutdown;

				// remove force
				m_CoreRigid.velocity = Vector3.zero;
				m_CoreRigid.angularVelocity = Vector3.zero;
				m_CoreRigid.Sleep();

				// clean up
				m_PeriodicCoroutine = null;

				// disable
				gameObject.SetActive(false);
			}
			else
			{
				throw new System.InvalidProgramException(ToString() + ":= can't shutdown engine before it's fully startup. - " + m_ActiveState.ToString("F"));
			}
		}

		private IEnumerator PeriodicUpdate()
		{
			while (transform.position != m_TargetPos || transform.rotation != m_TargetQuaternion)
			{
				float delta = Time.fixedDeltaTime;
				// Position
#if true
				/// input world position was clampped by...
				/// <see cref="Vehicle.Drive(Vector3, Quaternion, float)"/>
				Vector3 pos = m_TargetPos;
#else
				private const float ClampDistance = 1f;
				Vector3 vector = m_TargetPos - transform.position;
				if (vector.sqrMagnitude > ClampDistance * ClampDistance)
					vector = vector.normalized * ClampDistance;
				Vector3 pos = transform.TransformPoint(vector);
#endif

				// Rotation
				Quaternion rotate = Quaternion.Slerp(transform.rotation, m_TargetQuaternion, delta * AngularAngleSpeed);

				// Set Engine transform
				transform.SetPositionAndRotation(pos, rotate);

				// Apply force
				ApplySetting(new SoftJointLimitSpring()
				{
					spring = m_Spring,
					damper = m_Damper,
				});
				yield return new WaitForFixedUpdate();
			}
			m_PeriodicCoroutine = null; // clear up flag.
		}

		public void DrawGizmos()
		{
			// Draw pilot & engine
			Gizmos.color = m_Config.m_ColorEngine;

			// Current velocity of agent
			if (m_Config.m_ColorVelocity.a > 0.0039f) // 1f/255f
			{
				GizmosExtend.DrawLine(
					m_VehicleRigid.position,
					m_VehicleRigid.position + m_VehicleRigid.velocity,
					m_Config.m_ColorVelocity);
			}

			// Visualize engine position
			GizmosExtend.DrawCircle(transform.position, transform.up, radius: 2f);
			GizmosExtend.DrawArrow(transform.position, transform.forward * 2f, angle: 10f);
			Gizmos.DrawRay(transform.position, transform.up);

			// Core 1 will be no distance from center. skip that draw.
			if (m_CoreCount > 1)
			{
				for (int i = 0; i < m_CoreCount; i++)
				{
					Vector3 p1 = m_CoreRigid.transform.TransformPoint(m_Cores[i].anchor);
					Vector3 p2 = m_VehicleRigid.transform.TransformPoint(m_Cores[i].connectedAnchor);
					Gizmos.DrawLine(p1, p2);
				}
			}
		}

		private void Init(FlyAgent agent, Rigidbody vehicle, int coreNum)
		{
			if (m_VehicleRigid != null)
				throw new System.InvalidProgramException(ToString() + ":= can only init once.");

			m_Agent = agent;
			CoreCount = coreNum;
			if (coreNum < 1)
				throw new System.NotSupportedException(ToString() + ":= Core amount must more than 1.");
			else
			{
				float avgAngle = 360f / (float)coreNum;
				float accumulateAngle = 0f;
				m_Cores = new ConfigurableJoint[CoreCount];
				m_CoreRigid = gameObject.GetOrAddComponent<Rigidbody>();
				m_CoreRigid.angularDrag = 0f;
				m_CoreRigid.drag = 0f;
				m_CoreRigid.isKinematic = true;
				m_CoreRigid.useGravity = false;
				m_VehicleRigid = vehicle;
				float distance = coreNum == 1 ? 0f : 2f;
				for (int i = 0; i < CoreCount; i++)
				{
					m_Cores[i] = gameObject.AddComponent<ConfigurableJoint>();
					ConfigCore(ref m_Cores[i], vehicle, accumulateAngle, distance, coreNum < 2);
					accumulateAngle += avgAngle;
				}
			}
			
			m_ActiveState = gameObject.activeSelf ? eActiveState.Enable : eActiveState.Disable;
		}

		public void SetPositionAndRotation(Vector3 worldPosition, Quaternion worldRotation)
		{
			m_TargetPos = worldPosition;
			m_TargetQuaternion = worldRotation;
			if (m_PeriodicCoroutine == null)
				m_PeriodicCoroutine = StartCoroutine(PeriodicUpdate());
		}

		private void ApplySetting(SoftJointLimitSpring val)
		{
			foreach (var joint in m_Cores)
			{
				joint.linearLimitSpring = val;
			}
		}

		private void ConfigCore(ref ConfigurableJoint joint, Rigidbody vehicle, float angle, float distance, bool LimitAngular)
		{
			Vector3 direction = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, distance);
			direction = vehicle.transform.TransformDirection(direction);

			joint.connectedBody = vehicle;
			joint.transform.SetPositionAndRotation(vehicle.position, vehicle.rotation);
			joint.anchor = direction * 2f;
			joint.axis = Vector3.right;
			joint.autoConfigureConnectedAnchor = false;
			joint.connectedAnchor = direction;
			joint.secondaryAxis = Vector3.up;

			// position
			joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Limited;
			joint.linearLimit = LinearLimit;
			joint.linearLimitSpring = new SoftJointLimitSpring()
			{
				spring = m_Spring,
				damper = m_Damper,
			};

			// rotation
			if (LimitAngular)
			{
				joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Limited;
				joint.angularYLimit = joint.angularZLimit = LinearLimit;
				joint.lowAngularXLimit = joint.highAngularXLimit = LinearLimit;
			}
			else
			{
				joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Free;
			}

			// others
			joint.projectionMode = JointProjectionMode.PositionAndRotation;
			joint.rotationDriveMode = RotationDriveMode.XYAndZ;
			joint.configuredInWorldSpace = false;
			joint.breakForce = float.PositiveInfinity;
			joint.breakTorque = float.PositiveInfinity;
			joint.enableCollision = false;
			joint.enablePreprocessing = false;
		}

		public override string ToString()
		{
			return typeof(FlyAgentBase).Name + "'s " + GetType().Name +
				(m_VehicleRigid ? m_VehicleRigid.name : " -null- ");
		}

		#region IDisposable Support
		public void Dispose()
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

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Kit;
using FlyAgent.Navigation;


namespace FlyAgent.Agents
{
	public class FlyAgent : FlyAgentBase
	{
		#region Magic numbers
		// when distance are too far, the slippery effect will appear when destination are changing frequently.
		private const float deltaAccelerationDistance = 2f;
		private const float sqrDeltaAccelerationDistance = deltaAccelerationDistance * deltaAccelerationDistance;

		private const float OBSTACLE_WEIGHT_WRONG_DIRECTION = 1.25f; // moving toward to obstacle.
		private const float OBSTACLE_WEIGHT_RIGHT_DIRECTION = 0.25f; // not moving forward to obstacle.
		private const float OBSTACLE_FLEE_EXPIRE = 0.03f;

		// coreCount will affect the stability, when external force influence apply.
		private const int CORE_COUNT = 1;
		
		private static readonly Color ColorAgent = new Color(.4f, .8f, .4f, .5f);
		private static GUIStyle InfoStyle;
		#endregion

		#region variables
		[Header("Speed Control")]
		/// <summary>Engine maximum power output, giving as Unit/Sec^2</summary>
		public float m_MaxSpeed = 20f;

		public float m_AugularAngleSpeed = 5f;
		
		/// <summary>Engine have the performance curve to reach highest house power output.</summary>
		[SerializeField] [RectRange(0f, 0f, 1f, 1f)] AnimationCurve m_AccelerationCurve = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1f);

		/// <summary>How long the engine will reach it's best performance, (the most right side on acceleration curve)</summary>
		[SerializeField] float m_AccelerationTime = 3f;


		[Header("Brake Control")]
		/// <summary>Depended on destination, to define when to start braking behaviour.
		/// present in unit 1 == 1 unit.</summary>
		public float m_BrakingDistance = 5f;

		/// <summary>The max braking force this vehicle can support.
		/// a damping for rigidbody</summary>
		public float m_MaxBrakingForce = 10f;

		/// <summary>
		/// Brake system (Deceleration), allow to have performance limitation,
		/// brake performance can be defined by this curve
		/// left most (X-axis) repesent the start braking force Multiply by <see cref="m_MaxBrakingForce"/>
		/// right most (X-axis) repesent the braking force just arrived on target
		/// the bottom & top (Y-axis) to repesent the braking force output percentage
		/// </summary>
		[SerializeField] [RectRange(0f, 0f, 1f, 1f)] AnimationCurve m_DecelerationCurve = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1f);

		/// <summary>The distance close enough and pilot will stop engine
		/// use this to define arrived,
		/// otherwise pilot will keep trying to travel to destination.
		/// </summary>
		public float m_ArrivedDistance = 0.5f;

		public enum eUpwardBias
		{
			/// <summary>Not maintain upward</summary>
			Local = 0,
			/// <summary>Align to Vector.up</summary>
			WorldUp = 1,
		}
		/// <summary>Flight will try to maintance it's upward during travel.</summary>
		public eUpwardBias m_UpwardBias = eUpwardBias.WorldUp;

		[Header("Shape of agent, (Obstacle Avoidance)")]
		/// <summary>Capsule direction</summary>
		public eDirection m_Direction = eDirection.ZAxis;
		public enum eDirection { XAxis = 0, YAxis = 1, ZAxis = 2, }
		/// <summary>Capsule radius of the agent</summary>
		public float m_Radius = 0.5f;
		/// <summary>Capsule length based on direction.</summary>
		public float m_Length = 0f;
		/// <summary>Capsule pivot offset based on direction</summary>
		public float m_Offset = 0f;

		[Header("Physical")]
		public LayerMask m_LayerMask = Physics.DefaultRaycastLayers;
		public QueryTriggerInteraction m_QueryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
		public eQuality m_Quality = eQuality.MediumQuality;
		public enum eQuality
		{
			None = 0,
			LowQuality,
			MediumQuality,
			GoodQuality,
			HighQuality,
		}

		public AgentSize Size { get { return m_Size; } }
		protected AgentSize m_Size;
		
		public Vehicle Vehicle { get { return m_Vehicle; } }
		protected Vehicle m_Vehicle;

		public CurrentFrameCache CurrentFrameCache { get { return m_CurrentFrameCache; } }
		protected CurrentFrameCache m_CurrentFrameCache;

		private StringBuilder m_DebugReport = null;
		#endregion

		[Header("Behaviour, Feature & Debug")]
		[SerializeField] protected BehaviourBase m_Behaviour = null;
		public static readonly DefaultBehaviour m_DefaultBehaviour = new DefaultBehaviour();
		public IAgentBehaviour Behaviour
		{
			get
			{
				if (m_Behaviour == null)
					return m_DefaultBehaviour;
				else
					return m_Behaviour;
			}
		}

		#region System
		protected override void OnValidate()
		{
			base.OnValidate();
			if (m_MaxSpeed < 0f)
				m_MaxSpeed = 0f;
			if (m_AccelerationTime < 0f)
				m_AccelerationTime = 0f;
			if (m_BrakingDistance < 0f)
				m_BrakingDistance = 0f;
			if (m_MaxBrakingForce < 0f)
				m_MaxBrakingForce = 0f;
			if (m_ArrivedDistance < 0.01f)
				m_ArrivedDistance = 0.01f;
			if (m_Radius < 0f)
				m_Radius = 0f;
			if (m_Length < 0f)
				m_Length = 0f;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Vehicle.Online();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			AgentReset();
			Vehicle.Offline();
			m_Pilot.Reset();
		}

		protected virtual void OnDrawGizmosSelected()
		{
			if (!Application.isPlaying)
			{
				m_Size = new AgentSize(this);
				Vector3[] point = m_Size.GetCapsule();
				GizmosExtend.DrawCapsule(point[0], point[1], m_Radius, ColorAgent);
			}
			else if (Behaviour.DebugLabel)
			{
				if (m_DebugReport == null)
					m_DebugReport = new StringBuilder(1000);
				else
					m_DebugReport.Remove(0, m_DebugReport.Length);

				if (((EngineConfig)Behaviour.GetConfig(Engine.CONFIG_TYPE)).IsShowReport())
					Vehicle.Report(m_DebugReport);

				m_DebugReport.AppendLine("Factors:" + m_SteeringFactors.Count);

				int cnt = m_SteeringFactors.Count;
				for (int i = 0; i < cnt; i++)
				{
					m_SteeringFactors[i].DebugReport(m_DebugReport);
				}

				GizmosExtend.DrawLabel(m_Rigidbody.position + Vector3.down, m_DebugReport.ToString(), InfoStyle);
			}
		}

		protected virtual void OnDrawGizmos()
		{
			if (Behaviour.DebugDraw && Application.isPlaying)
			{
				Color old = Gizmos.color;

				Vehicle.m_Engine.DrawGizmos();
				
				int cnt = m_SteeringFactors.Count;
				for (int i = 0; i < cnt; i++)
				{
					m_SteeringFactors[i].DrawGizmos();
				}
				Gizmos.color = old;
			}
		}

		private void OnDestroy()
		{
			Vehicle.Dispose();
		}
		#endregion // System

		#region internal API
		protected override void OnAgentInit()
		{
			if (InfoStyle == null)
			{
				Texture2D grayTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false, false);
				grayTexture.SetPixel(1, 1, Color.gray.CloneAlpha(0.5f));
				grayTexture.alphaIsTransparency = true;
				grayTexture.anisoLevel = 0;
				grayTexture.Apply();
				InfoStyle = new GUIStyle()
				{
					alignment = TextAnchor.UpperLeft,
					normal = new GUIStyleState()
					{
						textColor = Color.cyan,
						background = grayTexture,
					},
					padding = new RectOffset(3,3,3,3),
					border = new RectOffset(2,2,2,2),
				};
			}

			m_Size = new AgentSize(this);
			m_Vehicle = new Vehicle(m_Rigidbody, Engine.Create(this, m_Rigidbody, CORE_COUNT), AirBrake.Create(m_Rigidbody));
			m_CurrentFrameCache = new CurrentFrameCache(this, Vehicle, m_Rigidbody);
			
			m_Pilot = new Pilot(this);
			m_SteeringFactors.Add(m_Pilot);
		}

		protected override void OnAgentUpdate(float fixedDeltaTime)
		{
			CurrentFrameCache.Update();
			LocalObstacleAvoidance();

			base.OnAgentUpdate(fixedDeltaTime);
		}

		protected override void AgentReset()
		{
			m_Pilot.Reset();
			int i = m_SteeringFactors.Count;
			while (i --> 0)
			{
				if (m_SteeringFactors[i] is StaticObstacleFlee)
				{
					m_SteeringFactors.RemoveAt(i);
				}
			}
			m_StaticObstacleFlee.Clear();
		}

		protected override void ToDestination(float fixedDeltaTime, Vector3 flightVector)
		{
			if (flightVector.IsNaN() || flightVector.IsInfinity())
				return;

			if (m_MaxSpeed > 0f && HasDestination())
			{
				Vehicle.DriveTime += fixedDeltaTime;

				// defined current speed
				float pt = Mathf.Clamp01(Vehicle.DriveTime / m_AccelerationTime - Random.value);
				Vehicle.Speed = m_MaxSpeed * m_AccelerationCurve.Evaluate(pt);

				// Where we should look at?
				Vector3 destinationVector = GetDestination() - m_Rigidbody.position;
				Vector3 upward = m_UpwardBias == eUpwardBias.WorldUp ? Vector3Up : transform.up;
				Quaternion faceRotation;
				
				if (m_Pilot.m_WaypointParams.HasPath())
				{
					// face to next waypoint, if we have path.
					faceRotation = Quaternion.LookRotation(m_Pilot.m_WaypointParams.m_ApproachingDirection, upward);
				}
				else if (destinationVector.sqrMagnitude > 0f)
				{
					// face to destination.
					Vector3 dir = Vector3.Slerp(destinationVector, CurrentFrameCache.velocity, .5f);
					faceRotation = Quaternion.LookRotation(dir, upward);
				}
				else
				{
					faceRotation = transform.rotation;
				}

				float distance = destinationVector.magnitude;
				if (m_Pilot.m_PathState == Pilot.ePathState.Patrol_Waypoint &&
					!m_Pilot.IsWithinTurningDeviation() &&
					m_Pilot.IsOverSpeed())
				{
					// moving toward corner, expect the turning later on. slow down a bit
					Vehicle.Brake = m_MaxBrakingForce * Mathf.Min(0.5f, m_DecelerationCurve.keys[0].value);
				}
				else if (m_Pilot.m_PathState == Pilot.ePathState.Patrol_SameNode &&
					distance < m_BrakingDistance)
				{
					// almost arrive, attempt stop.
					Vehicle.Brake = m_MaxBrakingForce * m_DecelerationCurve.Evaluate(distance / m_BrakingDistance);
				}
				else
				{
					// haven't reach destination yet.
					Vehicle.Brake = 0f;
				}

				// Stabilizer, when the destination are too far away.
				if (flightVector.sqrMagnitude > sqrDeltaAccelerationDistance)
					flightVector = flightVector.normalized * deltaAccelerationDistance;

				Vehicle.Drive(flightVector, faceRotation);
			}
			else
			{
				Vehicle.StopDrive(flightVector, m_Rigidbody.rotation);
			}
			Vehicle.m_Engine.AngularAngleSpeed = m_AugularAngleSpeed;
		}

		#endregion

		#region Pilot - Destination Control (Path finding)
		public Pilot m_Pilot { get; private set; }

		/// <summary>only if agent still in travel</summary>
		/// <returns></returns>
		public bool HasDestination() { return !m_Pilot.m_ArrivedFlag; }

		/// <summary>Set target destination to this flight</summary>
		/// <param name="target">world space position</param>
		/// <returns>true = successful apply</returns>
		public override bool SetDestination(Vector3 target)
		{
			if (target.IsNaN())
			{
				Debug.LogError(name + " " + GetType().Name + " : Ignored, invalid destination" + target.ToString(), this);
				return false;
			}

			return m_Pilot.SetTarget(target);
		}

		/// <summary>Get current destination</summary>
		/// <returns>last destination in memory</returns>
		public Vector3 GetDestination()
		{
			if (m_Pilot.m_WaypointParams.m_WayPoints.Count > 0)
				return m_Pilot.m_WaypointParams.m_WayPoints[0].center;
			return m_Pilot.GetGoal();
		}
		#endregion

		#region Obstacle avoidance (Flee)
		private List<StaticObstacleFlee> m_StaticObstacleFlee = new List<StaticObstacleFlee>(100);

		private void LocalObstacleAvoidance()
		{
			if (!HasDestination())
				return;

			var obj = Behaviour.GetConfig(StaticObstacleFlee.CONFIG_TYPE);
			if (!obj.IsFeatureEnable())
				return;
			
			StaticObstacleFleeConfig config = (StaticObstacleFleeConfig)obj;
			int cnt = m_StaticObstacleFlee.Count;
			for (int i=0; i<cnt; i++)
			{
				m_SteeringFactors.Remove(m_StaticObstacleFlee[i]);
			}
			m_StaticObstacleFlee.Clear();

			// check the map
			Vector3 predictPos = CurrentFrameCache.position + CurrentFrameCache.deltaPosition;
			Bounds nearlyBounds = m_Size.GetBounds();
			nearlyBounds.extents += Vector3.one * (m_Radius * config.m_InSightDistanceBias);
			List<Octree.Node> flees = MapBaker.GetInstance().GetNearlyByObstacle(predictPos, nearlyBounds);
			cnt = flees.Count;
			for (int i = 0; i < cnt; i++)
			{
				StaticObstacleFlee soflee = new StaticObstacleFlee(this, flees[i].bounds.center, flees[i].bounds.extents.x + config.m_InSightDistanceBias, config);
				m_StaticObstacleFlee.Add(soflee);
				m_SteeringFactors.Add(soflee);
			}
		}

		
		#endregion
	}
}
#define BACKGROUND_THREAD
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using FlyAgent.Navigation;
using FlyAgent.Utilities;
using Kit;

namespace FlyAgent.Agents
{
	[System.Serializable]
	public class PilotConfig : AgentConfigBase
	{
		public override bool IsFeatureEnable() { return true; }
		public bool m_Report = false;
		public override bool IsShowReport() { return m_Report; }

		[Range(-1f, 1f)] public float m_AngleDeviationThreshold = 0.78f; // around 30degree +/-
		public Color m_ColorGoalArea = Color.green.CloneAlpha(.4f);
		public Color m_ColorWaypoints = Color.gray.CloneAlpha(.3f);
		public Color m_ColorWaypointsNode = Color.gray.CloneAlpha(.3f);
		public override void OnValidate()
		{
		}
	}

	/// <summary>The path finding handler, to control the <see cref="FlyAgent"/>
	/// travel to the destination by following the path.</summary>
	public class Pilot : ISteeringFactor
	{
		public static readonly System.Type CONFIG_TYPE = typeof(PilotConfig);
		private const float DESTINATION_WEIGHT = 1f;
		private readonly FlyAgent m_Agent;
		private Vector3 m_GoalPoint;
		public Vector3 GetGoal() { return m_GoalPoint; }
		public bool m_ArrivedFlag { get; private set; }
		public ePathState m_PathState { get; private set; }

		[System.Obsolete("use WaypointParams.m_Waypoint")]
		public List<Bounds> m_WayPoints { get { return m_WaypointParams.m_WayPoints; } }
		
		public WaypointParams m_WaypointParams { get; private set; }
		
		private static readonly Vector2 m_PerformanceRange = new Vector2(0.55f, 0.85f);
		private float m_PerformanceShiftDelay;
		private float m_NextGoalRequest = 0f;
		private Vector3 m_NextGoalPoint;
		private Coroutine m_UpdateGoalCoroutine = null;
		private PilotConfig _Config = null;
		private PilotConfig m_Config
		{
			get
			{
				var obj = m_Agent.Behaviour.GetConfig(CONFIG_TYPE);
				if (!ReferenceEquals(obj, _Config))
				{
					if (obj is PilotConfig)
						_Config = (PilotConfig)obj;
					else
						_Config = null;
				}
				return _Config;
			}
		}

		public enum ePathState
		{
			Idle = 0,
			OctreeLookup,

			PathInit = 10,
			PathCalculating,
			Patrol_Waypoint,
			Patrol_SameNode,
			Patrol_WithoutPath, // when path finder can't find the path.

			Fail_Unknown = 100,
			Fail_NullStartPoint,
			Fail_NullEndPoint,
			Fail_PathEmpty,
		}
		private LazyThetaStar<Octree.Node>.PathFinder m_PathFinder = null;

		public Pilot(FlyAgent _agent)
		{
			m_Agent = _agent;
			m_PerformanceShiftDelay = Random.Range(m_PerformanceRange.x, m_PerformanceRange.y);
			m_WaypointParams = new WaypointParams(m_Agent, this);
			Reset();
		}

		public void Reset()
		{
			m_ArrivedFlag = true;
			if (m_PathFinder != null)
				m_PathFinder.Dispose();
			m_WaypointParams.Reset();
			m_PathState = ePathState.Idle;
			m_NextGoalRequest = 0f;
			if (m_UpdateGoalCoroutine != null)
				m_Agent.StopCoroutine(m_UpdateGoalCoroutine);
			m_NextGoalPoint = m_GoalPoint = default(Vector3);
		}

		private IEnumerator UpdateGoalCoroutine()
		{
			yield return new WaitUntil(() => m_NextGoalRequest < Time.timeSinceLevelLoad);
			SetTarget(m_NextGoalPoint);
			m_UpdateGoalCoroutine = null;
		}

		#region PathFinding
		public bool SetTarget(Vector3 _point)
		{
			// ignore common case cause by pooling control, this object are disabled, ignore that.
			if (!m_Agent.gameObject.activeSelf || !m_Agent.gameObject.activeInHierarchy)
				return true;

			if (m_PathState > ePathState.Idle && m_PathState < ePathState.Patrol_Waypoint)
			{
#if !BACKGROUND_THREAD
				Debug.LogError("double path request during searching.", m_Agent);
#endif
				return true;
			}

			// distance bias ignore.
			float distanceBiasSqr = Mathf.Max(m_Agent.m_BrakingDistance, m_Agent.m_Radius, m_Agent.m_ArrivedDistance);
			distanceBiasSqr *= distanceBiasSqr;
			if (m_PathState >= ePathState.Patrol_Waypoint &&
				m_PathState < ePathState.Fail_Unknown &&
				(_point - m_GoalPoint).sqrMagnitude < distanceBiasSqr)
				return true; // since they are too close, conside it's the same point.

			// time lock
			if (m_NextGoalRequest > Time.timeSinceLevelLoad)
			{
				if (m_UpdateGoalCoroutine != null)
					m_Agent.StopCoroutine(m_UpdateGoalCoroutine);
				m_NextGoalPoint = _point;
				m_UpdateGoalCoroutine = m_Agent.StartCoroutine(UpdateGoalCoroutine());
				return true; // put it into cache instead of doing it now.
			}

			// renew time
			m_NextGoalRequest = Time.timeSinceLevelLoad + m_PerformanceShiftDelay;

			Profiler.BeginSample("Set Destination Target");
			Vector3 desired = _point - m_Agent.CurrentFrameCache.position;
			if (desired.sqrMagnitude > m_Agent.CurrentFrameCache.arrivedDistanceSqr)
			{
				m_GoalPoint = _point;
				m_ArrivedFlag = false;

				List<Octree.Node> startNodes, endNodes;
				if (SpaceNodeLookup(out startNodes, out endNodes))
				{
					Octree.Node start, end;
					InitStartEndNodes(startNodes, endNodes, out start, out end);
					if (start.Equals(end))
					{
						// same node travel(space), just move toward.
						m_PathState = ePathState.Patrol_SameNode;
					}
					else
					{
						FindPathBetween(start, end);
					}
				}
			}
			Profiler.EndSample();
			return !m_ArrivedFlag;
		}

		/// <summary>Remove the obstacle(s)</summary>
		/// <param name="startNodes">A list of nodes that could be start node</param>
		/// <param name="endNodes">A list of nodes that could be end node</param>
		/// <returns>true = all process success</returns>
		private bool SpaceNodeLookup(out List<Octree.Node> startNodes, out List<Octree.Node> endNodes)
		{
			m_PathState = ePathState.OctreeLookup;

			// agent's size.
			Bounds agentBounds = m_Agent.Size.GetBounds();
			agentBounds.extents += Vector3.one * m_Agent.m_Radius;

			startNodes = new List<Octree.Node>(8);
			endNodes = new List<Octree.Node>(8);

			MapBaker.GetInstance().OctreeStatic.GetCollidingLeafNode(startNodes, agentBounds);
			startNodes.RemoveAll(n => n.HasObstacle());
			if (startNodes.Count == 0)
			{
				m_PathState = ePathState.Fail_NullStartPoint;
				return false;
			}
			MapBaker.Instance.OctreeStatic.GetCollidingLeafNode(endNodes, new Bounds(m_GoalPoint, agentBounds.size));
			endNodes.RemoveAll(n => n.HasObstacle());
			if (endNodes.Count == 0)
			{
				m_PathState = ePathState.Fail_NullEndPoint;
				return false;
			}
			return true;
		}

		/// <summary>Found out the most suitable node for path finding usage.</summary>
		/// <param name="startNodes"></param>
		/// <param name="endNodes"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		private void InitStartEndNodes(List<Octree.Node> startNodes, List<Octree.Node> endNodes, out Octree.Node start, out Octree.Node end)
		{
			m_PathState = ePathState.PathInit;
			// choose closer node to the goal, so the agent no move backward on start
			start = ClosestToPoint(m_GoalPoint, startNodes);

			// found node that contain goal point.
			end = ClosestToPoint(m_GoalPoint, endNodes);
		}

		private void FindPathBetween(Octree.Node start, Octree.Node end)
		{
			Profiler.BeginSample("FindPathBetween");
			// path finding waypoints
			Bounds agentBounds = m_Agent.Size.GetBounds(true);
			float agentSize = agentBounds.size.sqrMagnitude;
			m_PathFinder = LazyThetaStar<Octree.Node>.FindPath(start, end, agentSize);
			m_PathFinder.maxIterations = 200;
			m_PathFinder.heuristicWeight = 1f; // ?? 1.5f vertex nodes movement request more cost.

			m_PathState = ePathState.PathCalculating;
#if BACKGROUND_THREAD
			m_PathFinder.AsyncFind(_PathResultAnalisys);
#else
			// Org version Heavy cost!
			_PathResultAnalisys(m_PathFinder.QuickFind().Cast<Octree.Node>().ToList());
#endif

			Profiler.EndSample();
		}

		private void _PathResultAnalisys(List<Octree.Node> waypoints)
		{
			m_WaypointParams.Reset();
			if (waypoints == null)
				m_PathState = ePathState.Patrol_WithoutPath;
			else if (waypoints.Count == 0)
				m_PathState = ePathState.Fail_PathEmpty;
			else if (waypoints.Count == 1)
				m_PathState = ePathState.Patrol_SameNode;
			else if (waypoints.Count > 1)
			{
				m_PathState = ePathState.Patrol_Waypoint;
				m_WaypointParams.SetWaypoints(waypoints.Select(n => n.bounds));
			}
			else
			{
				throw new System.InvalidProgramException("Missing case.");
			}
		}
		
		/// <summary>Find the closest node, Assume 2 nodes are different</summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <param name="point">reference node</param>
		/// <returns></returns>
		private Octree.Node ClosestToPoint(Vector3 point, List<Octree.Node> nodes)
		{
			int cnt = nodes.Count;
			Octree.Node rst = nodes[0];
			while (cnt-- > 0)
			{
				rst = CloserToPoint(m_GoalPoint, rst, nodes[cnt]);
			}
			return rst;
		}

		/// <summary>Find the closer node, Assume 2 nodes are different</summary>
		/// <param name="point">reference point</param>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <returns></returns>
		private Octree.Node CloserToPoint(Vector3 point, Octree.Node lhs, Octree.Node rhs)
		{
			if ((lhs.center - point).sqrMagnitude < (rhs.center - point).sqrMagnitude)
				return lhs;
			else
				return rhs;
		}
		#endregion // PathFinding

		#region Debug Gizmos
		public void DrawGizmos()
		{
#if UNITY_EDITOR
			if (m_Config == null || !m_Config.IsFeatureEnable())
				return;
			// Current Path
			if (m_ArrivedFlag)
				return;

			switch (m_PathState)
			{
				case ePathState.Patrol_Waypoint:
					Gizmos.color = m_Config.m_ColorWaypoints;
					int cnt = m_WaypointParams.m_WayPoints.Count;
					Vector3 pt = m_Agent.CurrentFrameCache.position;
					for (int i = 0; i < cnt; i++)
					{
						Gizmos.DrawLine(pt, m_WaypointParams.m_WayPoints[i].center);
						pt = m_WaypointParams.m_WayPoints[i].center;
						GizmosExtend.DrawBounds(m_WaypointParams.m_WayPoints[i], m_Config.m_ColorWaypointsNode);
					}
					goto case ePathState.Patrol_SameNode;
				case ePathState.Patrol_SameNode:
					Gizmos.color = m_Config.m_ColorGoalArea;
					Gizmos.DrawWireSphere(GetGoal(), m_Agent.m_BrakingDistance);
					break;
			}
#endif
		}

		public void DebugReport(System.Text.StringBuilder sb)
		{
			if (m_Config == null || !m_Config.IsShowReport())
				return;

			if (m_ArrivedFlag)
			{
				sb.AppendFormat("Destination : {0}", m_PathState.ToString("F"));
			}
			else
			{
				sb.AppendFormat("Destination : {0}\n\tWaypoint remain = {1}\n\tGoal = {2:F2}", 
					m_PathState.ToString("F"), m_WaypointParams.m_WayPoints.Count, m_GoalPoint);
			}
		}
		#endregion // Debug  Gizmos

		/// <summary>current predictive turning angle is within the deviation.</summary>
		/// <returns>true = within deviation, conside it's straight line.</returns>
		public bool IsWithinTurningDeviation()
		{
			return m_WaypointParams.m_PredictiveTurnAngle > m_Config.m_AngleDeviationThreshold;
		}

		/// <summary>based on current velocity, is agent will over shoot the target?</summary>
		/// <returns>true = yes, we driving too fast.</returns>
		public bool IsOverSpeed()
		{
			bool isForward = 0 < Vector3.Dot(m_Agent.CurrentFrameCache.velocityNormalize, m_WaypointParams.m_ApproachingDirection_Normalized);
			bool overSpeed = m_Agent.CurrentFrameCache.velocitySqrMagnitude > m_WaypointParams.m_ApproachingDirection.sqrMagnitude;
			return isForward && overSpeed;
		}

		/// <summary>Steering factor</summary>
		/// <returns>the recommand direction to travel.</returns>
		public WeightVector GetWeightVector()
		{
			if (m_ArrivedFlag) // common case quit early
			{
				return WeightVector.zero;
			}
			else if (m_PathState == ePathState.Patrol_Waypoint)
			{
				if (m_WaypointParams.m_WayPoints.Count > 1)
				{
					m_WaypointParams.Update();
					return new WeightVector(m_WaypointParams.m_ApproachingDirection, DESTINATION_WEIGHT);
				}
				else if (m_WaypointParams.m_WayPoints.Count == 1)
				{
					// after Pathfinding search, the result just a single node.
					return _DirectTravelVector();
				}
				else // if (m_WayPoints.Count == 0)
				{
					// since path state will change to Patrol_SameNode
					Debug.LogError("Should change mode before reaching this session.", m_Agent);
					Reset();
					return WeightVector.zero;
				}
			}
			else if (m_PathState == ePathState.Patrol_SameNode)
			{
				return _DirectTravelVector();
			}
			else if (m_PathState <= ePathState.PathCalculating)
			{
				return WeightVector.zero;
			}
			else if (m_PathState == ePathState.Fail_NullStartPoint ||
				m_PathState == ePathState.Fail_NullEndPoint)
			{
				// the octree may not include this area.
				// TODO: if one of those point able to find, travel to the closest node. := 3 cases ?
				return _DirectTravelVector();
			}
			else if (m_PathState >= ePathState.Fail_Unknown)
			{
				// catch all unknown fail.
				Debug.LogError(GetType().Name + " search path fail :" + m_PathState.ToString("F") + ", drop request.", m_Agent);
				m_PathState = ePathState.Idle;
				return WeightVector.zero;
			}

			throw new System.InvalidProgramException(GetType().Name + " Unknow state.");
		}

		public class WaypointParams
		{
			private readonly FlyAgent m_Agent;
			private readonly Pilot m_Pilot;
			public List<Bounds> m_WayPoints = new List<Bounds>();

			private Vector3 _ApproachingDirection;
			/// <summary>The current waypoint/goal's direction</summary>
			public Vector3 m_ApproachingDirection
			{
				get { return _ApproachingDirection; }
				set {
					_ApproachingDirection = value;
					m_ApproachingDirection_Normalized = _ApproachingDirection.normalized;
					m_EstimateDistanceSqr = _ApproachingDirection.sqrMagnitude;
				}
			}
			public Vector3 m_ApproachingDirection_Normalized { get; private set; }
			/// <summary>estimated distance to next waypoint or destination.</summary>
			public float m_EstimateDistanceSqr { get; private set; }

			/// <summary>Only valid during <see cref="ePathState.Patrol_Waypoint"/>
			/// and more then one waypoint</summary>
			public Vector3 m_NextDirection
			{
				get { return _NextDirection; }
				set
				{
					_NextDirection = value;
					m_NextDirection_Normalized = _NextDirection.normalized;
				}
			}
			public Vector3 m_NextDirection_Normalized { get; private set; }
			private Vector3 _NextDirection;

			/// <summary>Only valid during <see cref="ePathState.Patrol_Waypoint"/></summary>
			public float m_PredictiveTurnAngle;
			public Vector3 m_PredictiveTurnAngleNormal;
			
			public WaypointParams(FlyAgent agent, Pilot pilot)
			{
				m_Agent = agent;
				m_Pilot = pilot;
			}

			public void Reset()
			{
				m_WayPoints.Clear();
			}

			public void SetWaypoints(IEnumerable<Bounds> waypoints)
			{
				m_WayPoints = new List<Bounds>(waypoints);
			}

			public bool HasPath()
			{
				return m_WayPoints != null && m_WayPoints.Count > 0;
			}

			public void Update()
			{
				// check if we need to go farther later on,
				TravelWaypoint();

				// check if current waypoint can good enough to drop.
				if (ShouldHeadingNextWaypoint())
					m_WayPoints.RemoveAt(0);
			}


			/// <summary>
			/// To predict agent movement between current & target waypoint position.
			/// and apply angle correction if we needed to do a quick turn.
			/// based on turning deviation <see cref="PilotConfig.m_AngleDeviationThreshold"/>
			/// </summary>
			private void TravelWaypoint()
			{
				// got another path later on. do some math
				Vector3 p0 = m_WayPoints[0].center;
				bool reachNode = m_WayPoints[0].Contains(m_Agent.CurrentFrameCache.position);

				m_ApproachingDirection = p0 - m_Agent.CurrentFrameCache.position;

				// the error angle of current waypoint direction and forward angle
				float dotForwardErrorAngle = Vector3.Dot(m_ApproachingDirection_Normalized, m_Agent.CurrentFrameCache.forward.normalized);

				if (m_WayPoints.Count > 1)
				{
					m_NextDirection = m_WayPoints[1].center - p0;
					m_PredictiveTurnAngle = Vector3.Dot(m_ApproachingDirection_Normalized, m_NextDirection_Normalized);
					m_PredictiveTurnAngleNormal = Vector3.Cross(m_ApproachingDirection, m_NextDirection_Normalized);
					// deviation, perfect alignment = 1f
					// to check the angle are big enough to consider it's corner.
					if (!reachNode && // moving toward p0, but not yet reach
						dotForwardErrorAngle >= m_Pilot.m_Config.m_AngleDeviationThreshold) // and facing p0
					{
						// Expect the turning later on, prepare for it.
						Vector3 inverseTangentDir = -m_NextDirection_Normalized * (m_Agent.m_Radius * 2f);
						// Debug.DrawRay(p0, inverseTangentDir, Color.yellow, 0.3f, false);
						Vector3 predictAgentPos = m_Agent.CurrentFrameCache.position + m_Agent.CurrentFrameCache.velocity;
						Vector3 predictMovementDir = m_Agent.CurrentFrameCache.deltaPosition;
						if (predictMovementDir.sqrMagnitude < m_ApproachingDirection.sqrMagnitude)
						{
							// before we close enough to turn, we aim for outer tangent point.
							// limited within bound box (empty space).
							Vector3 waypointPredictCorrection = m_WayPoints[0].ClosestPoint(p0 + inverseTangentDir);
							m_ApproachingDirection = waypointPredictCorrection - m_Agent.CurrentFrameCache.position;
							// Debug.DrawLine(p0, waypointPredictCorrection, Color.green, 0.3f, false);
						}
					}
				}
				else
				{
					// less then 2 way point, can not do any turning.
					m_NextDirection = Vector3.zero;
					m_PredictiveTurnAngle = 0f;
					m_PredictiveTurnAngleNormal = Vector3.zero;
				}

				if (!reachNode)
				{
					// when not facing to current destination point,
					// slow down by the angle different, but not too close to zero.
					m_ApproachingDirection *= Mathf.Clamp(dotForwardErrorAngle, 0.3f, 1f);
				}
			}

			/// <summary>To design should head to the next waypoint.</summary>
			/// <returns>true = should move.</returns>
			private bool ShouldHeadingNextWaypoint()
			{
				// arrive the node contain agent, if space large enough
				bool withInNode = m_WayPoints[0].Contains(m_Agent.CurrentFrameCache.position);
				
				if (m_WayPoints.Count > 1)
				{
					// developer bias, choose to slow down within this distance.
					Bounds agentBounds = m_Agent.Size.GetBounds();

					bool onMoveCloseEnough = m_EstimateDistanceSqr < m_Agent.m_BrakingDistance * m_Agent.m_BrakingDistance;

					// we still have next point to go.
					if (withInNode && m_WayPoints[0].IsFullyEncapsulate(agentBounds))
					{
						return true;
					}
					else if (withInNode || onMoveCloseEnough)
					{
						// TODO: fix following cases
						// case 1) path node are too small - moving around the node until we reach.
						// case 2) braking distance are too large - may cancel waypoint too early before pass via the corner
						float alignNextDirectionDeviation = Vector3.Dot(m_Agent.CurrentFrameCache.forward.normalized, m_NextDirection_Normalized);
						bool facingNextPoint = alignNextDirectionDeviation > m_Pilot.m_Config.m_AngleDeviationThreshold;

						// And we are pointing to next waypoint,
						// so we can drop the pervious one. instead of move backward.
						// to smooth the movement.
						return facingNextPoint;
					}
					return false;
				}
				else // if (cnt <= 1)
				{
					// developer bias, choose to stop at this distance.
					bool onArriveCloseEnough = m_EstimateDistanceSqr < m_Agent.CurrentFrameCache.arrivedDistanceSqr;

					return withInNode || onArriveCloseEnough;
				}
			}

		}

		/// <summary>
		/// Direct move to position, without any waypoint.
		/// </summary>
		/// <returns></returns>
		private WeightVector _DirectTravelVector()
		{
			if (m_WaypointParams.HasPath())
				m_WaypointParams.Reset();
			if (m_PathState != ePathState.Patrol_SameNode)
				m_PathState = ePathState.Patrol_SameNode;

			m_WaypointParams.m_ApproachingDirection = m_GoalPoint - m_Agent.CurrentFrameCache.position;
			if (m_WaypointParams.m_EstimateDistanceSqr < m_Agent.CurrentFrameCache.arrivedDistanceSqr)
				Reset();
			return new WeightVector(m_WaypointParams.m_ApproachingDirection, DESTINATION_WEIGHT);
		}
	}
}
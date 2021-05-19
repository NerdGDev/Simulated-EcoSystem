using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlyAgent.Navigation;
using FlyAgent.Utilities;
using FlyAgent.Agents;

namespace FlyAgent.Navigation
{
	public partial class Octree
	{
		/// <summary>To define EACH obstacle can trigger node split O(8 ^ n) in 1 update.</summary>
		const int NODE_SPLIT_QUOTA = 3;
		/// <summary>To define how many level can merge in 1 update. O(8 ^ n) </summary>
		const int NODE_MERGE_QUOTA = 1;

		// Should be a value between 1 and 2. A multiplier for the base size of a node.
		// 1.0 is a "normal" octree, while values > 1 have overlap
		readonly float m_Looseness;

		// Size that the octree was on creation
		readonly float m_InitialSize;

		// Minimum side length that a node can be - essentially an alternative to having a max depth
		readonly float m_MinSize;
		// For collision visualisation. Automatically removed in builds.
#if UNITY_EDITOR
		const int NUM_COLLISIONS_TO_SAVE = 4;
		readonly Queue<Bounds> m_LastBoundsCollisionChecks = new Queue<Bounds>();
		readonly Queue<Ray> m_LastRayCollisionChecks = new Queue<Ray>();
#endif

		// The total amount of objects currently in the tree
		public int Count { get; private set; }

		// Root node of the octree
		public Node rootNode { get; private set; }

		// Reference all the obstacle and update their state
		public List<Obstacle> m_Obstacles = new List<Obstacle>();

		// to bake map async, we require to reduce the amount to explore for each obstacle
		// we cache the unfinish job here to separate the effort to locate all edge of obstacle. 
		private Dictionary<Obstacle, SurfaceReconstructionRequest> m_SplitRequest = null;
		private PriorityQueue<MergeRequest> m_MergeRequest = null;

		public event System.Action EVENT_SizeChanged;
		public event System.Action<Node> EVENT_Split;
		public event System.Action<Node> EVENT_Merge;

		private int m_TotalDepth = 0;
		private bool m_DepthChanged = true;
		public int totalDepth
		{
			get
			{
				if (m_DepthChanged)
				{
					m_DepthChanged = false;
					rootNode.GetMaxDepth(ref m_TotalDepth);
				}
				return m_TotalDepth;
			}
		}

		/// <summary>
		/// Constructor for the bounds octree.
		/// </summary>
		/// <param name="initialWorldSize">Size of the sides of the initial node, in metres. The octree will never shrink smaller than this.</param>
		/// <param name="initialWorldPos">Position of the centre of the initial node.</param>
		/// <param name="minNodeSize">Nodes will stop splitting if the new nodes would be smaller than this (metres).</param>
		/// <param name="loosenessVal">Clamped between 1 and 2. Values > 1 let nodes overlap.</param>
		public Octree(float initialWorldSize, Vector3 initialWorldPos, float minNodeSize, float loosenessVal)
		{
			if (minNodeSize > initialWorldSize)
			{
				Debug.LogWarning("Minimum node size must be at least as big as the initial world size. Was: " + minNodeSize + " Adjusted to: " + initialWorldSize);
				minNodeSize = initialWorldSize;
			}
			Count = 0;
			m_InitialSize = initialWorldSize;
			m_MinSize = minNodeSize;
			m_Looseness = Mathf.Clamp(loosenessVal, 1.0f, 2.0f);
			rootNode = new Node(this, null, m_InitialSize, m_MinSize, loosenessVal, initialWorldPos);
			Obstacle.EVENT_Removed += Remove;
			Obstacle.EVENT_Updated += OnObstacleUpdated;

			m_SplitRequest = new Dictionary<Obstacle, SurfaceReconstructionRequest>();
			m_MergeRequest = new PriorityQueue<MergeRequest>();
		}

		~Octree()
		{
			Obstacle.EVENT_Removed -= Remove;
			Obstacle.EVENT_Updated -= OnObstacleUpdated;
			m_SplitRequest = null;
			m_MergeRequest = null;
		}

		
		/// <summary>Add an object with collider.</summary>
		/// <param name="collider">Object collider to add.</param>
		public void Add(Collider collider)
		{
			if (collider.isTrigger)
				return;
			Add(new Obstacle(collider));
		}

		public void Add(Obstacle obstacle)
		{
			// since rootNode store all obstacles as same as m_Obstacles.
			if (!obstacle.IsValid)
				return;

			if (rootNode.Contains(obstacle))
				return;
			
			if (obstacle.collider.GetComponent<FlyAgentBase>() != null)
				return; // it's our flight agent.

			// Add object or expand the octree until it can be added
			int count = 0; // Safety check against infinite/excessive growth

			m_SplitRequest.Add(obstacle, new SurfaceReconstructionRequest()
			{
				m_NodeRequest = new List<ObstacleInsertionInfo>(32),
				m_Obstacle = obstacle,
				m_SpiltQuota = NODE_SPLIT_QUOTA,
			});
			while (!rootNode.Add(obstacle))
			{
				Grow(obstacle.bounds.center - rootNode.center);
				if (++count > 20)
				{
					Debug.LogError("Aborted Add operation as it seemed to be going on forever (" + (count - 1) + ") attempts at growing the octree.");
					return;
				}
			}
			
			m_Obstacles.Add(obstacle);
			Count++;
		}

		/// <summary>passive call to remove the obstacle in the list</summary>
		/// <param name="obstacle"></param>
		public void Remove(Obstacle obstacle)
		{
			if (m_Obstacles.Remove(obstacle))
			{
				rootNode.Remove(obstacle);
				Count--;
				Shrink();
			}
		}

		private void OnObstacleUpdated(Obstacle obstacle)
		{
			rootNode.CheckObstacleRemainPosition(obstacle);
		}

		public void StartPeriodicUpdate(MonoBehaviour mb)
		{
			mb.StartCoroutine(UpdateSurfaceReconstruct());
		}

		private IEnumerator UpdateSurfaceReconstruct()
		{
			while (true)
			{
				float timeSinceLevelLoad = Time.timeSinceLevelLoad;

				// Merge request
				ContinueMergeRequest();

				int cnt = m_Obstacles.Count;
				while (cnt-- > 0)
				{
					// Surface reconstruct
					ContinueObstacleSplitRequest(m_Obstacles[cnt]);

					// Obstacle movement test
					m_Obstacles[cnt].StateCheck(timeSinceLevelLoad);
					yield return new WaitForFixedUpdate();
				}
				yield return new WaitForFixedUpdate();
			}
		}

		private void ContinueMergeRequest()
		{
			if (m_MergeRequest.Count() == 0)
				return;

			int quota = NODE_MERGE_QUOTA;
			while (quota > 0 && m_MergeRequest.Count() > 0)
			{
				MergeRequest request = m_MergeRequest.Dequeue();
				if (request.IsVaild)
				{
					if (request.m_AllowMerge())
					{
						request.m_Callback();
						quota--;
						// Kit.Extend.DebugExtend.DrawBounds(request.m_Node.bounds, Color.green, 0.1f, false);
					}
					// TODO: make it depend on something and re-trigger if possible.
					//else // just cancel that request.
					//{
					//	Kit.Extend.DebugExtend.DrawBounds(request.m_Node.bounds, Color.magenta, 0.1f, false);
					//}
				}
			}
		}

		private void ContinueObstacleSplitRequest(Obstacle obstacle)
		{
			if (!obstacle.IsValid)
			{
				m_SplitRequest.Remove(obstacle);
				return;
			}

			if (m_SplitRequest.Count == 0)
				return; // common case.

			SurfaceReconstructionRequest request;
			if (m_SplitRequest.TryGetValue(obstacle, out request))
			{
				// let's reset the quota.
				request.m_SpiltQuota = NODE_SPLIT_QUOTA;

				if (request.m_NodeRequest.Count > 0)
				{
					// sort by size, bigger first.
					request.m_NodeRequest.Sort((x, y) => {
						return -1 * x.m_Node.baseLength.CompareTo(y.m_Node.baseLength);
					});

					// during this loop, the m_SpiltQuota can also run out,
					// because split() will trigger right after we add obstacle in it.
					int quota = NODE_SPLIT_QUOTA;
					while (quota-- > 0 &&
						request.m_SpiltQuota > 0 &&
						request.m_NodeRequest.Count > 0)
					{
						// continue split & add task process.
						// Request from different node + obstacle, but share the same node space.
						if (request.m_NodeRequest[0].m_Node == null)
							quota++; // node destroy before request finish.
						else
							request.m_NodeRequest[0].m_ContinueInternalSplitCallback(obstacle);
						request.m_NodeRequest.RemoveAt(0); // remove node in record.
					}
				}
				else if (request.m_NodeRequest.Count == 0 && request.m_SpiltQuota == NODE_SPLIT_QUOTA)
				{
					// all node was explored this task can be removed.
					m_SplitRequest.Remove(obstacle);
				}
			}
			// else no request for that obstacle
		}

		/// <summary>
		/// Check if the specified bounds intersect with anything in the tree. See also: GetColliding.
		/// </summary>
		/// <param name="checkBounds">bounds to check.</param>
		/// <returns>True if there was a collision.</returns>
		public bool IsColliding(Bounds checkBounds)
		{
			AddCollisionDebug(checkBounds);
			return rootNode.bounds.Intersects(checkBounds);
		}

		/// <summary>
		/// Check if the specified ray intersects with anything in the tree. See also: GetColliding.
		/// </summary>
		/// <param name="checkRay">ray to check.</param>
		/// <param name="maxDistance">distance to check.</param>
		/// <returns>True if there was a collision.</returns>
		public bool IsColliding(Ray checkRay, float maxDistance = float.PositiveInfinity)
		{
			AddCollisionDebug(checkRay);
			float distance;
			return rootNode.bounds.IntersectRay(checkRay, out distance) && distance <= maxDistance;
		}

		/// <summary>
		/// Returns an array of objects that intersect with the specified bounds, if any. Otherwise returns an empty array. See also: IsColliding.
		/// </summary>
		/// <param name="collidingWith">list to store intersections.</param>
		/// <param name="checkBounds">bounds to check.</param>
		/// <returns>Objects that intersect with the specified bounds.</returns>
		public void GetCollidingLeafNode(List<Node> collidingWith, Bounds checkBounds)
		{
			AddCollisionDebug(checkBounds);
			rootNode.GetCollidingLeafNode(ref checkBounds, collidingWith);
		}

		public void GetNeighborsColliding(List<Node> collidingWith, Bounds checkBounds, out Node anchor)
		{
			AddCollisionDebug(checkBounds);
			rootNode.GetNeighborsColliding(collidingWith, ref checkBounds, out anchor);
		}

		public void GetObstacleShape(Obstacle obstacle, HashSet<Node> volume)
		{
			rootNode.GetObstacleShape(obstacle, volume);
		}

		/// <summary>
		/// Returns an array of objects that intersect with the specified ray, if any. Otherwise returns an empty array. See also: IsColliding.
		/// </summary>
		/// <param name="collidingWith">list to store intersections.</param>
		/// <param name="checkRay">ray to check.</param>
		/// <param name="maxDistance">distance to check.</param>
		/// <returns>Objects that intersect with the specified ray.</returns>
		public void GetColliding(List<Node> collidingWith, Ray checkRay, float maxDistance = float.PositiveInfinity)
		{
			// For debugging
			AddCollisionDebug(checkRay);
			rootNode.GetCollidingLeafNode(ref checkRay, collidingWith, maxDistance);
		}

		public Bounds GetMaxBounds()
		{
			return rootNode.bounds;
		}

		/// <summary>
		/// Draws node boundaries visually for debugging.
		/// Must be called from OnDrawGizmos externally. See also: DrawAllObjects.
		/// </summary>
		public void DrawAllBounds(float alpha, int requestDepth = 0)
		{
			Color oldColor = Gizmos.color;
			requestDepth = Mathf.Clamp(requestDepth, 0, totalDepth);
			rootNode.DrawAllBounds(alpha, 0f, requestDepth);
			Gizmos.color = oldColor;
		}

		/// <summary>
		/// Draws the bounds of all objects in the tree visually for debugging.
		/// Must be called from OnDrawGizmos externally. See also: DrawAllBounds.
		/// </summary>
		public void DrawCost(float alpha)
		{
			rootNode.DrawCost(alpha);
		}

		// Intended for debugging. Must be called from OnDrawGizmos externally
		// See also DrawAllBounds and DrawAllObjects
		/// <summary>
		/// Visualises collision checks from IsColliding and GetColliding.
		/// Collision visualisation code is automatically removed from builds so that collision checks aren't slowed down.
		/// </summary>
		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public void DrawCollisionChecks()
		{
#if UNITY_EDITOR
			Color oldColor = Gizmos.color;
			int count = 0;
			foreach (Bounds collisionCheck in m_LastBoundsCollisionChecks)
			{
				Gizmos.color = new Color(1.0f, 1.0f - ((float) count / NUM_COLLISIONS_TO_SAVE), 1.0f);
				Gizmos.DrawCube(collisionCheck.center, collisionCheck.size);
				count++;
			}

			foreach (Ray collisionCheck in m_LastRayCollisionChecks)
			{
				Gizmos.color = new Color(1.0f, 1.0f - ((float) count / NUM_COLLISIONS_TO_SAVE), 1.0f);
				Gizmos.DrawRay(collisionCheck.origin, collisionCheck.direction);
				count++;
			}
			Gizmos.color = oldColor;
#endif
		}

		// #### PRIVATE METHODS ####

		/// <summary>
		/// Used for visualising collision checks with DrawCollisionChecks.
		/// Automatically removed from builds so that collision checks aren't slowed down.
		/// </summary>
		/// <param name="checkBounds">bounds that were passed in to check for collisions.</param>
		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		void AddCollisionDebug(Bounds checkBounds)
		{
#if UNITY_EDITOR
			m_LastBoundsCollisionChecks.Enqueue(checkBounds);
			if (m_LastBoundsCollisionChecks.Count > NUM_COLLISIONS_TO_SAVE)
			{
				m_LastBoundsCollisionChecks.Dequeue();
			}
#endif
		}

		/// <summary>
		/// Used for visualising collision checks with DrawCollisionChecks.
		/// Automatically removed from builds so that collision checks aren't slowed down.
		/// </summary>
		/// <param name="checkRay">ray that was passed in to check for collisions.</param>
		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		void AddCollisionDebug(Ray checkRay)
		{
#if UNITY_EDITOR
			m_LastRayCollisionChecks.Enqueue(checkRay);
			if (m_LastRayCollisionChecks.Count > NUM_COLLISIONS_TO_SAVE)
			{
				m_LastRayCollisionChecks.Dequeue();
			}
#endif
		}

		/// <summary>
		/// Grow the octree to fit in all objects.
		/// </summary>
		/// <param name="direction">Direction to grow.</param>
		void Grow(Vector3 direction)
		{
			m_DepthChanged = true;
			int xDirection = direction.x >= 0 ? 1 : -1;
			int yDirection = direction.y >= 0 ? 1 : -1;
			int zDirection = direction.z >= 0 ? 1 : -1;
			Node oldRoot = rootNode;
			float half = rootNode.baseLength * 0.5f;
			float newLength = rootNode.baseLength * 2;
			Vector3 newCenter = rootNode.center + new Vector3(xDirection * half, yDirection * half, zDirection * half);

			// Create a new, bigger octree root node
			rootNode = new Node(this, null, newLength, m_MinSize, m_Looseness, newCenter);

			if (oldRoot.HasObstacle())
			{
				// Create 7 new octree children to go with the old root as children of the new root
				int rootPos = OctreeUtilities.GetRootPosIndex(xDirection, yDirection, zDirection);
				Node[] children = new Node[8];
				for (int i = 0; i < 8; i++)
				{
					if (i == rootPos)
					{
						children[i] = oldRoot;
						oldRoot.SetParent(rootNode);
					}
					else
					{
						xDirection = i % 2 == 0 ? -1 : 1;
						yDirection = i > 3 ? -1 : 1;
						zDirection = (i < 2 || (i > 3 && i < 6)) ? -1 : 1;
						children[i] = new Node(this, rootNode, rootNode.baseLength, m_MinSize, m_Looseness, newCenter + new Vector3(xDirection * half, yDirection * half, zDirection * half));
					}
				}

				// Attach the new children to the new root node
				rootNode.SetChildren(children);
			}
			rootNode.UpdateNeighborsIfNeed();
			if (EVENT_SizeChanged != null)
				EVENT_SizeChanged();
		}

		/// <summary>
		/// Shrink the octree if possible, else leave it the same.
		/// </summary>
		void Shrink()
		{
			m_DepthChanged = true;
			Node tmp = rootNode.ShrinkIfPossible(m_InitialSize);
			if (rootNode != tmp)
			{
				rootNode = tmp;
				rootNode.UpdateNeighborsIfNeed();
				if (EVENT_SizeChanged != null)
					EVENT_SizeChanged();
			}
		}

		/// <summary>when node are going to split 8 childrens</summary>
		/// <param name="node">the parent node, will going to CREATE nodes</param>
		private void OnChildNodeSplit(Node node)
		{
			if (EVENT_Split != null)
				EVENT_Split(node);
		}

		/// <summary>when node's children are going to merge into this node(parent)</summary>
		/// <param name="node">the node's children will going to REMOVED</param>
		private void OnChildNodeMerge(Node node)
		{
			if (EVENT_Merge != null)
				EVENT_Merge(node);
		}
	}
}
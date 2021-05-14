// #define DEVIATION_DEBUG
// #define Draw_InternalNeighborsColliding
#define BACKGROUND_THREAD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Kit;

namespace FlyAgent.Navigation
{
	public partial class Octree
	{
		public class Node : System.IComparable<Node>, IPFVertex<Node>, System.IEquatable<Node>
		{
			/// <summary>Bounding box that represents this node.</summary>
			public Bounds bounds { get; protected set; }
			private int m_BoundsHashCode;

			public Vector3 center { get { return bounds.center; } }
			
			// Length of this node if it has a looseness of 1.0
			private float m_BaseLength;
			public float baseLength { get { return m_BaseLength; } private set { m_BaseLength = value; } }

			// TODO: do we need this ?
			// Looseness value for this node
			float m_Looseness;

			// Minimum size for a node in this octree
			float m_MinSize;

			// Actual length of sides, taking the looseness value into account
			float m_AdjLength;

			// Child nodes, if any
			private Node[] m_Children = null;

			// Bounds of potential children to this node. These are actual size (with looseness taken into account), not base size
			private Bounds[] m_ChildBoundsCache;

			/// <summary>Get a list of bounds perfect matching <see cref="eNeighbor"/> order</summary>
			/// <returns>list of bounds</returns>
			private Bounds[] m_DirectionBoundsCache
			{
				get
				{
					if (_DirectionBoundsCache == null)
						_DirectionBoundsCache = OctreeUtilities.GetDirectionBounds(this);
					return _DirectionBoundsCache;
				}
			}
			private Bounds[] _DirectionBoundsCache;

			public readonly Octree m_Root;

			public Node m_Parent { get; protected set; }

			private List<Node>[] m_NeighborsCache = null; // 0~25 + 26
			private Vector3[] m_Vertices = null;

			// Objects in this node, a get/set wrapper for m_Data
			private List<Obstacle> m_Obstacles = new List<Obstacle>(10);
			
			// TODO: do we still need to known depth within the octree ?
			public int Depth { get { return (m_Parent == null) ? 1 : m_Parent.Depth + 1; } }

			private static readonly Vector2 m_MergeDelayRange = new Vector2(8f, 30f);
			private float m_MergeUnlockDuration = m_MergeDelayRange.x;
			private ExpiringLock m_MergeRequestLock;

#if UNITY_EDITOR
			private static readonly Color COLLISION_CHECK_COLOR = new Color(1f, 0f, 0f, 40f / 255f);
#endif

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="baseLengthVal">Length of this node, not taking looseness into account.</param>
			/// <param name="minSizeVal">Minimum size of nodes in this octree.</param>
			/// <param name="loosenessVal">Multiplier for baseLengthVal to get the actual size.</param>
			/// <param name="centerVal">Centre position of this node.</param>
			public Node(Octree rootVal, Node parent, float baseLengthVal, float minSizeVal, float loosenessVal, Vector3 centerVal)
			{
				m_Root = rootVal;
				m_Parent = parent;
				m_MergeRequestLock = new ExpiringLock(m_MergeUnlockDuration, false);
				SetValues(baseLengthVal, minSizeVal, loosenessVal, centerVal);
			}

			~Node()
			{
				m_NeighborsCache = null;
			}

			/// <summary>Set parent of this node</summary>
			/// <param name="parent"></param>
			public void SetParent(Node parent)
			{
				m_Parent = parent;
				// parent should contain all the obstacle that I have.
				if (m_Parent != null && m_Obstacles != null)
				{
					if (m_Parent.m_Obstacles == null)
						m_Parent.m_Obstacles = new List<Obstacle>(m_Obstacles);
					else
					{
						HashSet<Obstacle> mirror = new HashSet<Obstacle>(m_Parent.m_Obstacles);
						int cnt = m_Obstacles.Count;
						while (cnt-- > 0)
						{
							if (!mirror.Contains(m_Obstacles[cnt]))
								m_Parent.m_Obstacles.Add(m_Obstacles[cnt]);
						}
						mirror.Clear();
					}
				}
			}

			/// <summary>Set the 8 children of this node.</summary>
			/// <param name="childOctrees">The 8 new child nodes.</param>
			public void SetChildren(Node[] childOctrees)
			{
				if (childOctrees.Length != 8)
				{
					Debug.LogError("Child octree array must be length 8. Was length: " + childOctrees.Length);
					return;
				}

				m_Children = childOctrees;
			}

			public Node[] GetChildren()
			{
				if (m_Children == null)
					return new Node[0];
				return m_Children;
			}

			/// <summary>Add an obstacle.</summary>
			/// <param name="obstacle">the obstacle</param>
			/// <returns>True if the object fits entirely within this node.</returns>
			public bool Add(Obstacle obstacle)
			{
				if (!bounds.IsFullyEncapsulate(obstacle.bounds))
				{
					return false;
				}
				InternalAdd(obstacle, false);
				return true;
			}

			/// <summary>Remove the obstacle in this node and all it's children nodes.</summary>
			/// <param name="obstacle"></param>
			/// <returns></returns>
			public bool Remove(Obstacle obstacle)
			{
				if (m_Obstacles == null)
					return false;

				return InternalRemove(obstacle);
			}

			public bool Contains(Obstacle obstacle)
			{
				if (m_Obstacles == null)
					return false;
				return m_Obstacles.Contains(obstacle);
			}

			/// <summary>
			/// Returns an array of objects that intersect with the specified bounds, if any. Otherwise returns an empty array. See also: IsColliding.
			/// </summary>
			/// <param name="checkBounds">Bounds to check. Passing by ref as it improves performance with structs.</param>
			/// <param name="result">List result.</param>
			/// <returns>Objects that intersect with the specified bounds.</returns>
			public void GetCollidingLeafNode(ref Bounds checkBounds, List<Node> result)
			{
				// Are the input bounds at least partially in this node?
				if (!bounds.Intersects(checkBounds))
					return;

				if (bounds.Intersects(checkBounds))
				{
					if (m_Children == null)
					{
						result.Add(this);
					}
					else
					{
						for (int i = 0; i < 8; i++)
						{
							m_Children[i].GetCollidingLeafNode(ref checkBounds, result);
						}
					}
				}
			}

			/// <summary>
			/// Returns an array of objects that intersect with the specified ray, if any. Otherwise returns an empty array. See also: IsColliding.
			/// </summary>
			/// <param name="checkRay">Ray to check. Passing by ref as it improves performance with structs.</param>
			/// <param name="maxDistance">Distance to check.</param>
			/// <param name="result">List result.</param>
			/// <returns>Objects that intersect with the specified ray.</returns>
			public void GetCollidingLeafNode(ref Ray checkRay, List<Node> result, float maxDistance = float.PositiveInfinity)
			{
				float distance;
				// Is the input ray at least partially in this node?
				if (!bounds.IntersectRay(checkRay, out distance) || distance > maxDistance)
					return;

				if (bounds.IntersectRay(checkRay, out distance) && distance <= maxDistance)
				{
					if (m_Children == null)
					{
						result.Add(this);
					}
					else
					{
						for (int i = 0; i < 8; i++)
						{
							m_Children[i].GetCollidingLeafNode(ref checkRay, result, maxDistance);
						}
					}
				}
			}

			/// <summary>Get neighbors which colliding to giving reference</summary>
			/// <param name="neighbors">list of neighbors</param>
			/// <param name="obj">reference object</param>
			/// <param name="checkBounds">reference bounds area.</param>
			public void GetNeighborsColliding(List<Node> neighbors, ref Bounds checkBounds, out Node anchor)
			{
				// Are the input bounds at least partially in this node?
				if (!bounds.IsFullyEncapsulate(checkBounds))
				{
					anchor = null;
					return;
				}

				anchor = LocateBestFitChild(ref checkBounds, IsLeaf: true);
				neighbors.AddRange(anchor.GetNeighbors(eNeighbor.All));
			}

			public void GetObstacleShape(Obstacle obstacle, HashSet<Node> volume)
			{
				if (m_Obstacles == null || !m_Obstacles.Contains(obstacle))
					return;

				if (m_Children != null)
				{
					for (int i = 0; i < 8; i++)
					{
						m_Children[i].GetObstacleShape(obstacle, volume);
					}
				}
				else
				{
					volume.Add(this);
				}
			}

			/// <summary>Assume parent are fully encapsulate checkBounds.</summary>
			/// <param name="checkBounds"></param>
			/// <param name="IsLeaf">to take leaf node or best contain node.</param>
			/// <returns></returns>
			private Node LocateBestFitChild(ref Bounds checkBounds, bool IsLeaf)
			{
				if (m_Children != null)
				{
					int bestFitChild = (int)this.BestFitChild(checkBounds);
					bool contain = IsLeaf ?
						m_Children[bestFitChild].bounds.Intersects(checkBounds) :
						m_Children[bestFitChild].bounds.IsFullyEncapsulate(checkBounds);
					if (contain)
					{
						// Go deeper
						return m_Children[bestFitChild].LocateBestFitChild(ref checkBounds, IsLeaf);
					}
				}
				return this;
			}

			private void InternalNeighborsColliding(List<Node> result, ref Bounds checkBounds, Node anchor)
			{
				// Profiler.BeginSample("InternalNeighborsColliding");
				// Are the input bounds at least partially in this node?
				if (this == anchor || !bounds.Intersects(checkBounds))
				{
					// do nothing. Profiler end.
				}
				else if (m_Children == null)
				{
					// leaf node
					result.Add(this);

#if UNITY_EDITOR && Draw_InternalNeighborsColliding
					// for visualize the locate neighbors process in editor.
					DebugExtend.DrawBounds(bounds, COLLISION_CHECK_COLOR, .3f, false);
#endif
				}
				else
				{
					// pass to children
					for (int i = 0; i < 8; i++)
					{
						m_Children[i].InternalNeighborsColliding(result, ref checkBounds, anchor);
					}
				}
				// Profiler.EndSample();
			}

			public Vector3[] GetVertices(eNeighbor tag)
			{
				if (m_Vertices == null)
				{
					Vector3 extents = bounds.extents;
					Vector3
						ufl = center + extents,
						ufr = center + new Vector3(-extents.x, extents.y, extents.z),
						ubl = center + new Vector3(-extents.x, extents.y, -extents.z),
						ubr = center + new Vector3(extents.x, extents.y, -extents.z),
						dfl = center + new Vector3(extents.x, -extents.y, extents.z),
						dfr = center + new Vector3(-extents.x, -extents.y, extents.z),
						dbl = center - extents,
						dbr = center + new Vector3(extents.x, -extents.y, -extents.z);

					m_Vertices = new Vector3[]
					{
					ufl,ufr,ubl,ubr, // 0123
					dfl,dfr,dbl,dbr, // 4567
					};
				}

				int no = (int)tag;
				if (tag == eNeighbor.All)
					return m_Vertices;
				else if (no >= 18) // specify vertex
					return new[] { m_Vertices[no - 18] };
				else
				{
					switch (tag)
					{
						// Edge
						case eNeighbor.UF:
						return new[] { m_Vertices[0], m_Vertices[1] };
						case eNeighbor.UB:
						return new[] { m_Vertices[2], m_Vertices[3] };
						case eNeighbor.UL:
						return new[] { m_Vertices[0], m_Vertices[2] };
						case eNeighbor.UR:
						return new[] { m_Vertices[1], m_Vertices[3] };
						case eNeighbor.DF:
						return new[] { m_Vertices[4], m_Vertices[5] };
						case eNeighbor.DB:
						return new[] { m_Vertices[6], m_Vertices[7] };
						case eNeighbor.DL:
						return new[] { m_Vertices[4], m_Vertices[6] };
						case eNeighbor.DR:
						return new[] { m_Vertices[5], m_Vertices[7] };
						case eNeighbor.LF:
						return new[] { m_Vertices[0], m_Vertices[4] };
						case eNeighbor.LB:
						return new[] { m_Vertices[2], m_Vertices[6] };
						case eNeighbor.RF:
						return new[] { m_Vertices[1], m_Vertices[5] };
						case eNeighbor.RB:
						return new[] { m_Vertices[3], m_Vertices[7] };
						// Face
						case eNeighbor.U:
						return new[] { m_Vertices[0], m_Vertices[1], m_Vertices[2], m_Vertices[3] };
						case eNeighbor.D:
						return new[] { m_Vertices[4], m_Vertices[5], m_Vertices[6], m_Vertices[7] };
						case eNeighbor.F:
						return new[] { m_Vertices[0], m_Vertices[1], m_Vertices[4], m_Vertices[5] };
						case eNeighbor.B:
						return new[] { m_Vertices[2], m_Vertices[3], m_Vertices[6], m_Vertices[7] };
						case eNeighbor.L:
						return new[] { m_Vertices[0], m_Vertices[2], m_Vertices[4], m_Vertices[6] };
						case eNeighbor.R:
						return new[] { m_Vertices[1], m_Vertices[3], m_Vertices[5], m_Vertices[7] };
						default:
						throw new System.InvalidProgramException();
					}
				}
			}

			public List<Node> GetNeighbors(eNeighbor tag)
			{
				if (m_NeighborsCache == null) // no cache found.
				{
#if !BACKGROUND_THREAD
					Profiler.BeginSample("GetNeighbors");
#endif
					if (m_Root.rootNode == this) // root node request.
						return new List<Node>(0); // impossible to having neighbors.
					
					// alloc memory
					// perfect matching eNeighbor order
					m_NeighborsCache = new List<Node>[27];
					for (int i = 0; i < 27; i++)
					{
						// each side have 1 or 4 multiple nodes are collide.
						// cell[26] are speical case to contain all neighbors.
						m_NeighborsCache[i] = new List<Node>(i < 26 ? 4 : 64);
					}

					// search for neighbor(s)
					// extend bounds a little bit to ENSURE AABB can detect collision.
					Bounds extendBounds = new Bounds(bounds.center, bounds.size * (1f + float.Epsilon));

					if (m_Parent.m_NeighborsCache == null)
						LookUpNeighborsFromRoot(ref extendBounds);
					else
						LookUpNeighborsFromParent(ref extendBounds);

#if !BACKGROUND_THREAD
					Profiler.EndSample();
#endif
				}
				return m_NeighborsCache[(int)tag];
			}

			/// <summary>our parent didn't have neighbor records, we manage to search for our own.</summary>
			/// <param name="extendBounds"></param>
			private void LookUpNeighborsFromRoot(ref Bounds extendBounds)
			{
#if !BACKGROUND_THREAD
				Profiler.BeginSample("LookUpNeighborsFromRoot");
#endif
				List<Node> data = new List<Node>(64);
				m_Root.rootNode.InternalNeighborsColliding(data, ref extendBounds, this);
				LookUpNeighborsFromData(ref extendBounds, data);
#if !BACKGROUND_THREAD
				Profiler.EndSample();
#endif
			}

			/// <summary>Parent's neighbors + childrens - this cell, those nodes will be our neighbors.</summary>
			/// <param name="extendBounds"></param>
			private void LookUpNeighborsFromParent(ref Bounds extendBounds)
			{
#if !BACKGROUND_THREAD
				Profiler.BeginSample("LookUpNeighborsFromParent");
#endif
				Node _parent = m_Parent;
				List<Node> data = new List<Node>(_parent.GetChildren());
				data.AddRange(_parent.m_NeighborsCache[26]); // include this node.
				LookUpNeighborsFromData(ref extendBounds, data);
#if !BACKGROUND_THREAD
				Profiler.EndSample();
#endif
			}

			private void LookUpNeighborsFromData(ref Bounds extendBounds, List<Node> data)
			{
				int cnt = data.Count;
				while (cnt-- > 0)
				{
					if (data[cnt] == this)
						continue; // Neighbors != Self

					for (int i = 0; i < 26; i++)
					{
						if (m_DirectionBoundsCache[i].Intersects(data[cnt].bounds, containSurfaceCollision: false))
						{
							// could double in different direction, because neighbors can be bigger.
							m_NeighborsCache[i].Add(data[cnt]);

							// also keep reference in cell[26], for fast look up
							if (!m_NeighborsCache[26].Contains(data[cnt]))
								m_NeighborsCache[26].Add(data[cnt]);
						}
					}
				}
			}

			internal void UpdateNeighborsIfNeed()
			{
				if (m_Children != null)
				{
					for (int i = 0; i < 8; i++)
					{
						m_Children[i].UpdateNeighborsIfNeed();
					}
				}

				if (m_NeighborsCache == null)
					return; // on demend, avoid common case the edge node never used at runtime.

				Bounds extendBounds = new Bounds(bounds.center, bounds.size * (1f + float.Epsilon)); // extend a bit to detect collision.

				int k = (int)eNeighbor.All;
				while (k-- > 0)
				{
					// only update the edge node
					if (m_NeighborsCache[k].Count == 0)
					{
						// no cache found.
						List<Node> data = new List<Node>();
						// start search from root.
						m_Root.rootNode.InternalNeighborsColliding(data, ref m_DirectionBoundsCache[k], this);

						int cnt = data.Count;
						while (cnt-- > 0)
						{
							if (data[cnt].bounds.Intersects(extendBounds))
							{
								m_NeighborsCache[k].Add(data[cnt]);
							}
						}
					}
				}
			}

			private void InternalRemoveNeighbors(Node node)
			{
				if (m_NeighborsCache == null)
					return;
				for (int i = 0; i < 26; i++)
				{
					if (m_NeighborsCache[i].Contains(node))
					{
						m_NeighborsCache[i].Remove(node);
					}
				}
				m_NeighborsCache[26].Remove(node);
			}

			

			/// <summary>
			/// Draws node boundaries visually for debugging.
			/// Must be called from OnDrawGizmos externally. See also: DrawAllObjects.
			/// </summary>
			/// <param name="depth">Used for recurcive calls to this method.</param>
			public void DrawAllBounds(float alpha, float depth, int requestDepth)
			{
				if (requestDepth == 0 || depth == requestDepth)
				{
					Gizmos.color = ColorExtend.GetJetColor(1f - depth / (float)m_Root.totalDepth).CloneAlpha(alpha);

					Bounds thisBounds = new Bounds(center, new Vector3(m_AdjLength, m_AdjLength, m_AdjLength));
					Gizmos.DrawWireCube(thisBounds.center, thisBounds.size);
				}
				if (requestDepth == 0 || depth <= requestDepth)
				{
					if (m_Children != null)
					{
						depth++;
						for (int i = 0; i < 8; i++)
						{
							m_Children[i].DrawAllBounds(alpha, depth, requestDepth);
						}
					}
				}
			}

			/// <summary>
			/// Draws the cost of current node in the tree visually for debugging.
			/// Must be called from OnDrawGizmos externally. See also: DrawAllBounds.
			/// </summary>
			public void DrawCost(float alpha)
			{
				Color oldColor = Gizmos.color;
				if (m_Children != null)
				{
					for (int i = 0; i < 8; i++)
					{
						m_Children[i].DrawCost(alpha);
					}
				}
				else
				{
					// TODO: draw cost
					if (m_Obstacles.Count > 0)
					{
						// cyan, yelow, red, black
						Gizmos.color = ColorExtend.GetJetColor(m_Obstacles.Count * 0.3f).CloneAlpha(0.3f);
						Gizmos.DrawCube(bounds.center, bounds.size);
					}
				}
				Gizmos.color = oldColor;
			}

			/// <summary>
			/// We can shrink the octree if:
			/// - This node is >= double minLength in length
			/// - All objects in the root node are within one octant
			/// - This node doesn't have children, or does but 7/8 children are empty
			/// We can also shrink it if there are no objects left at all!
			/// </summary>
			/// <param name="minLength">Minimum dimensions of a node in this octree.</param>
			/// <returns>The new root, or the existing one if we didn't shrink.</returns>
			public Node ShrinkIfPossible(float minLength)
			{
				if (baseLength < (2 * minLength))
				{
					return this;
				}
				if (m_Obstacles.Count == 0 && m_Children == null)
				{
					return this;
				}

				// Check objects in root
				int bestFit = -1;
				int cnt = m_Obstacles.Count;
				for (int i = 0; i < m_Obstacles.Count; i++)
				{
					int newBestFit = (int)this.BestFitChild(m_Obstacles[i].bounds);
					if (bestFit == -1 || (int)newBestFit == bestFit)
					{
						// In same octant as the other(s). Does it fit completely inside that octant?
						if (m_ChildBoundsCache[newBestFit].IsFullyEncapsulate(m_Obstacles[i].bounds))
						{
							if (bestFit < 0)
							{
								bestFit = newBestFit;
							}
						}
						else
						{
							// Nope, so we can't reduce. Otherwise we continue
							return this;
						}
					}
					else
					{
						return this; // Can't reduce - objects fit in different octants
					}
				}

				// Check objects in children if there are any
				if (m_Children != null)
				{
					bool childHadContent = false;
					for (int i = 0; i < m_Children.Length; i++)
					{
						if (m_Children[i].HasObstacle())
						{
							if (childHadContent)
							{
								return this; // Can't shrink - another child had content already
							}
							if (bestFit >= 0 && bestFit != i)
							{
								return this; // Can't reduce - objects in root are in a different octant to objects in child
							}
							childHadContent = true;
							bestFit = i;
						}
					}
				}

				// Can reduce
				if (m_Children == null)
				{
					// We don't have any children, so just shrink this node to the new size
					// We already know that everything will still fit in it
					SetValues(baseLength * 0.5f, m_MinSize, m_Looseness, m_ChildBoundsCache[bestFit].center);
					return this;
				}

				// No objects in entire octree
				if (bestFit == -1)
				{
					return this;
				}

				// We have children. Use the appropriate child as the new root node
				return m_Children[bestFit];
			}

			// #### PRIVATE METHODS ####

			/// <summary>
			/// Set values for this node. 
			/// </summary>
			/// <param name="baseLengthVal">Length of this node, not taking looseness into account.</param>
			/// <param name="minSizeVal">Minimum size of nodes in this octree.</param>
			/// <param name="loosenessVal">Multiplier for baseLengthVal to get the actual size.</param>
			/// <param name="centerVal">Centre position of this node.</param>
			void SetValues(float baseLengthVal, float minSizeVal, float loosenessVal, Vector3 centerVal)
			{
				baseLength = baseLengthVal;
				m_MinSize = minSizeVal;
				m_Looseness = loosenessVal;
				m_AdjLength = m_Looseness * baseLengthVal;

				// Create the bounding box.
				Vector3 size = new Vector3(m_AdjLength, m_AdjLength, m_AdjLength);
				bounds = new Bounds(centerVal, size);
				m_BoundsHashCode = bounds.GetHashCode();
				CacheSubDivisionWithLooseness();
			}
			
			public void CheckObstacleRemainPosition(Obstacle obstacle)
			{
				Profiler.BeginSample("CheckObstacleRemainPosition");
#if true
				// accurate, slower
				Vector3 closestPoint = obstacle.collider.ClosestPoint(bounds.center);
				if (!bounds.Contains(closestPoint))
#else
				// Fuzzy, faster
				if (!bounds.Intersects(obstacle.collider.bounds))
#endif
				{
					// obstacle leave this node
					Remove(obstacle);
				}
				else if (!m_Obstacles.Contains(obstacle))
				{
					// obstacle enter this node
					InternalAdd(obstacle, false);
				}
				else if (m_Children != null)
				{
					// pass to children
					for (int i = 0; i < 8; i++)
					{
						m_Children[i].CheckObstacleRemainPosition(obstacle);
					}
				}
				Profiler.EndSample();
			}


			/// <summary>Add obstacle and detect the obstacle edge within this node,
			/// and pass it to children</summary>
			/// <param name="obstacle">obstacle to add</param>
			/// <param name="continueAdd">this add request are come from <see cref="ContinueInternalSplit(Obstacle)"/></param>
			void InternalAdd(Obstacle obstacle, bool continueAdd)
			{
				bool hadObstacleInRecord = m_Obstacles.Contains(obstacle); // optimize list contain check.

				// case 1, not recalculate, if new obstacle continue, otherwise stop.
				// case 2, recalculate child bounds, and internalAdd recursive. O(8 ^ n)
				if (!continueAdd && hadObstacleInRecord)
					return;

				if (continueAdd)
					Profiler.BeginSample("InternalAdd_Continue");
				else
					Profiler.BeginSample("InternalAdd_FirstAdd");
				
				// We know it fits at this level if we've got this far
				if (!hadObstacleInRecord)
					m_Obstacles.Add(obstacle);

				// Find which child the object is closest to based on where the
				// object's center is located in relation to the octree's center.
				int bestFitChild = (int)this.BestFitChild(obstacle.bounds);
				if (m_Children != null && m_Children[bestFitChild].bounds.IsFullyEncapsulate(obstacle.bounds))
				{
					m_Children[bestFitChild].InternalAdd(obstacle, false); // Go a level deeper
				}
				else if (m_Children == null && m_ChildBoundsCache[bestFitChild].IsFullyEncapsulate(obstacle.bounds))
				{
					InternalSplitRequest(obstacle, bestFitChild);
				}
				else
				{
					// none of the child have best fit and this is the leaf node 
					// which can encapsulate whole object.
					// but wait, we have the actual collider. do the math.
					int[] overlapChilds = new int[8];
					int overlapChildsCnt = 0;
					Color emptyCellColor = Color.red.CloneAlpha(.5f);
					for (int i = 0; i < 8; i++)
					{

#if false
						bool contact;
						if (hadObstacleInRecord)
						{
							// cheap test
							//Vector3 closestPointOnBound = obstacle.collider.ClosestPointOnBounds(m_ChildBoundsCache[i].center);
							//Vector3 boundsPt = m_ChildBoundsCache[i].ClosestPoint(closestPointOnBound);
							Vector3 closestPoint = obstacle.collider.ClosestPoint(m_ChildBoundsCache[i].center);
							contact = m_ChildBoundsCache[i].Contains(closestPoint);
						}
						else
						{
							// physics, slow test
							// yes, this may detect something else, but obstacle update should clean up later on.
							contact = Physics.CheckBox(m_ChildBoundsCache[i].center, m_ChildBoundsCache[i].extents, Quaternion.identity);
						}

						if (contact)
#else
						// FIXME: known bug, when scene are too simple and the obstacle plancement are perfect align
						// to root node position e.g. (0,0,0) vs (0,0,0) or (0, 100, 200), or (100, 0, 200) or (100, 200, 0)
						// the obstacle surface may not able to recognize by this simple check...
						Vector3 closestPoint = obstacle.collider.ClosestPoint(m_ChildBoundsCache[i].center);
						if (m_ChildBoundsCache[i].Contains(closestPoint))
#endif
						{
							// The collison childrens
#if DEVIATION_DEBUG
							DebugExtend.DrawBounds(m_ChildBoundsCache[i], Color.cyan);
#endif
							overlapChilds[overlapChildsCnt] = i;
							overlapChildsCnt++;
						}
						else
						{
#if DEVIATION_DEBUG
							DebugExtend.DrawBounds(m_ChildBoundsCache[i], emptyCellColor);
							// Debug.DrawLine(closestPoint, m_ChildBoundsCache[i].center, emptyCellColor);
#endif
						}
					}

					bool IsNotSmallestNode = baseLength * 0.5f > m_MinSize;

					if (overlapChildsCnt == 8)
					{
						// nope we still the one who encapsulate whole object.
						// case : obstacle may land right on the center of the bounds,
						// e.g. Vector3.zero cube with 1,1,1 size.
						if (IsNotSmallestNode)
						{
							// when we are not the smallest node, we try to calculate the obstacle size
							float halfSize = baseLength * 0.25f;
							Vector3[] vertices = GetVertices(eNeighbor.All);
							int overlapPoint = 0;
							for (int i=0; i<8; i++)
							{
								if (!vertices[i].EqualRoughly(obstacle.collider.ClosestPoint(vertices[i]), halfSize))
								{
									overlapPoint++;
									if (overlapPoint >= 4)
										break;
								}
							}
							
							if (overlapPoint >= 4)
							{
								// to much empty space remain, still can go deeper,
								// so the obstacle are smaller then one of the children.
								if (m_Children == null)
								{
									InternalSplitRequest(obstacle, new int[8] { 0, 1, 2, 3, 4, 5, 6, 7 });
								}
								else
								{
									for (int i = 0; i < 8; i++)
									{
										m_Children[i].InternalAdd(obstacle, false);
									}
								}
							}
						}
						else
						{
#if DEVIATION_DEBUG
							// this is the smallest node, it's acceptable deviation
							DebugExtend.DrawBounds(bounds, Color.magenta, 10f, true);
#endif
						}
					}
					else if (overlapChildsCnt == 0)
					{
#if DEVIATION_DEBUG
						Vector3 closestPoint = obstacle.collider.ClosestPoint(bounds.center);
						if (bounds.Contains(closestPoint))
						{
							DebugExtend.DrawPoint(closestPoint, Color.white, baseLength, 10f, true);
							DebugExtend.DrawBounds(bounds, Color.green, 10f, true);
						}
						else
						{
							Debug.LogError("Try to access " + bounds + " with no collision.", obstacle.transform);
							// throw new System.InvalidProgramException();
							DebugExtend.DrawBounds(bounds, Color.red, 10f, true);
						}
						m_Obstacles.Remove(obstacle);
#else
						// it's okay to escape deviation, when the closest point are out of the bounds box,
						// usually because deviation smaller than min size (m_MinSize),
						// octree had it's own limitation. (lose detail)
						m_Obstacles.Remove(obstacle);
#endif
					}
					else if (IsNotSmallestNode)
					{
						if (m_Children == null)
						{
							System.Array.Resize(ref overlapChilds, overlapChildsCnt);
							InternalSplitRequest(obstacle, overlapChilds);
						}
						else
						{
							for (int i = 0; i < overlapChildsCnt; i++)
							{
								m_Children[overlapChilds[i]].InternalAdd(obstacle, false);
							}
						}
					}
					else
					{
#if DEVIATION_DEBUG
						// this is the smallest node, it's acceptable deviation
						DebugExtend.DrawBounds(bounds, Color.magenta, 10f, true);
#endif
					}
				}

				Profiler.EndSample();
			}

			/// <summary>Should only based on obstacle, since we develop pathfinding octree.</summary>
			/// <param name="obstacle"></param>
			/// <param name="childPos"></param>
			void InternalSplitRequest(Obstacle obstacle, params int[] childPos)
			{
				if (!obstacle.IsValid)
					throw new System.InvalidProgramException("Invalid obstacle shouldn't reach this level, handle this on higher level.");
				if (m_Children != null)
					throw new System.InvalidProgramException("This method only called by leaf nodes");
				int cnt = childPos.Length;
				if (cnt > 8)
					throw new System.IndexOutOfRangeException("Octree only have 8 children for each node.");
				else if (cnt < 1)
					throw new System.IndexOutOfRangeException("Octree can't split without position reference.");

				// Check if we have enough quota to process split()
				SurfaceReconstructionRequest request;
				if (m_Root.m_SplitRequest.TryGetValue(obstacle, out request) &&
					request.m_SpiltQuota > 0)
				{
					Profiler.BeginSample("Internal_SplitRequest_Complete");
					// we use that quota directly.
					if (Application.isPlaying)
						request.m_SpiltQuota--;

					float quarter = baseLength * 0.25f; // divide 4
					float newLength = baseLength * 0.5f; // divide 2
					
					// create new nodes are alway have performance cost.
					m_Children = new Node[8]
					{
						new Node(m_Root, this, newLength, m_MinSize, m_Looseness, center + new Vector3(-quarter, -quarter, -quarter)), // LDF
						new Node(m_Root, this, newLength, m_MinSize, m_Looseness, center + new Vector3(-quarter, -quarter, quarter)), // LDB
						new Node(m_Root, this, newLength, m_MinSize, m_Looseness, center + new Vector3(-quarter, quarter, -quarter)), // LUF
						new Node(m_Root, this, newLength, m_MinSize, m_Looseness, center + new Vector3(-quarter, quarter, quarter)), // LUB
						new Node(m_Root, this, newLength, m_MinSize, m_Looseness, center + new Vector3(quarter, -quarter, -quarter)), // RDF
						new Node(m_Root, this, newLength, m_MinSize, m_Looseness, center + new Vector3(quarter, -quarter, quarter)), // RDB
						new Node(m_Root, this, newLength, m_MinSize, m_Looseness, center + new Vector3(quarter, quarter, -quarter)), // RUF
						new Node(m_Root, this, newLength, m_MinSize, m_Looseness, center + new Vector3(quarter, quarter, quarter)), // RUB
					};

					for (int i = 0; i < cnt; i++)
					{
						m_Children[childPos[i]].InternalAdd(obstacle, false); // Go a level deeper
					}
					Profiler.EndSample();
					m_Root.OnChildNodeSplit(this);
				}
				else
				{
					// no more quota, request again.
					if (request == null)
					{
						// it's being removed or finished. make a new empty request with quota.
						// because it only removed when the quota not being used on last time.
						m_Root.m_SplitRequest.Add(obstacle, new SurfaceReconstructionRequest()
						{
							m_NodeRequest = new List<ObstacleInsertionInfo>(),
							// since it removed on last frame, we should reset quota
							m_SpiltQuota = NODE_SPLIT_QUOTA,
						});
						request = m_Root.m_SplitRequest[obstacle];
					}

					request.m_NodeRequest.Add(new ObstacleInsertionInfo()
					{
						m_Node = this,
						m_ContinueInternalSplitCallback = ContinueInternalSplit,
					});
				}
			}

			/// <summary>A async callback for <see cref="InternalSplitRequest"/>,
			/// and <seealso cref="ContinueObstacleSplitRequest"/>
			/// when we are out of quota, the split request will be delay.</summary>
			/// <param name="obstacle"></param>
			void ContinueInternalSplit(Obstacle obstacle)
			{
				if (obstacle.IsValid)
					InternalAdd(obstacle, true);
			}

			/// <summary>Remove obstacle</summary>
			/// <param name="obstacle">Object to remove.</param>
			/// <returns>True if the object was removed successfully.</returns>
			bool InternalRemove(Obstacle obstacle)
			{
				bool removed = m_Obstacles.Remove(obstacle);

				if (removed && m_Children != null)
				{
					for (int i = 0; i < 8; i++)
					{
						m_Children[i].InternalRemove(obstacle);
					}
					MergeIfPossible();
				}
				return removed;
			}

			private void CacheSubDivisionWithLooseness()
			{
				float quarter = baseLength * 0.25f;
				float childActualLength = baseLength * 0.5f * m_Looseness;
				Vector3 childActualSize = new Vector3(childActualLength, childActualLength, childActualLength);
				m_ChildBoundsCache = new Bounds[8]
				{
					new Bounds(center + new Vector3(-quarter, -quarter, -quarter), childActualSize), // LDF
					new Bounds(center + new Vector3(-quarter, -quarter, quarter), childActualSize), // LDB
					new Bounds(center + new Vector3(-quarter, quarter, -quarter), childActualSize), // LUF
					new Bounds(center + new Vector3(-quarter, quarter, quarter), childActualSize), // LUB
					new Bounds(center + new Vector3(quarter, -quarter, -quarter), childActualSize), // RDF
					new Bounds(center + new Vector3(quarter, -quarter, quarter), childActualSize), // RDB
					new Bounds(center + new Vector3(quarter, quarter, -quarter), childActualSize), // RUF
					new Bounds(center + new Vector3(quarter, quarter, quarter), childActualSize), // RUB
				};
			}
			
			/// <summary>
			/// Sent request up to root, queue up and wait for response.
			/// logic:
			/// if previous request still cooling down, the duration for the lock will double.
			/// when previous request are expired, the duration will divide by 2;
			/// result :
			/// if node are too often sending the merge request, we reject that merge request here.
			/// </summary>
			/// <returns><see cref="ShouldMerge"/></returns>
			void MergeIfPossible()
			{
				if (ShouldMerge())
				{
					m_Root.m_MergeRequest.Enqueue(new MergeRequest(this, ShouldMerge, Merge));
				}
			}

			/// <summary>Check if children node can be merge into this node.
			/// This checking only vaild on level (N-1).</summary>
			/// <returns>true = all children are leaf node.</returns>
			bool ShouldMerge()
			{
				if (!m_MergeRequestLock.IsExpired())
				{
					return false;
				}
				else if (m_Children == null)
				{
					return false;
				}
				else
				{
					for (int i = 0; i < 8; i++)
					{
						if (m_Children[i].m_Children != null)
						{
							// If any of the *children* have children, there are definitely too many to merge,
							// or the child would have been merged already
							return false;
						}
					}
				}
				return true;
			}

			/// <summary>
			/// Merge all children into this node - the opposite of Split.
			/// Note: We only have to check one level down since a merge will never happen if the children already have children,
			/// since THAT won't happen unless there are already too many objects to merge.
			/// </summary>
			void Merge()
			{
				// if trigger too often, merge request will be reject,
				Profiler.BeginSample("Merge");
				// Note: We know children != null or we wouldn't be merging
				HashSet<Obstacle> dump = new HashSet<Obstacle>(m_Obstacles);
				for (int i = 0; i < 8; i++)
				{
					dump.UnionWith(m_Children[i].m_Obstacles);
				}
				m_Obstacles = new List<Obstacle>(dump);
				Profiler.EndSample();

				m_Root.OnChildNodeMerge(this);

				// Remove the child nodes (and the objects in them - they've been added elsewhere now)
				m_Children = null;

				// Merge success, set lock.
				m_MergeUnlockDuration *= (m_MergeRequestLock.IsExpired() ? 0.5f : 2f);
				m_MergeUnlockDuration = Mathf.Clamp(m_MergeUnlockDuration, m_MergeDelayRange.x, m_MergeDelayRange.y);
				m_MergeRequestLock = ExpiringLock.CreateWhenLater(m_MergeRequestLock, m_MergeUnlockDuration);
			}

			/// <summary>Check this node is leaf Node.</summary>
			/// <returns>true = no children node under this node.</returns>
			public bool IsLeafNode()
			{
				return m_Children == null;
			}

			/// <summary>Checks if this node or anything below it has something in it.</summary>
			/// <returns>True if this node or any of its children, grandchildren etc have something in them</returns>
			public bool HasObstacle(bool includeChild = false)
			{
				if (m_Obstacles == null)
					return false;
				else if (m_Obstacles.Count > 0)
					return true;
				else if (includeChild && m_Children != null)
				{
					for (int i = 0; i < 8; i++)
					{
						if (m_Children[i].HasObstacle())
							return true;
					}
				}
				return false;
			}

			/// <summary>Should only be called from root node, to calculate the max depth in tree.</summary>
			/// <param name="maxDepth"></param>
			public void GetMaxDepth(ref int maxDepth)
			{
				if (m_Children == null)
				{
					if (Depth >= maxDepth)
						maxDepth = Depth;
				}
				else
				{
					for (int i = 0; i < 8; i++)
					{
						m_Children[i].GetMaxDepth(ref maxDepth);
					}
				}
			}

			public override int GetHashCode()
			{
				/// <see cref="SetValues(float, float, float, Vector3)"/>
				return m_BoundsHashCode;
			}

			public override string ToString()
			{
				return string.Format("Hashcode={3}, OctreeNode {0}, obstacles={1}, depth={2}", bounds, m_Obstacles.Count, Depth, GetHashCode());
			}

			/// <summary>For priority queue usage, we compare their size, instead of position.
			/// <see cref="NetworkGraph.m_PQueue"/></summary>
			/// <param name="other"></param>
			/// <returns></returns>
			public int CompareTo(Node other)
			{
				// inverse result, bigger size process first;
				return baseLength.CompareTo(other.baseLength) * -1;
			}
			
			#region IPVertex
			public bool LineOfSight(Node other)
			{
				if (other.m_Obstacles.Count > 0)
					return false;

				Vector3 direction = other.center - center;
				float distance = direction.magnitude;
				Ray ray = new Ray(center, direction);
				List<Node> collisionList = new List<Node>(10);
				m_Root.GetColliding(collisionList, ray, distance);
				int cnt = collisionList.Count;
				for (int i = 0; i < cnt; i++)
				{
					if (collisionList[i].m_Obstacles.Count > 0)
						return false;
				}
				return true;
			}

			public float Cost(Node other, float sizeRef)
			{
				if (other.m_Obstacles.Count > 0)
					return float.PositiveInfinity;

				float distanceSqr = (center - other.center).sqrMagnitude;

				// the other node are too small to pass via.
				float boundsSizeSqr = other.bounds.size.sqrMagnitude;
				if (boundsSizeSqr < sizeRef)
					return float.PositiveInfinity;

				return distanceSqr - boundsSizeSqr;
			}

			public IEnumerable<Node> Neighbours()
			{
				return GetNeighbors(eNeighbor.All);
			}

			public bool Equals(Node other)
			{
				if (GetHashCode() == other.GetHashCode())
					return bounds == other.bounds;
				else
					return false;
			}
			#endregion
		}
	}
}
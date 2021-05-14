using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlyAgent.Navigation
{
	/// <summary>Direction of neighbor node</summary>
	/// <remarks>U = Up, D = Down, F = Forward, B = Backward, L = Left, R = Right</remarks>
	public enum eNeighbor
	{
		All = 26,
		// face 0 ~ 5
		U = 0,
		D,
		F,
		B,
		L,
		R,

		// edge 6 ~ 17
		UF = 6,
		UB,
		UL,
		UR,
		DF,
		DB,
		DL,
		DR,
		LF,
		LB,
		RF,
		RB,

		// vertex 18 ~ 25
		UFL = 18,
		UFR,
		UBL,
		UBR,
		DFL,
		DFR,
		DBL,
		DBR,
	}

	/// <summary>octree children in Z-curve order</summary>
	public enum eZCurvePos
	{
		/// <summary>Left down front</summary>
		LDF = 0,
		/// <summary>Left down back</summary>
		LDB = 1,
		/// <summary>Left up front</summary>
		LUF = 2,
		/// <summary>Left up back</summary>
		LUB = 3,
		/// <summary>Right down front</summary>
		RDF = 4,
		/// <summary>Right down back</summary>
		RDB = 5,
		/// <summary>Right up front</summary>
		RUF = 6,
		/// <summary>Right up back</summary>
		RUB = 7,
	}

	public class MergeRequest : System.IComparable<MergeRequest>
	{
		public readonly Octree.Node m_Node;
		public System.Func<bool> m_AllowMerge;
		public System.Action m_Callback;
		public MergeRequest(Octree.Node node, System.Func<bool> allowMerge, System.Action callback)
		{
			m_Node = node;
			m_AllowMerge = allowMerge;
			m_Callback = callback;
		}

		public bool IsVaild { get { return m_Node != null; } }
		public float baseLength { get { return IsVaild ? m_Node.baseLength : 0f; } }

		public int CompareTo(MergeRequest other)
		{
			return baseLength.CompareTo(other.baseLength);
		}
	}

	public class SurfaceReconstructionRequest
	{
		public List<ObstacleInsertionInfo> m_NodeRequest;
		public Obstacle m_Obstacle;
		public int m_SpiltQuota;
	}

	public class ObstacleInsertionInfo
	{
		public Octree.Node m_Node;
		public ContinueInternalSplitCallback m_ContinueInternalSplitCallback;
	}

	/// <summary><see cref="Octree.Node.ContinueInternalSplit(Obstacle)"/></summary>
	/// <param name="obstacle"></param>
	public delegate void ContinueInternalSplitCallback(Obstacle obstacle);

	public static class OctreeUtilities
	{
		public static eNeighbor GetNeighborInverseDirection(eNeighbor from)
		{
			switch (from)
			{
				case eNeighbor.U: return eNeighbor.D;
				case eNeighbor.D: return eNeighbor.U;
				case eNeighbor.F: return eNeighbor.B;
				case eNeighbor.B: return eNeighbor.F;
				case eNeighbor.L: return eNeighbor.R;
				case eNeighbor.R: return eNeighbor.L;
				// edge 6 ~ 17
				case eNeighbor.UF: return eNeighbor.DB;
				case eNeighbor.UB: return eNeighbor.DF;
				case eNeighbor.UL: return eNeighbor.DR;
				case eNeighbor.UR: return eNeighbor.DL;
				case eNeighbor.DF: return eNeighbor.UB;
				case eNeighbor.DB: return eNeighbor.UF;
				case eNeighbor.DL: return eNeighbor.UR;
				case eNeighbor.DR: return eNeighbor.UL;
				case eNeighbor.LF: return eNeighbor.RB;
				case eNeighbor.LB: return eNeighbor.RF;
				case eNeighbor.RF: return eNeighbor.LB;
				case eNeighbor.RB: return eNeighbor.LF;
				// vertex 18 ~ 25
				case eNeighbor.UFL: return eNeighbor.DBR;
				case eNeighbor.UFR: return eNeighbor.DBL;
				case eNeighbor.UBL: return eNeighbor.DFR;
				case eNeighbor.UBR: return eNeighbor.DFL;
				case eNeighbor.DFL: return eNeighbor.UBR;
				case eNeighbor.DFR: return eNeighbor.UBL;
				case eNeighbor.DBL: return eNeighbor.UFR;
				case eNeighbor.DBR: return eNeighbor.UFL;
				case eNeighbor.All: throw new System.InvalidOperationException();
				default: throw new System.NotImplementedException();
			}
		}

		/// <summary>Find which child node this object would be most likely to fit in.</summary>
		/// <param name="objBounds">The object's bounds.</param>
		/// <returns>One of the eight child octants.</returns>
		public static eZCurvePos BestFitChild(this Octree.Node node, Bounds objBounds)
		{
			return (eZCurvePos)
				(objBounds.center.x <= node.center.x ? 0 : 4) +
				(objBounds.center.y <= node.center.y ? 0 : 2) +
				(objBounds.center.z <= node.center.z ? 0 : 1);
		}

		public static Bounds[] GetDirectionBounds(Octree.Node node)
		{
			Bounds bounds = node.bounds;
			Vector3 size = bounds.size;
			Vector3 distance = bounds.size;
			return new Bounds[26]
			{
				// up
				new Bounds(bounds.center + new Vector3(0f, distance.y, 0f), size),
				// down
				new Bounds(bounds.center + new Vector3(0f, -distance.y, 0f), size),
				// forward
				new Bounds(bounds.center + new Vector3(0f, 0f, distance.z), size),
				// backward
				new Bounds(bounds.center + new Vector3(0f, 0f, -distance.z), size),
				// left
				new Bounds(bounds.center + new Vector3(-distance.x, 0f, 0f), size),
				// Right
				new Bounds(bounds.center + new Vector3(distance.x, 0f, 0f), size),
				// UF
				new Bounds(bounds.center + new Vector3(0f, distance.y, distance.z), size),
				// UB
				new Bounds(bounds.center + new Vector3(0f, distance.y, -distance.z), size),
				// UL
				new Bounds(bounds.center + new Vector3(-distance.x, distance.y, 0f), size),
				// UR
				new Bounds(bounds.center + new Vector3(distance.x, distance.y, 0f), size),
				// DF
				new Bounds(bounds.center + new Vector3(0f, -distance.y, distance.z), size),
				// DB
				new Bounds(bounds.center + new Vector3(0f, -distance.y, -distance.z), size),
				// DL
				new Bounds(bounds.center + new Vector3(-distance.x, -distance.y, 0f), size),
				// DR
				new Bounds(bounds.center + new Vector3(distance.x, -distance.y, 0f), size),
				// LF
				new Bounds(bounds.center + new Vector3(-distance.x, 0f, distance.z), size),
				// LB
				new Bounds(bounds.center + new Vector3(-distance.x, 0f, -distance.z), size),
				// RF
				new Bounds(bounds.center + new Vector3(distance.x, 0f, distance.z), size),
				// RB
				new Bounds(bounds.center + new Vector3(distance.x, 0f, -distance.z), size),
				// UFL
				new Bounds(bounds.center + new Vector3(-distance.x, distance.y, distance.z), size),
				// UFR
				new Bounds(bounds.center + new Vector3(distance.x, distance.y, distance.z), size),
				// UBL
				new Bounds(bounds.center + new Vector3(-distance.x, distance.y, -distance.z), size),
				// UBR
				new Bounds(bounds.center + new Vector3(distance.x, distance.y, -distance.z), size),
				// DFL
				new Bounds(bounds.center + new Vector3(-distance.x, -distance.y, distance.z), size),
				// DFR
				new Bounds(bounds.center + new Vector3(distance.x, -distance.y, distance.z), size),
				// DBL
				new Bounds(bounds.center + new Vector3(-distance.x, -distance.y, -distance.z), size),
				// DBR
				new Bounds(bounds.center + new Vector3(distance.x, -distance.y, -distance.z), size)
			};
		}

		/// <summary>
		/// Used when growing the octree. Works out where the old root node would fit inside a new, larger root node.
		/// </summary>
		/// <param name="xDir">X direction of growth. 1 or -1.</param>
		/// <param name="yDir">Y direction of growth. 1 or -1.</param>
		/// <param name="zDir">Z direction of growth. 1 or -1.</param>
		/// <returns>Octant where the root node should be.</returns>
		public static int GetRootPosIndex(int xDir, int yDir, int zDir)
		{
			int result = xDir > 0 ? 4 : 0;
			if (yDir > 0) result += 2;
			if (zDir > 0) result += 1;
			return result;
		}
	}
}
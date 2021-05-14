using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kit;
using FlyAgent.Navigation;

namespace FlyAgent.Utilities
{
	/// <summary>A helper class to do the neighbors check for octree</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(BoxCollider))]
	public class NodeWalker : MonoBehaviour
	{
		[SerializeField] BoxCollider m_Collider = null;
		[SerializeField] MapBaker m_Map = null;
		[SerializeField] float m_Interval = 0.3f;
		[SerializeField] Color m_CurrentNodeColor = Color.green.CloneAlpha(.3f);
		[SerializeField] Color m_ObstacleNodeColor = Color.blue.CloneAlpha(.8f);
		public eCheckFor m_CheckFor = eCheckFor.GetCollidingLeafNode;
		public eNeighbor m_Neighbor = eNeighbor.All;
		
		public enum eCheckFor
		{
			None = 0,
			GetCollidingLeafNode = 1,
			GetNeighborsColliding = 2,
			GetObstacle = 3,
		}

		private void Reset()
		{
			if (m_Collider == null)
				m_Collider = GetComponent<BoxCollider>();
			if (m_Collider != null)
				m_Collider.isTrigger = true;
		}

		private void OnEnable()
		{
			StartCoroutine(PeriodicUpdate());
		}

		IEnumerator PeriodicUpdate()
		{
			yield return new WaitUntil(() => m_Map != null && m_Map.OctreeStatic != null);
			Octree octree = m_Map.OctreeStatic;

			List<Octree.Node> result = new List<Octree.Node>();
			while (true)
			{
				yield return new WaitForSeconds(m_Interval);

				if (m_CheckFor == eCheckFor.GetCollidingLeafNode)
				{
					octree.GetCollidingLeafNode(result, m_Collider.bounds);
				}
				else if (m_CheckFor == eCheckFor.GetNeighborsColliding)
				{
					octree.GetCollidingLeafNode(result, m_Collider.bounds);

					Octree.Node anchor = null;
					if (result != null && result.Count > 0)
					{
						anchor = result[0];
						DebugExtend.DrawBounds(anchor.bounds, Color.green.CloneAlpha(Mathf.Repeat(Time.timeSinceLevelLoad, 1f)), m_Interval, true);

						var other = anchor.GetNeighbors(m_Neighbor);
						foreach (var o in other)
						{
							DebugExtend.DrawBounds(o.bounds, m_ObstacleNodeColor, m_Interval, true);
						}
					}

				}
				else if (m_CheckFor == eCheckFor.GetObstacle)
				{
					Collider[] rst = new Collider[1];
					int tmp = Physics.OverlapBoxNonAlloc(m_Collider.bounds.center, m_Collider.bounds.extents, rst, transform.rotation);
					if (tmp > 0)
					{
						Obstacle obstacle = new Obstacle(rst[0]);
						HashSet<Octree.Node> shapeOfNodes = new HashSet<Octree.Node>();
						octree.GetObstacleShape(obstacle, shapeOfNodes);
						result.AddRange(shapeOfNodes);
					}
				}
				
				int cnt = result.Count;
				for (int i = 0; i < cnt; i++)
				{
					if (result[i].HasObstacle(false))
						DebugExtend.DrawBounds(result[i].bounds, m_ObstacleNodeColor, m_Interval, true);
					else
						DebugExtend.DrawBounds(result[i].bounds, m_CurrentNodeColor, m_Interval, true);
				}
				result.Clear();
			}
		}
	}
}

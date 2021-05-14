using System.Collections;
using UnityEngine;

namespace FlyAgent.Agents
{
	public class DestinationBeacon : MonoBehaviour
	{
		public FlyAgent[] m_FlyAgents;
		public float m_Interval = 0.3f;

		private void OnEnable()
		{
			StartCoroutine(PeriodicUpdate());
		}

		private IEnumerator PeriodicUpdate()
		{
			while (true)
			{
				yield return new WaitForSeconds(m_Interval);
				int cnt = m_FlyAgents.Length;
				for (int i=0; i<cnt;i++)
				{
					if (m_FlyAgents[i] != null && m_FlyAgents[i].gameObject.activeSelf)
						m_FlyAgents[i].SetDestination(transform.position);
				}
			}
		}
	}
}
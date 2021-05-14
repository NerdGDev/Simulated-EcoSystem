using UnityEngine;

namespace Kit
{
	public class DontDestroyOnLoad : MonoBehaviour
	{
		void Awake()
		{
			DontDestroyOnLoad(this);
		}
	}
}

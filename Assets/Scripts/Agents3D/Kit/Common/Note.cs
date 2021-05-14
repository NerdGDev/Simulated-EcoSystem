using UnityEngine;

namespace Kit
{
	public class Note : MonoBehaviour
	{
		private enum eMessageType
		{
			None = 0,
			Info = 1,
			Warning = 2,
			Error = 3
		}
		[SerializeField] eMessageType type;
		[SerializeField] string note;
	}
}
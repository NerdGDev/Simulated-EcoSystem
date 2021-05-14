using UnityEngine;

namespace FlyAgent.Agents
{
	public abstract class Brake : MonoBehaviour, System.IDisposable
	{
		public abstract void SetFriction(float amount, float duration = 0f);
		public abstract void Dispose();
	}
}
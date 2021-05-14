using UnityEngine;

namespace Kit
{
	public class ContextButtonAttribute : PropertyAttribute
	{
		public readonly string Callback;
		public ContextButtonAttribute(string callbackMethod)
		{
			Callback = callbackMethod;
		}
	}
}
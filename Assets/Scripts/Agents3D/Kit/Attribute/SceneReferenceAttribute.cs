using UnityEngine;

namespace Kit
{
	/// <summary>label the string type input field on inspector,
	/// and get scene name from build setting.</summary>
	public class SceneReferenceAttribute : PropertyAttribute
	{
		public readonly bool IsShowLabel;
		public SceneReferenceAttribute(bool showLabel = true)
		{
			IsShowLabel = showLabel;
		}
	}
}
using UnityEngine;
using System.Collections;

namespace Kit
{
	/// <summary>Regex attribute.</summary>
	/// <see cref="http://blogs.unity3d.com/2012/09/07/property-drawers-in-unity-4/"/>
	public class RegexAttribute : PropertyAttribute
	{
		public readonly string pattern;
		public readonly string helpMessage;
		public RegexAttribute(string pattern, string helpMessage)
		{
			this.pattern = pattern;
			this.helpMessage = helpMessage;
		}
	}
}
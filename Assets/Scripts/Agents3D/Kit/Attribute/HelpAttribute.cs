using UnityEngine;

namespace Kit
{
	// enum for UnityEditor.MessageType
	public enum eMessageType
	{
		None = 0,
		Info = 1,
		Warning = 2,
		Error = 3
	}

	public class HelpAttribute : PropertyAttribute
	{
		public readonly string text;
		public readonly eMessageType type;
		public HelpAttribute(string _text, eMessageType _type = eMessageType.Info)
		{
			text = _text;
			type = _type;
		}
	}
}
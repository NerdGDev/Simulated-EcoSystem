using UnityEngine;

namespace Kit
{
	public class RectRangeAttribute : PropertyAttribute
	{
		public readonly Rect range;
		public RectRangeAttribute(float minX = 0f, float minY = 0f, float maxX = 1f, float maxY = 1f)
		{
			this.range = new Rect(minX, minY, maxX, maxY);
		}

		public RectRangeAttribute(Vector2 min, Vector2 max)
		{
			this.range = new Rect(min, max);
		}
	}
}
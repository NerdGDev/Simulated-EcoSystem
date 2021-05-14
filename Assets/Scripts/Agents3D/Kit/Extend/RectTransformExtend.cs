using UnityEngine;

namespace Kit
{
	public static class RectTransformExtend
	{
        public static Vector2 GetSize(this RectTransform rectTransform)
        {
            return rectTransform.rect.size;
        }
        public static float GetWidth(this RectTransform rectTransform)
        {
            return rectTransform.rect.width;
        }
        public static float GetHeight(this RectTransform rectTransform)
        {
            return rectTransform.rect.height;
        }
        public static void SetSize(this RectTransform rectTransform, Vector2 newSize)
        {
            Vector2 oldSize = rectTransform.rect.size;
            Vector2 deltaSize = newSize - oldSize;
            rectTransform.offsetMin = rectTransform.offsetMin - new Vector2(deltaSize.x * rectTransform.pivot.x, deltaSize.y * rectTransform.pivot.y);
            rectTransform.offsetMax = rectTransform.offsetMax + new Vector2(deltaSize.x * (1f - rectTransform.pivot.x), deltaSize.y * (1f - rectTransform.pivot.y));
        }

		/// <summary>Get screen size of giving rectTransform</summary>
		/// <see cref="http://www.oguzkonya.com/2016/01/18/converting-unitys-recttransform-to-a-rectangle-in-screen-coordinates/"/>
		/// <param name="rectTransform"></param>
		/// <param name="canvas"></param>
		/// <returns></returns>
		public static Rect GetScreenRect(this RectTransform rectTransform, Canvas canvas)
		{
			Vector3[] corners = new Vector3[4];
			Vector3[] screenCorners = new Vector3[2];

			rectTransform.GetWorldCorners(corners);

			if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
			{
				screenCorners[0] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[1]);
				screenCorners[1] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[3]);
			}
			else
			{
				screenCorners[0] = RectTransformUtility.WorldToScreenPoint(null, corners[1]);
				screenCorners[1] = RectTransformUtility.WorldToScreenPoint(null, corners[3]);
			}

			screenCorners[0].y = Screen.height - screenCorners[0].y;
			screenCorners[1].y = Screen.height - screenCorners[1].y;

			return new Rect(screenCorners[0], screenCorners[1] - screenCorners[0]);
		}

		private const string m_NormalizePositionExceptionMsg = "Normalize position only supported when parent & child have direct relationship.";
		public static Vector2 GetNormalizePosition(RectTransform parent, RectTransform child, bool strict = true)
		{
			if (strict && child.parent != parent)
				throw new System.InvalidOperationException(m_NormalizePositionExceptionMsg);
			return new Vector2(parent.GetNormalizedPositionHorizontal(child, false), parent.GetNormalizedPositionVertical(child, false));
		}

		public static float GetNormalizedPositionVertical(this RectTransform parent, RectTransform child, bool strict = true)
		{
			if (strict && child.parent != parent)
				throw new System.InvalidOperationException(m_NormalizePositionExceptionMsg);
#if !SLOW_CODE
			float pTotal = parent.sizeDelta.y;
			float cLocalH = child.sizeDelta.y;
			float cLocalY = child.offsetMin.y;
			float rst = ((pTotal + cLocalY + cLocalH) / pTotal);
			Debug.LogFormat("({0} + {1} + {2}) / {3} = {4}, index : {5}", pTotal, cLocalY, cLocalH, pTotal, rst, child.GetSiblingIndex());
			return rst;
#else
			return Mathf.Clamp01(1f + (child.anchoredPosition.y / (parent.sizeDelta.y - child.rect.height)));
#endif
		}

		public static float GetNormalizedPositionHorizontal(this RectTransform parent, RectTransform child, bool strict = true)
		{
			if (strict && child.parent != parent)
				throw new System.InvalidOperationException(m_NormalizePositionExceptionMsg);
#if !SLOW_CODE
			float total = parent.sizeDelta.x;
			float localW = child.rect.width;
			float localX = child.anchoredPosition.x;
			// Debug.LogFormat("({0} / ({1} - {2}) = {4}, index : {3}", localX, total, localW, child.GetSiblingIndex(), (localX / (total - localW)));
			return (localX / (total - localW));
#else
			return Mathf.Clamp01(1f + (child.anchoredPosition.x / (parent.sizeDelta.x - child.sizeDelta.x)));
#endif
		}
	}
}
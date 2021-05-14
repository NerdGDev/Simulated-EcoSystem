using UnityEngine;
using System;

[Serializable]
public class TransformCache
{
	/// <summary><see cref="TransformCacheDrawer.InvokeMethod"/></summary>
	public const string Editor_Record_Callback = "OnEditorTransformRecord";
	/// <summary><see cref="TransformCacheDrawer.InvokeMethod"/></summary>
	public const string Editor_Load_Callback = "OnEditorTransformLoad";

	// Transform
	public Vector3 m_LocalPosition = Vector3.zero;
	public Quaternion m_LocalRotation = Quaternion.identity;
	public Vector3 m_LocalScale = Vector3.one;

	[System.Flags]
	public enum eTransformRef
	{
		// None = 0,
		LocalPosition = 1 << 0,
		LocalRotation = 1 << 1,
		LocalScale = 1 << 2,
		AnchorMin = 1 << 3,
		AnchorMax = 1 << 4,
		SizeDelta = 1 << 5,
		Pivot = 1 << 6,
		ALL = LocalPosition | LocalRotation | LocalScale | AnchorMin | AnchorMax | SizeDelta | Pivot,
	}

	public eTransformRef m_TransformRef = eTransformRef.ALL;

	// RectTransform
	public Vector2 m_AnchorMin = Vector3.one * .5f;
	public Vector2 m_AnchorMax = Vector3.one * .5f;
	public Vector2 m_SizeDelta = Vector3.one * 100f;
	public Vector2 m_Pivot = Vector3.one * .5f;

	public TransformCache() { }

	public TransformCache(Transform sample)
	{
		if (sample is Transform)
		{
			m_LocalPosition = sample.localPosition;
			m_LocalRotation = sample.localRotation;
			m_LocalScale = sample.localScale;
		}

		if (sample is RectTransform)
		{
			RectTransform obj = (RectTransform)sample;
			m_AnchorMin = obj.anchorMin;
			m_AnchorMax = obj.anchorMax;
			m_SizeDelta = obj.sizeDelta;
			m_Pivot = obj.pivot;
			m_LocalPosition = obj.anchoredPosition3D;
		}
	}

	public void RecordFrom(Transform target)
	{
		m_LocalPosition = target.localPosition;
		m_LocalRotation = target.localRotation;
		m_LocalScale = target.localScale;

		if (target is RectTransform)
		{
			RectTransform obj = (RectTransform)target;
			m_AnchorMin = obj.anchorMin;
			m_AnchorMax = obj.anchorMax;
			m_SizeDelta = obj.sizeDelta;
			m_Pivot = obj.pivot;

			m_LocalPosition = obj.anchoredPosition3D;
		}
	}

	public void AssignTo(Transform target)
	{
		AssignTo(target, m_TransformRef);
	}

	public void AssignTo(Transform target, eTransformRef tranRef)
	{
		bool isRect = target.transform is RectTransform;

		if (!isRect && (tranRef & eTransformRef.LocalPosition) != 0)
			target.transform.localPosition = m_LocalPosition;
		if ((tranRef & eTransformRef.LocalRotation) != 0)
			target.transform.localRotation = m_LocalRotation;
		if ((tranRef & eTransformRef.LocalScale) != 0)
			target.transform.localScale = m_LocalScale;

		if (isRect)
		{
			RectTransform obj = (RectTransform)target.transform;
			if ((tranRef & eTransformRef.AnchorMin) != 0)
				obj.anchorMin = m_AnchorMin;
			if ((tranRef & eTransformRef.AnchorMax) != 0)
				obj.anchorMax = m_AnchorMax;
			if ((tranRef & eTransformRef.SizeDelta) != 0)
				obj.sizeDelta = m_SizeDelta;
			if ((tranRef & eTransformRef.Pivot) != 0)
				obj.pivot = m_Pivot;
			if ((tranRef & eTransformRef.LocalPosition) != 0)
				obj.anchoredPosition3D = m_LocalPosition;
		}
	}

	public static TransformCache Lerp(TransformCache from, TransformCache to, float pt)
	{
		return new TransformCache()
		{
			m_TransformRef = (pt < .5f) ? from.m_TransformRef : to.m_TransformRef,
			m_AnchorMin = Vector2.Lerp(from.m_AnchorMin, to.m_AnchorMin, pt),
			m_AnchorMax = Vector2.Lerp(from.m_AnchorMax, to.m_AnchorMax, pt),
			m_SizeDelta = Vector2.Lerp(from.m_SizeDelta, to.m_SizeDelta, pt),
			m_Pivot = Vector2.Lerp(from.m_Pivot, to.m_Pivot, pt),
			m_LocalPosition = Vector3.Lerp(from.m_LocalPosition, to.m_LocalPosition, pt),
			m_LocalRotation = Quaternion.Lerp(from.m_LocalRotation, to.m_LocalRotation, pt),
			m_LocalScale = Vector3.Lerp(from.m_LocalScale, to.m_LocalScale, pt)
		};
	}
}
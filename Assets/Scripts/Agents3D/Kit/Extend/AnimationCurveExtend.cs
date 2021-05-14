using UnityEngine;
using System.Collections;

namespace Kit
{
	public static class AnimationCurveExtend
	{
		public static readonly AnimationCurve Snap = new AnimationCurve() { keys = new[] { new Keyframe() { time = 0, value = 1f } } };

		/// <summary>To validate the animation curve is inRange.</summary>
		/// <example>public void OnValidate(){ curve.Clamp(0f,0f,1f,1f); }</example>
		/// <param name="self"></param>
		/// <param name="startTime"></param>
		/// <param name="startValue"></param>
		/// <param name="endTime"></param>
		/// <param name="endValue"></param>
		public static void Clamp(this AnimationCurve self, float startTime = 0f, float startValue = 0f, float endTime = float.MaxValue, float endValue = float.MaxValue)
		{
			for (int i = self.keys.Length - 1; i >= 0; i--)
			{
				if (self.keys[i].value < startValue || self.keys[i].value > endValue || self.keys[i].time < startTime || self.keys[i].time > endTime)
				{
					Keyframe clampKey = new Keyframe(
						Mathf.Clamp(self.keys[i].time, startTime, endTime),
						Mathf.Clamp(self.keys[i].value, startValue, endValue),
						self.keys[i].inTangent,
						self.keys[i].outTangent);
					clampKey.tangentMode = self.keys[i].tangentMode;
					self.MoveKey(i, clampKey);
				}
			}
		}

		/// <summary>Clamp keyframes time & values within 0f~1f</summary>
		/// <param name="self"></param>
		public static void Clamp01(this AnimationCurve self)
		{
			self.Clamp(0f, 0f, 1f, 1f);
		}

		public static Keyframe FirstKey(this AnimationCurve self)
		{
			return self.keys[0];
		}

		public static Keyframe LastKey(this AnimationCurve self)
		{
			return self.keys[self.keys.Length - 1];
		}

		public static bool MatchStartEndKeysValues(this AnimationCurve curve, float start = 0f, float end = 1f)
		{
			return (Mathf.Approximately(curve.FirstKey().value, start) && Mathf.Approximately(curve.LastKey().value, end));
		}

		public static AnimationCurve FixStartEndKeysValues(this AnimationCurve curve, float start = 0f, float end = 1f)
		{
			if (!Mathf.Approximately(curve.FirstKey().value, start))
			{
				curve.MoveKey(0, new Keyframe(curve.FirstKey().time, start, curve.FirstKey().inTangent, curve.FirstKey().outTangent));
			}
			if (!Mathf.Approximately(curve.LastKey().value, end))
			{
				curve.MoveKey(curve.keys.Length - 1, new Keyframe(curve.LastKey().time, end, curve.LastKey().inTangent, curve.LastKey().outTangent));
			}
			return curve;
		}
	}
}
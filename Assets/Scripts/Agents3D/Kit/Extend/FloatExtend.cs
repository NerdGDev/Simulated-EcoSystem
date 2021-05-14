using System;

namespace Kit
{
    public static class FloatExtend
    {
		/// <summary>Shortcut for <see cref="UnityEngine.Mathf.Approximately(float, float)"/></summary>
		/// <param name="self"></param>
		/// <param name="target"></param>
		/// <returns></returns>
        public static bool Approximately(this float self, float target)
        {
            return UnityEngine.Mathf.Approximately(self, target);
        }
        
		/// <summary>Roughly test for float,
		/// <see cref="http://floating-point-gui.de/errors/comparison/"/></summary>
		/// <param name="self"></param>
		/// <param name="target"></param>
		/// <param name="threshold"></param>
		/// <returns>return true when float's are close enough to each other.</returns>
		public static bool EqualRoughly(this float self, float target, float threshold = float.Epsilon)
        {
            return Math.Abs(self - target) < threshold;
        }
		/// <summary>Get Number after scale.</summary>
		/// <param name="self"></param>
		/// <param name="fromMin"></param>
		/// <param name="fromMax"></param>
		/// <param name="toMin"></param>
		/// <param name="toMax"></param>
		/// <returns></returns>
		/// <see cref="http://stackoverflow.com/questions/11121012/how-to-scale-down-the-values-so-they-could-fit-inside-the-min-and-max-values"/>
		public static float Scale(this float self, float fromMin, float fromMax, float toMin, float toMax)
		{
			return toMin + ((toMax - toMin) / (fromMax - fromMin)) * (self - fromMin);
		}

		/// <summary>Faster equation for usually wanted to scale down to 0f~1f.</summary>
		/// <param name="self"></param>
		/// <param name="fromMin"></param>
		/// <param name="fromMax"></param>
		/// <returns></returns>
		public static float Scale01(this float self, float fromMin, float fromMax)
		{
			// return self.Scale(fromMin, fromMax, 0f, 1f); // same
			// return toMin + ((toMax - toMin) / (fromMax - fromMin)) * (self - fromMin);
			// return 0f + ((1f - 0f) / (fromMax - fromMin)) * (self - fromMin);
			// return (1f / (fromMax - fromMin)) * (self - fromMin);
			// return 1f / (fromMax - fromMin) * (self - fromMin);
			return (self - fromMin) / (fromMax - fromMin);
		}
    }
}
using UnityEngine;

namespace Kit
{
    public static class ColorExtend
    {
		#region basic
		/// <summary>Clone & modify alpha value, This method alloc double memory.</summary>
		/// <param name="self"></param>
		/// <param name="value"></param>
		/// <returns>return a new color with new alpha value.</returns>
        public static Color CloneAlpha(this Color self, float value)
        {
            self.a = value;
            return self;
        }

        public static bool Approximately(this Color self, Color target)
        {
			return
				Mathf.Approximately(self.r, target.r) &&
				Mathf.Approximately(self.g, target.g) &&
				Mathf.Approximately(self.b, target.b) &&
				Mathf.Approximately(self.a, target.a);
        }

        public static bool EqualRoughly(this Color self, Color target, float threshold = float.Epsilon)
		{
            return
                self.r.EqualRoughly(target.r, threshold) &&
                self.g.EqualRoughly(target.g, threshold) &&
                self.b.EqualRoughly(target.b, threshold) &&
                self.a.EqualRoughly(target.a, threshold);
        }

        public static Color TryParse(string RGBANumbers)
        {
            // clear up
            string[] param = RGBANumbers.Trim().Split(',');
            if (param == null || param.Length == 0)
                return Color.black;

            int pt = 0;
            int count = 0;
            bool Is255 = false;
            float[] rgba = new float[4]{ 0f,0f,0f,1f };
            
            while(param.Length > pt && count <= 4)
            {
                float tmp;
                if(float.TryParse(param[pt], out tmp))
                {
                    rgba[count] = tmp;
                    count++;
                    if (tmp > 1f) Is255 = true;
                }
                pt++;
            }

            // hotfix for 255
            if (Is255)
            {
                for (int i = 0; i < 3; i++) { rgba[i] /= 255f; }
                rgba[3] = Mathf.Clamp(rgba[3], 0f, 1f);
            }
            return new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
        }

		public static Color Random(this Color self)
		{
			return Random();
		}

        public static Color Random()
        {
            return RandomRange(Color.black, Color.white);
        }

		public static Color RandomRange(this Color self, Color min, Color max)
		{
			return RandomRange(min, max);
		}

        public static Color RandomRange(Color min, Color max)
        {
            return new Color(
				UnityEngine.Random.Range(min.r, max.r),
				UnityEngine.Random.Range(min.g, max.g),
				UnityEngine.Random.Range(min.b, max.b),
				UnityEngine.Random.Range(min.a, max.a));
        }
		#endregion

		#region Color map
		/// <summary>Get the jet color (based on the Jet color map)</summary>
		/// <param name="val">normalized between 0f and 1f</param>
		/// <see cref="https://cn.mathworks.com/help/matlab/ref/jet.html"/>
		public static Color GetJetColor(float val)
		{
			float fourValue = 4.0f * val;
			float red = Mathf.Min(fourValue - 1.5f, -fourValue + 4.5f);
			float green = Mathf.Min(fourValue - 0.5f, -fourValue + 3.5f);
			float blue = Mathf.Min(fourValue + 0.5f, -fourValue + 2.5f);
			Color newColor = new Color();
			newColor.r = Mathf.Clamp01(red);
			newColor.g = Mathf.Clamp01(green);
			newColor.b = Mathf.Clamp01(blue);
			newColor.a = 1;
			return newColor;
		}
		#endregion

	}
}
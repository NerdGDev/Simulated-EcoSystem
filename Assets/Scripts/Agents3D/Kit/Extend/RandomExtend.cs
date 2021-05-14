using System.Collections;
using UnityEngine;
using Math = System.Math;


namespace Kit
{
    /// <summary>the better random.</summary>
    /// <see cref="http://stackoverflow.com/questions/3365337/best-way-to-generate-a-random-float-in-c-sharp"/>
    public static class RandomExtend
    {
		/// <summary>Random number in double.</summary>
		/// <returns></returns>
        public static double NextDouble()
        {
            System.Random random = new System.Random();
            double mantissa = (random.NextDouble() * 2f) - 1f;
            double expoenet = Math.Pow(2f, random.Next(-126, 128));
            return mantissa * expoenet;
        }
        public static double NextDouble(double minValue, double maxValue)
        {
            return RandomExtend.NextDouble() * (maxValue - minValue) + minValue;
        }
        public static float NextFloat()
        {
            return (float)RandomExtend.NextDouble();
        }
        public static float NextFloat(float minValue, float maxValue)
        {
            return RandomExtend.NextFloat() * (maxValue - minValue) + minValue;
        }

		/// <summary>Return a random point inside a giving angle degree with radius 1</summary>
		/// <param name="minAngle"></param>
		/// <param name="maxAngle"></param>
		/// <returns>An angle based on Vector2.up</returns>
		/// <remarks>Special thank for Uglysoft - Dan</remarks>
		public static Vector2 insideUnitAngle(float minAngle, float maxAngle)
		{
			// Radians 0, start on right, rotate 90 degree to top.
			float radians = Mathf.Deg2Rad * (Random.Range(minAngle, maxAngle) + 90f);
			return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
		}
    }
}

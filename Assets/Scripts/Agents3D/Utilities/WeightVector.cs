using UnityEngine;

namespace FlyAgent.Utilities
{
	/// <summary>A Vector with weight value, used for calculate centroid.</summary>
	/// <see cref="https://en.wikipedia.org/wiki/Centroid"/>
	public struct WeightVector
	{
		public float x, y, z, w;
		
		public WeightVector(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public WeightVector(Vector3 v, float w)
			: this(v.x, v.y, v.z, w) { }

		public WeightVector(Vector4 v)
			: this(v.x, v.y, v.z, v.w) { }

		public static readonly WeightVector zero = Vector4.zero;
		public Vector3 vector { get { return (Vector3)this; } }
		public float weight { get { return w; } }
		public Vector3 centroid { get { return weight > 0f ? vector / weight : Vector3.zero; } }

		public override string ToString()
		{
			return typeof(WeightVector).Name + "(" + x + "," + y + "," + z + "," + w + ")";
		}

		public static explicit operator Vector3(WeightVector v)
		{
			return new Vector3(v.x, v.y, v.z);
		}

		public static explicit operator float(WeightVector v)
		{
			return v.w;
		}

		public static implicit operator WeightVector(Vector4 v)
		{
			return new WeightVector(v.x, v.y, v.z, v.w);
		}

		public static implicit operator Vector4(WeightVector v)
		{
			return new Vector4(v.x, v.y, v.z, v.w);
		}

		public static WeightVector operator +(WeightVector lhs, WeightVector rhs)
		{
			return new WeightVector(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z, lhs.w + rhs.w);
		}
	}
}
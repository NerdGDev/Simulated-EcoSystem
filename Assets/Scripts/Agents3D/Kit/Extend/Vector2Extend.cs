using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Kit
{
	public static class Vector2Extend
	{
		#region cast to Vector2
		/// <summary>
		/// Cast Vector3 to Vector2 on a plane
		/// <see cref="http://answers.unity3d.com/questions/742205/how-to-cast-vector3-on-a-plane-to-get-vector2.html"/>
		/// </summary>
		/// <param name="self"></param>
		/// <param name="normal"></param>
		/// <returns></returns>
		public static Vector2 CastVector2ByNormal(this Vector3 self, Vector3 normal)
		{
			Vector3 d = self - self.PointOnDistance(normal, 1f);
			return new Vector2(Mathf.Sqrt(d.x * d.x + d.z * d.z), d.y);
		}
		#endregion

		/// <summary>
		/// <see cref="https://forum.unity3d.com/threads/making-a-square-vector2-fit-a-circle-vector2.422352/"/>
		/// </summary>
		/// <param name="input"></param>
		/// <param name="threshold"></param>
		/// <returns></returns>
		public static Vector2 ConvertCircleToSquare(this Vector2 input, float threshold = float.Epsilon)
		{
			const float COS_45 = 0.70710678f;
			float sqrThreshold = threshold * threshold;
			if (input.sqrMagnitude <= sqrThreshold)
			{
				return Vector2.zero;
			}

			Vector2 normal = input.normalized;
			float x, y;

			if (normal.x != 0f && normal.y >= -COS_45 && normal.y <= COS_45)
			{
				x = normal.x >= 0f ? input.x / normal.x : -input.x / normal.x;
			}
			else
			{
				x = input.x / Mathf.Abs(normal.y);
			}

			if (normal.y != 0f && normal.x >= -COS_45 && normal.x <= COS_45)
			{
				y = normal.y >= 0f ? input.y / normal.y : -input.y / normal.y;
			}
			else
			{
				y = input.y / Mathf.Abs(normal.x);
			}

			return new Vector2(x, y);
		}

		/// <summary>
		/// <see cref="http://amorten.com/blog/2017/mapping-square-input-to-circle-in-unity/"/>
		/// <see cref="http://mathproofs.blogspot.hk/2005/07/mapping-square-to-circle.html"/>
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static Vector2 ConvertSquareToCircle(this Vector2 input)
		{
			return new Vector2(
				input.x * Mathf.Sqrt(1f - input.y * input.y * 0.5f),
				input.y * Mathf.Sqrt(1f - input.x * input.x * 0.5f)
				);
		}
		
		public static Vector2 Scale(this Vector2 self, float fromMin, float fromMax, float toMin, float toMax)
		{
			return new Vector2(
				self.x.Scale(fromMin, fromMax, toMin, toMax),
				self.y.Scale(fromMin, fromMax, toMin, toMax)
				);
		}

		/// <summary>find angle between two vector, with signed</summary>
		/// <param name="lhs">from left hand side</param>
		/// <param name="rhs">to right hand side</param>
		/// <returns></returns>
		/// <see cref="http://referencesource.microsoft.com/#WindowsBase/Base/System/Windows/Vector.cs,102"/>
		/// <remarks>result can be flip, if you flip input (lhs ~ rhs)</remarks>
		public static float SignedAngle(this Vector2 lhs, Vector2 rhs)
		{
			lhs.Normalize();
			rhs.Normalize();
			var sin = rhs.x * lhs.y - lhs.x * rhs.y;
			var cos = lhs.x * rhs.x + lhs.y * rhs.y;
			return Mathf.Atan2(sin, cos) * Mathf.Rad2Deg;
		}
	}
}
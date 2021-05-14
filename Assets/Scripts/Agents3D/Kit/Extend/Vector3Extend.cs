using UnityEngine;

namespace Kit
{
	public static class Vector3Extend
    {
		#region Basic
		/// <summary>To find out this vector3 is Nan</summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static bool IsNaN(this Vector3 self)
		{
			return float.IsNaN(self.x) || float.IsNaN(self.y) || float.IsNaN(self.z);
		}

		public static bool IsInfinity(this Vector3 self)
		{
			return float.IsInfinity(self.x) || float.IsInfinity(self.y) || float.IsInfinity(self.z);
		}

		/// <summary>Compare all axis by <see cref="Mathf.Approximately(float, float)"/></summary>
		/// <param name="self"></param>
		/// <param name="target"></param>
		/// <returns>return true when it's close enought to each other.</returns>
        public static bool Approximately(this Vector3 self, Vector3 target)
        {
			return Mathf.Approximately(self.x, target.x) &&
				Mathf.Approximately(self.y, target.y) &&
				Mathf.Approximately(self.z, target.z);
        }
        /// <summary>Compare two Vector is roughly equal to each others</summary>
        /// <param name="self">Vector3</param>
        /// <param name="target">Vector3</param>
        /// <param name="threshold">The threshold value that can ignore.</param>
        /// <returns>true/false</returns>
        public static bool EqualRoughly(this Vector3 self, Vector3 target, float threshold = float.Epsilon)
        {
            return self.x.EqualRoughly(target.x, threshold) &&
                self.y.EqualRoughly(target.y, threshold) &&
                self.z.EqualRoughly(target.z, threshold);
        }
        /// <summary>Absolute value of vector</summary>
        /// <param name="self"></param>
        /// <returns></returns>
        /// <example>Vector3(2f,-1f,-100f) = Vector3(2f,1f,100f)</example>
        public static Vector3 Abs(this Vector3 self)
        {
            return new Vector3(Mathf.Abs(self.x),Mathf.Abs(self.y),Mathf.Abs(self.z));
        }
        /// <summary>Divide current Vector by the other</summary>
        /// <param name="self"></param>
        /// <param name="denominator"></param>
        /// <returns></returns>
        /// <example>Vector3(6,4,2).Divide(new Vector3(2,2,2)) == Vector3(3,2,1)</example>
        public static Vector3 Divide(this Vector3 self, Vector3 denominator)
        {
            return new Vector3(self.x / denominator.x, self.y / denominator.y, self.z / denominator.z);
        }
		#endregion

		#region Position
		/// <summary>Transforms position from local space to world space.</summary>
		/// <param name="position"></param>
		/// <param name="localRotate"></param>
		/// <param name="localScale"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		/// <remarks>As same as Transform.TransformPoint</remarks>
		/// <see cref="http://docs.unity3d.com/412/Documentation/ScriptReference/Transform.TransformPoint.html"/>
		/// <seealso cref="https://en.wikipedia.org/wiki/Transformation_matrix"/>
		public static Vector3 TransformPoint(this Vector3 position, Quaternion localRotate, Vector3 localScale, Vector3 offset)
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetTRS(position, localRotate, localScale);
            //return position + localRotate * Vector3.Scale(offset, localScale);
            // Why the fuck in the world your document didn't write it down.
            return matrix.MultiplyPoint3x4(offset);
        }
		/// <summary>Transforms position from local space to world space.</summary>
		/// <param name="position"></param>
		/// <param name="localRotate"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		/// <remarks>As same as Transform.TransformPoint</remarks>
		public static Vector3 TransformPoint(this Vector3 position, Quaternion localRotate, Vector3 offset)
        {
            return TransformPoint(position, localRotate, Vector3.one, offset);
        }
		/// <summary>Transforms position from world space to local space.</summary>
		/// <param name="position"></param>
		/// <param name="localRotate"></param>
		/// <param name="localScale"></param>
		/// <param name="targetPosition"></param>
		/// <returns></returns>
		public static Vector3 InverseTransformPoint(this Vector3 position, Quaternion localRotate, Vector3 localScale, Vector3 targetPosition)
        {
            // http://answers.unity3d.com/questions/1124805/world-to-local-matrix-without-transform.html
            // return (localRotate.Inverse() * (targetPosition - position)).Divide(localScale);

            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetTRS(position, localRotate, localScale);
            return matrix.inverse.MultiplyPoint(targetPosition);
        }
		/// <summary>Transforms position from world space to local space.</summary>
		/// <param name="position"></param>
		/// <param name="localRotate"></param>
		/// <param name="targetPosition"></param>
		/// <returns></returns>
		public static Vector3 InverseTransformPoint(this Vector3 position, Quaternion localRotate, Vector3 targetPosition)
		{
			return position.InverseTransformPoint(localRotate, Vector3.one, targetPosition);
		}
		#endregion

		#region Direction
		/// <summary>Transforms Direction from local space to world space</summary>
		/// <param name="position"></param>
		/// <param name="localRotate"></param>
		/// <param name="localScale"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public static Vector3 TransformDirection(this Vector3 position, Quaternion localRotate, Vector3 localScale, Vector3 direction)
        {
            return position.TransformVector(localRotate, Vector3.one, direction);
        }
        /// <summary>Transform Direction from world space to local space</summary>
        /// <param name="position"></param>
        /// <param name="localRotate"></param>
        /// <param name="localScale"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Vector3 InverseTransformDirection(this Vector3 position, Quaternion localRotate, Vector3 localScale, Vector3 direction)
        {
            return position.InverseTransformVector(localRotate, Vector3.one, direction);
        }
        /// <summary>Transform vector from local space to world space</summary>
        /// <param name="position"></param>
        /// <param name="localRotate"></param>
        /// <param name="localScale"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 TransformVector(this Vector3 position, Quaternion localRotate, Vector3 localScale, Vector3 vector)
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetTRS(position, localRotate, localScale);
            return matrix.MultiplyVector(vector);
        }
        /// <summary>Transforms vector from world space to local space</summary>
        /// <param name="position"></param>
        /// <param name="localRotate"></param>
        /// <param name="localScale"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 InverseTransformVector(this Vector3 position, Quaternion localRotate, Vector3 localScale, Vector3 vector)
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetTRS(position, localRotate, localScale);
            return matrix.inverse.MultiplyVector(vector);
        }
        /// <summary>Direction between 2 position</summary>
        /// <param name="from">Position</param>
        /// <param name="to">Position</param>
        /// <returns>Direction Vector</returns>
        public static Vector3 Direction(this Vector3 from, Vector3 to)
        {
            return (to - from).normalized;
        }
        /// <summary>Rotate X axis on current direction vector</summary>
        /// <param name="self"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Vector3 RotateX(this Vector3 self, float angle)
        {
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);

            float ty = self.y;
            float tz = self.z;
            self.y = (cos * ty) - (sin * tz);
            self.z = (cos * tz) + (sin * ty);
            return self;
        }
        /// <summary>Rotate Y axis on current direction vector</summary>
        /// <param name="self"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Vector3 RotateY(this Vector3 self, float angle)
        {
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);

            float tx = self.x;
            float tz = self.z;
            self.x = (cos * tx) + (sin * tz);
            self.z = (cos * tz) - (sin * tx);
            return self;
        }
        /// <summary>Rotate Z axis on current direction vector</summary>
        /// <param name="self"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Vector3 RotateZ(this Vector3 self, float angle)
        {
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);

            float tx = self.x;
            float ty = self.y;
            self.x = (cos * tx) - (sin * ty);
            self.y = (cos * ty) + (sin * tx);
            return self;
        }

		/// <summary>Returns vector projected to a plane and multiplied by weight</summary>
		/// <param name="tangent"></param>
		/// <param name="normal"></param>
		/// <param name="weight"></param>
		/// <returns></returns>
		public static Vector3 ExtractHorizontal(Vector3 tangent, Vector3 normal, float weight = 1f)
		{
			if (weight == 0f)
				return Vector3.zero;
			Vector3 copy = tangent;
			Vector3.OrthoNormalize(ref normal, ref copy);
			return Vector3.Project(tangent, copy) * weight;
		}

		/// <summary>Returns vector projection on axis multiplied by weight.</summary>
		/// <param name="tangent"></param>
		/// <param name="verticalAxis"></param>
		/// <param name="weight"></param>
		/// <returns></returns>
		public static Vector3 ExtractVertical(Vector3 tangent, Vector3 verticalAxis, float weight = 1f)
		{
			if (weight == 0f)
				return Vector3.zero;
			return Vector3.Project(tangent, verticalAxis) * weight;
		}

        /// <summary>Find the relative vector from giving angle & axis</summary>
        /// <param name="self"></param>
        /// <param name="angle">0~360</param>
        /// <param name="axis">Vector direction e.g. Vector.up</param>
        /// <param name="useRadians">0~360 = false, 0~1 = true</param>
        /// <returns></returns>
        public static Vector3 RotateAroundAxis(this Vector3 self, float angle, Vector3 axis, bool useRadians = false)
        {
            if (useRadians) angle *= Mathf.Rad2Deg;
            Quaternion q = Quaternion.AngleAxis(angle, axis);
            return (q * self);
        }
        #endregion

        #region Distance
        /// <summary>Distance between two position</summary>
        /// <param name="self"></param>
        /// <param name="position"></param>
        /// <returns>Disatnce</returns>
        /// <see cref="http://answers.unity3d.com/questions/384932/best-way-to-find-distance.html"/>
        /// <seealso cref="http://forum.unity3d.com/threads/square-root-runs-1000-times-in-0-01ms.147661/"/>
        public static float Distance(this Vector3 self, Vector3 position)
        {
			//position -= self;
			//return Mathf.Sqrt( position.x * position.x + position.y * position.y + position.z * position.z);
			return Vector3.Distance(self, position);
        }
        /// <summary>Return lerp Vector3 by giving distance</summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector3 LerpByDistance(Vector3 start, Vector3 end, float distance)
        {
            return distance * (end - start) + start;
        }
        /// <summary>Use start position, direction and known distance to find the end point position</summary>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <returns>End point position</returns>
        public static Vector3 PointOnDistance(this Vector3 position, Vector3 direction, float distance)
        {
            return position + (direction * distance);
        }
        #endregion

        #region Angle
        /// <summary>find angle between 2 position, using itself as center</summary>
        /// <param name="center"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static float AngleBetweenPosition(this Vector3 center, Vector3 point1, Vector3 point2)
        {
            return Vector3.Angle((point1 - center), (point2 - center));
        }

        /// <summary>Determine the signed angle between two vectors, with normal as the rotation axis.</summary>
        /// <example>Vector3.AngleBetweenDirectionSigned(Vector3.forward,Vector3.right)</example>
        /// <param name="direction1">Direction vector</param>
        /// <param name="direction2">Direction vector</param>
        /// <param name="normal">normal vector e.g. AxisXZ = Vector3.Cross(Vector3.forward, Vector3.right);</param>
        /// <see cref="http://forum.unity3d.com/threads/need-vector3-angle-to-return-a-negtive-or-relative-value.51092/"/>
        /// <see cref="http://stackoverflow.com/questions/19675676/calculating-actual-angle-between-two-vectors-in-unity3d"/>
        public static float AngleBetweenDirectionSigned(this Vector3 direction1, Vector3 direction2, Vector3 normal)
        {
            return Mathf.Rad2Deg * Mathf.Atan2(Vector3.Dot(normal, Vector3.Cross(direction1, direction2)), Vector3.Dot(direction1, direction2));
            // return Vector3.Angle(direction1, direction2) * Mathf.Sign(Vector3.Dot(normal, Vector3.Cross(direction1, direction2)));
        }
		#endregion

		#region Intersect
		/// <summary>Check if giving ray was intersect current sphere.</summary>
		/// <param name="ray"></param>
		/// <param name="origin"></param>
		/// <param name="radius"></param>
		/// <param name="distance"></param>
		/// <param name="intersection"></param>
		/// <returns></returns>
		/// <see cref="http://answers.unity3d.com/questions/745560/handle-for-clickable-scene-objects.html"/>
		public static bool IntersectRaySphere(Ray ray, Vector3 origin, float radius, ref float distance, ref Vector3 intersection)
		{
			Vector3 m = ray.origin - origin;
			float b = Vector3.Dot(m, ray.direction);
			float c = Vector3.Dot(m, m) - (radius * radius);
			// Exit if rís origin outside s (c > 0)and r pointing away from s (b > 0)
			if ((c > 0.0f) && (b > 0.0f)) return false;
			float discr = (b * b) - c;

			// A negative discriminant corresponds to ray missing sphere
			if (discr < 0.0f) return false;

			// Ray now found to intersect sphere, compute smallest distance value of intersection
			distance = -b - Mathf.Sqrt(discr);

			// If t is negative, ray started inside sphere so clamp distance to zero
			if (distance < 0.0f) distance = 0.0f;
			intersection = ray.origin + distance * ray.direction;
			return true;
		}

		public static bool IntersectRaySphere(Ray ray, Vector3 origin, float radius)
		{
			Vector3 hitPosition = Vector3.zero;
			float distance = 0f;
			return IntersectRaySphere(ray, origin, radius, ref distance, ref hitPosition);
		}

		/// <summary>Giving point and ray to calculate the intersect point on target surface normal.</summary>
		/// <param name="ray"></param>
		/// <param name="pointOnSurface"></param>
		/// <param name="surfaceNormal"></param>
		/// <see cref="https://www.gamedev.net/articles/programming/math-and-physics/practical-use-of-vector-math-in-games-r2968/"/>
		/// <returns></returns>
		public static Vector3 IntersectPointOnPlane(Ray ray, Vector3 pointOnSurface, Vector3 surfaceNormal)
		{
			float dot1 = Vector3.Dot(surfaceNormal, pointOnSurface - ray.origin);
			float dot2 = Vector3.Dot(surfaceNormal, ray.direction);
			float u = dot1 / dot2;
			return ray.origin + (ray.direction * u);
		}
		#endregion
	}
}

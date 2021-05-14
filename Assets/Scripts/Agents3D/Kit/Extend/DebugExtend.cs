using UnityEngine;

/// <summary>
/// Debug Extension
/// 	- Static class that extends Unity's debugging functionallity.
/// </summary>

namespace Kit
{
    public static class DebugExtend
    {
        #region DebugDrawFunctions
        /// <summary>- Debugs a point.</summary>
        /// <param name='position'>- The point to debug.</param>
        /// <param name='color'>- The color of the point.</param>
        /// <param name='scale'>- The size of the point.</param>
        /// <param name='duration'>- How long to draw the point.</param>
        /// <param name='depthTest'>- Whether or not this point should be faded when behind other objects.</param>
        public static void DrawPoint(Vector3 position, Color color = default(Color), float scale = 1.0f, float duration = 0, bool depthTest = false)
        {
            color = (color == default(Color)) ? Color.white : color;
            Debug.DrawRay(position + (Vector3.up * (scale * 0.5f)), -Vector3.up * scale, color, duration, depthTest);
            Debug.DrawRay(position + (Vector3.right * (scale * 0.5f)), -Vector3.right * scale, color, duration, depthTest);
            Debug.DrawRay(position + (Vector3.forward * (scale * 0.5f)), -Vector3.forward * scale, color, duration, depthTest);
        }

		public static void DrawRay(Vector3 position, Vector3 direction, Color color = default(Color), float duration = 0, bool depthTest = false)
		{
			color = (color == default(Color)) ? Color.white : color;
			Debug.DrawLine(position, position.PointOnDistance(direction, direction.magnitude), color, duration, depthTest);
		}

		public static void DrawLine(Vector3 point1, Vector3 point2, Color color = default(Color), float duration = 0, bool depthTest = false)
		{
			color = (color == default(Color)) ? Color.white : color;
			Debug.DrawLine(point1, point2, color, duration, depthTest);
		}

		/// <summary>- Debugs an axis-aligned bounding box.</summary>
		/// <param name='bounds'>- The bounds to debug.</param>
		/// <param name='color'>- The color of the bounds.</param>
		/// <param name='duration'>- How long to draw the bounds.</param>
		/// <param name='depthTest'>- Whether or not the bounds should be faded when behind other objects.</param>
		public static void DrawBounds(Bounds bounds, Color color = default(Color), float duration = 0, bool depthTest = false)
        {
            color = (color == default(Color)) ? Color.white : color;
            Vector3
                ruf = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z),
                rub = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z),
                luf = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z),
                lub = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z),
                rdf = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z),
                rdb = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z),
                lfd = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z),
                lbd = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z);

            Debug.DrawLine(ruf, luf, color, duration, depthTest);
            Debug.DrawLine(ruf, rub, color, duration, depthTest);
            Debug.DrawLine(luf, lub, color, duration, depthTest);
            Debug.DrawLine(rub, lub, color, duration, depthTest);

            Debug.DrawLine(ruf, rdf, color, duration, depthTest);
            Debug.DrawLine(rub, rdb, color, duration, depthTest);
            Debug.DrawLine(luf, lfd, color, duration, depthTest);
            Debug.DrawLine(lub, lbd, color, duration, depthTest);

            Debug.DrawLine(rdf, lfd, color, duration, depthTest);
            Debug.DrawLine(rdf, rdb, color, duration, depthTest);
            Debug.DrawLine(lfd, lbd, color, duration, depthTest);
            Debug.DrawLine(lbd, rdb, color, duration, depthTest);
        }


        /// <summary>- Debugs a local cube.</summary>
        /// <param name='transform'>- The transform that the cube will be local to.</param>
        /// <param name='size'>- The size of the cube.</param>
        /// <param name='color'>- Color of the cube.</param>
        /// <param name='center'>- The position (relative to transform) where the cube will be debugged.</param>
        /// <param name='duration'>- How long to draw the cube.</param>
        /// <param name='depthTest'>- Whether or not the cube should be faded when behind other objects.</param>
        public static void DrawLocalCube(Transform transform, Vector3 size = default(Vector3), Color color = default(Color), Vector3 center = default(Vector3), float duration = 0, bool depthTest = false)
        {
            size = (size == default(Vector3)) ? Vector3.one : size;
            color = (color == default(Color)) ? Color.white : color;
            Vector3
                lbb = transform.TransformPoint(center + ((-size) * 0.5f)),
                rbb = transform.TransformPoint(center + (new Vector3(size.x, -size.y, -size.z) * 0.5f)),
                lbf = transform.TransformPoint(center + (new Vector3(size.x, -size.y, size.z) * 0.5f)),
                rbf = transform.TransformPoint(center + (new Vector3(-size.x, -size.y, size.z) * 0.5f)),
                lub = transform.TransformPoint(center + (new Vector3(-size.x, size.y, -size.z) * 0.5f)),
                rub = transform.TransformPoint(center + (new Vector3(size.x, size.y, -size.z) * 0.5f)),
                luf = transform.TransformPoint(center + ((size) * 0.5f)),
                ruf = transform.TransformPoint(center + (new Vector3(-size.x, size.y, size.z) * 0.5f));

            Debug.DrawLine(lbb, rbb, color, duration, depthTest);
            Debug.DrawLine(rbb, lbf, color, duration, depthTest);
            Debug.DrawLine(lbf, rbf, color, duration, depthTest);
            Debug.DrawLine(rbf, lbb, color, duration, depthTest);

            Debug.DrawLine(lub, rub, color, duration, depthTest);
            Debug.DrawLine(rub, luf, color, duration, depthTest);
            Debug.DrawLine(luf, ruf, color, duration, depthTest);
            Debug.DrawLine(ruf, lub, color, duration, depthTest);

            Debug.DrawLine(lbb, lub, color, duration, depthTest);
            Debug.DrawLine(rbb, rub, color, duration, depthTest);
            Debug.DrawLine(lbf, luf, color, duration, depthTest);
            Debug.DrawLine(rbf, ruf, color, duration, depthTest);
        }

        /// <summary>- Debugs a local cube.</summary>
        /// <param name='space'>- The space the cube will be local to.</param>
        /// <param name='size'>- The size of the cube.</param>
        /// <param name='color'>- Color of the cube.</param>
        /// <param name='center'>- The position (relative to transform) where the cube will be debugged.</param>
        /// <param name='duration'>- How long to draw the cube.</param>
        /// <param name='depthTest'>- Whether or not the cube should be faded when behind other objects.</param>
        public static void DrawLocalCube(Matrix4x4 space, Vector3 size = default(Vector3), Color color = default(Color), Vector3 center = default(Vector3), float duration = 0, bool depthTest = false)
        {
            size = (size == default(Vector3)) ? Vector3.one : size;
            color = (color == default(Color)) ? Color.white : color;
            Vector3
                lbb = space.MultiplyPoint3x4(center + ((-size) * 0.5f)),
                rbb = space.MultiplyPoint3x4(center + (new Vector3(size.x, -size.y, -size.z) * 0.5f)),
                lbf = space.MultiplyPoint3x4(center + (new Vector3(size.x, -size.y, size.z) * 0.5f)),
                rbf = space.MultiplyPoint3x4(center + (new Vector3(-size.x, -size.y, size.z) * 0.5f)),
                lub = space.MultiplyPoint3x4(center + (new Vector3(-size.x, size.y, -size.z) * 0.5f)),
                rub = space.MultiplyPoint3x4(center + (new Vector3(size.x, size.y, -size.z) * 0.5f)),
                luf = space.MultiplyPoint3x4(center + ((size) * 0.5f)),
                ruf = space.MultiplyPoint3x4(center + (new Vector3(-size.x, size.y, size.z) * 0.5f));

            Debug.DrawLine(lbb, rbb, color, duration, depthTest);
            Debug.DrawLine(rbb, lbf, color, duration, depthTest);
            Debug.DrawLine(lbf, rbf, color, duration, depthTest);
            Debug.DrawLine(rbf, lbb, color, duration, depthTest);

            Debug.DrawLine(lub, rub, color, duration, depthTest);
            Debug.DrawLine(rub, luf, color, duration, depthTest);
            Debug.DrawLine(luf, ruf, color, duration, depthTest);
            Debug.DrawLine(ruf, lub, color, duration, depthTest);

            Debug.DrawLine(lbb, lub, color, duration, depthTest);
            Debug.DrawLine(rbb, rub, color, duration, depthTest);
            Debug.DrawLine(lbf, luf, color, duration, depthTest);
            Debug.DrawLine(rbf, ruf, color, duration, depthTest);
        }
        
        /// <summary>- Debugs a circle.</summary>
        /// <param name='position'>- Where the center of the circle will be positioned.</param>
        /// <param name='up'>- The direction perpendicular to the surface of the circle.</param>
        /// <param name='color'>- The color of the circle.</param>
        /// <param name='radius'>- The radius of the circle.</param>
        /// <param name='duration'>- How long to draw the circle.</param>
        /// <param name='depthTest'>- Whether or not the circle should be faded when behind other objects.</param>
        public static void DrawCircle(Vector3 position, Vector3 up, Color color = default(Color), float radius = 1.0f, float duration = 0, bool depthTest = false)
        {
            up = ((up == default(Vector3)) ? Vector3.up : up).normalized * radius;
            Vector3
                forward = Vector3.Slerp(up, -up, 0.5f),
                right = Vector3.Cross(up, forward).normalized * radius;

            Matrix4x4 matrix = new Matrix4x4()
            {
                m00 = right.x,
                m10 = right.y,
                m20 = right.z,

                m01 = up.x,
                m11 = up.y,
                m21 = up.z,

                m02 = forward.x,
                m12 = forward.y,
                m22 = forward.z
            };

            Vector3
                lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0))),
                nextPoint = Vector3.zero;

            color = (color == default(Color)) ? Color.white : color;

            for (int i = 0; i <= 90; i++)
            {
                nextPoint = position + matrix.MultiplyPoint3x4(
                    new Vector3(
                        Mathf.Cos((i * 4) * Mathf.Deg2Rad),
                        0f,
                        Mathf.Sin((i * 4) * Mathf.Deg2Rad)
                        )
                    );
                Debug.DrawLine(lastPoint, nextPoint, color, duration, depthTest);
                lastPoint = nextPoint;
            }
        }

        /// <summary>- Debugs a wire sphere.</summary>
        /// <param name='position'>- The position of the center of the sphere.</param>
        /// <param name='color'>- The color of the sphere.</param>
        /// <param name='radius'>- The radius of the sphere.</param>
        /// <param name='duration'>- How long to draw the sphere.</param>
        /// <param name='depthTest'>- Whether or not the sphere should be faded when behind other objects.</param>
        public static void DrawWireSphere(Vector3 position, Color color = default(Color), float radius = 1.0f, float duration = 0, bool depthTest = false)
        {
            float angle = 10.0f;

            Vector3
                x = new Vector3(position.x, position.y + radius * Mathf.Sin(0), position.z + radius * Mathf.Cos(0)),
                y = new Vector3(position.x + radius * Mathf.Cos(0), position.y, position.z + radius * Mathf.Sin(0)),
                z = new Vector3(position.x + radius * Mathf.Cos(0), position.y + radius * Mathf.Sin(0), position.z),
                new_x,new_y,new_z;

            color = (color == default(Color)) ? Color.white : color;

            for (int i = 1; i <= 36; i++)
            {
                new_x = new Vector3(position.x, position.y + radius * Mathf.Sin(angle * i * Mathf.Deg2Rad), position.z + radius * Mathf.Cos(angle * i * Mathf.Deg2Rad));
                new_y = new Vector3(position.x + radius * Mathf.Cos(angle * i * Mathf.Deg2Rad), position.y, position.z + radius * Mathf.Sin(angle * i * Mathf.Deg2Rad));
                new_z = new Vector3(position.x + radius * Mathf.Cos(angle * i * Mathf.Deg2Rad), position.y + radius * Mathf.Sin(angle * i * Mathf.Deg2Rad), position.z);

                Debug.DrawLine(x, new_x, color, duration, depthTest);
                Debug.DrawLine(y, new_y, color, duration, depthTest);
                Debug.DrawLine(z, new_z, color, duration, depthTest);

                x = new_x;
                y = new_y;
                z = new_z;
            }
        }

        
        /// <summary>- Debugs a cylinder.</summary>
        /// <param name='start'>- The position of one end of the cylinder.</param>
        /// <param name='end'>- The position of the other end of the cylinder.</param>
        /// <param name='color'>- The color of the cylinder.</param>
        /// <param name='radius'>- The radius of the cylinder.</param>
        /// <param name='duration'>- How long to draw the cylinder.</param>
        /// <param name='depthTest'>- Whether or not the cylinder should be faded when behind other objects.</param>
        public static void DrawCylinder(Vector3 start, Vector3 end, Color color = default(Color), float radius = 1, float duration = 0, bool depthTest = false)
        {
            Vector3
                up = (end - start).normalized * radius,
                forward = Vector3.Slerp(up, -up, 0.5f),
                right = Vector3.Cross(up, forward).normalized * radius;

            color = (color == default(Color)) ? Color.white : color;

            //Radial circles
            DebugExtend.DrawCircle(start, up, color, radius, duration, depthTest);
            DebugExtend.DrawCircle(end, -up, color, radius, duration, depthTest);
            DebugExtend.DrawCircle((start + end) * 0.5f, up, color, radius, duration, depthTest);

            //Side lines
            Debug.DrawLine(start + right, end + right, color, duration, depthTest);
            Debug.DrawLine(start - right, end - right, color, duration, depthTest);

            Debug.DrawLine(start + forward, end + forward, color, duration, depthTest);
            Debug.DrawLine(start - forward, end - forward, color, duration, depthTest);

            //Start endcap
            Debug.DrawLine(start - right, start + right, color, duration, depthTest);
            Debug.DrawLine(start - forward, start + forward, color, duration, depthTest);

            //End endcap
            Debug.DrawLine(end - right, end + right, color, duration, depthTest);
            Debug.DrawLine(end - forward, end + forward, color, duration, depthTest);
        }

        /// <summary>- Debugs a cone.</summary>
        /// <param name='position'>- The position for the tip of the cone.</param>
        /// <param name='direction'>- The direction for the cone gets wider in.</param>
        /// <param name='angle'>- The angle of the cone.</param>
        /// <param name='color'>- The color of the cone.</param>
        /// <param name='duration'>- How long to draw the cone.</param>
        /// <param name='depthTest'>- Whether or not the cone should be faded when behind other objects.</param>
        public static void DrawCone(Vector3 position, Vector3 direction, Color color = default(Color), float angle = 45f, float duration = 0, bool depthTest = false)
        {
            float length = direction.magnitude;
            direction = direction.normalized;

            Vector3
                forward = direction,
                up = Vector3.Slerp(forward, -forward, 0.5f),
                right = Vector3.Cross(forward, up).normalized * length,

                slerpedVector = Vector3.Slerp(forward, up, angle / 90.0f);

            Plane farPlane = new Plane(-direction, position + forward);
            Ray distRay = new Ray(position, slerpedVector);

            float dist;
            farPlane.Raycast(distRay, out dist);

            color = (color == default(Color)) ? Color.white : color;

            Debug.DrawRay(position, slerpedVector.normalized * dist, color);
            Debug.DrawRay(position, Vector3.Slerp(forward, -up, angle / 90.0f).normalized * dist, color, duration, depthTest);
            Debug.DrawRay(position, Vector3.Slerp(forward, right, angle / 90.0f).normalized * dist, color, duration, depthTest);
            Debug.DrawRay(position, Vector3.Slerp(forward, -right, angle / 90.0f).normalized * dist, color, duration, depthTest);

            DebugExtend.DrawCircle(position + forward, direction, color, (forward - (slerpedVector.normalized * dist)).magnitude, duration, depthTest);
            DebugExtend.DrawCircle(position + (forward * 0.5f), direction, color, ((forward * 0.5f) - (slerpedVector.normalized * (dist * 0.5f))).magnitude, duration, depthTest);
        }

        /// <summary>- Debugs an arrow.</summary>
        /// <param name='start'>- The start position of the arrow.</param>
        /// <param name='end'>- The end position of the arrow.</param>
        /// <param name='color'>- The color of the arrow.</param>
        /// <param name='duration'>- How long to draw the arrow.</param>
        /// <param name='depthTest'>- Whether or not the arrow should be faded when behind other objects.</param>
        public static void DrawArrow(Vector3 start, Vector3 end, Color color = default(Color), float angle = 15f, float duration = 0, bool depthTest = false)
        {
            color = (color == default(Color)) ? Color.white : color;
			Debug.DrawLine(start, end, color);
			float side = 0.5f + Mathf.Clamp01((start - end).sqrMagnitude / 100f);
			Quaternion rot = Quaternion.LookRotation(start - end);
			Debug.DrawLine(end + (rot * Quaternion.Euler(0f, angle, 0f) * Vector3.forward * side), end, color, duration, depthTest);
			Debug.DrawLine(end + (rot * Quaternion.Euler(0f, -angle, 0f) * Vector3.forward * side), end, color, duration, depthTest);
        }

        /// <summary>- Debugs a capsule.</summary>
        /// <param name='start'>- The position of one end of the capsule.</param>
        /// <param name='end'>- The position of the other end of the capsule.</param>
        /// <param name='color'>- The color of the capsule.</param>
        /// <param name='radius'>- The radius of the capsule.</param>
        /// <param name='duration'>- How long to draw the capsule.</param>
        /// <param name='depthTest'>- Whether or not the capsule should be faded when behind other objects.</param>
        public static void DrawCapsule(Vector3 start, Vector3 end, Color color = default(Color), float radius = 1f, float duration = 0f, bool depthTest = false)
        {
            float
                height = (start - end).magnitude,
                sideLength = Mathf.Max(0, (height * 0.5f) - radius);

            Vector3
                up = (end - start).normalized * radius,
                forward = Vector3.Slerp(up, -up, 0.5f),
                right = Vector3.Cross(up, forward).normalized * radius,
                middle = (end + start) * 0.5f;

            start = middle + ((start - middle).normalized * sideLength);
            end = middle + ((end - middle).normalized * sideLength);

            color = (color == default(Color)) ? Color.white : color;

            //Radial circles
            DebugExtend.DrawCircle(start, up, color, radius, duration, depthTest);
            DebugExtend.DrawCircle(end, -up, color, radius, duration, depthTest);

            //Side lines
            Debug.DrawLine(start + right, end + right, color, duration, depthTest);
            Debug.DrawLine(start - right, end - right, color, duration, depthTest);

            Debug.DrawLine(start + forward, end + forward, color, duration, depthTest);
            Debug.DrawLine(start - forward, end - forward, color, duration, depthTest);

            for (int i = 1; i < 26; i++)
            {
                //Start endcap
                Debug.DrawLine(Vector3.Slerp(right, -up, i / 25.0f) + start, Vector3.Slerp(right, -up, (i - 1) / 25.0f) + start, color, duration, depthTest);
                Debug.DrawLine(Vector3.Slerp(-right, -up, i / 25.0f) + start, Vector3.Slerp(-right, -up, (i - 1) / 25.0f) + start, color, duration, depthTest);
                Debug.DrawLine(Vector3.Slerp(forward, -up, i / 25.0f) + start, Vector3.Slerp(forward, -up, (i - 1) / 25.0f) + start, color, duration, depthTest);
                Debug.DrawLine(Vector3.Slerp(-forward, -up, i / 25.0f) + start, Vector3.Slerp(-forward, -up, (i - 1) / 25.0f) + start, color, duration, depthTest);

                //End endcap
                Debug.DrawLine(Vector3.Slerp(right, up, i / 25.0f) + end, Vector3.Slerp(right, up, (i - 1) / 25.0f) + end, color, duration, depthTest);
                Debug.DrawLine(Vector3.Slerp(-right, up, i / 25.0f) + end, Vector3.Slerp(-right, up, (i - 1) / 25.0f) + end, color, duration, depthTest);
                Debug.DrawLine(Vector3.Slerp(forward, up, i / 25.0f) + end, Vector3.Slerp(forward, up, (i - 1) / 25.0f) + end, color, duration, depthTest);
                Debug.DrawLine(Vector3.Slerp(-forward, up, i / 25.0f) + end, Vector3.Slerp(-forward, up, (i - 1) / 25.0f) + end, color, duration, depthTest);
            }
        }

        #endregion
    }
}
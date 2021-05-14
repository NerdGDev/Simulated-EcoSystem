using UnityEngine;
namespace Kit
{
	/// <summary>Gizmo Extension - Static class that extends Unity's gizmo functionallity.</summary>
	public static class GizmosExtend
	{
		#region GizmoDrawFunctions
		/// <summary>Override DrawLine</summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="color"></param>
		public static void DrawLine(Vector3 from, Vector3 to, Color color = default(Color))
		{
			Color oldColor = Gizmos.color;
			if (color != default(Color)) Gizmos.color = color;
			Gizmos.DrawLine(from, to);
			if (color != default(Color)) Gizmos.color = oldColor;
		}

		/// <summary>- Draws a point.</summary>
		/// <param name='position'>- The point to draw.</param>
		///  <param name='color'>- The color of the drawn point.</param>
		/// <param name='scale'>- The size of the drawn point.</param>
		public static void DrawPoint(Vector3 position, Color color = default(Color), float scale = 1.0f)
		{
			Color oldColor = Gizmos.color;

			if (color != default(Color)) Gizmos.color = color;
			Gizmos.DrawRay(position + (Vector3.up * (scale * 0.5f)), -Vector3.up * scale);
			Gizmos.DrawRay(position + (Vector3.right * (scale * 0.5f)), -Vector3.right * scale);
			Gizmos.DrawRay(position + (Vector3.forward * (scale * 0.5f)), -Vector3.forward * scale);

			if (color != default(Color)) Gizmos.color = oldColor;
		}

		/// <summary>- Draws an axis-aligned bounding box.</summary>
		/// <param name='bounds'>- The bounds to draw.</param>
		/// <param name='color'>- The color of the bounds.</param>
		public static void DrawBounds(Bounds bounds, Color color = default(Color))
		{
			Vector3
				ruf = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z),
				rub = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z),
				luf = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z),
				lub = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z),
				rdf = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z),
				rdb = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z),
				lfd = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z),
				lbd = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z);

			Color oldColor = Gizmos.color;
			if (color != default(Color)) Gizmos.color = color;

			Gizmos.DrawLine(ruf, luf);
			Gizmos.DrawLine(ruf, rub);
			Gizmos.DrawLine(luf, lub);
			Gizmos.DrawLine(rub, lub);

			Gizmos.DrawLine(ruf, rdf);
			Gizmos.DrawLine(rub, rdb);
			Gizmos.DrawLine(luf, lfd);
			Gizmos.DrawLine(lub, lbd);

			Gizmos.DrawLine(rdf, lfd);
			Gizmos.DrawLine(rdf, rdb);
			Gizmos.DrawLine(lfd, lbd);
			Gizmos.DrawLine(lbd, rdb);

			if (color != default(Color)) Gizmos.color = oldColor;
		}

		private static void DrawLocalCube(ref Color color, ref Vector3 lbb, ref Vector3 rbb, ref Vector3 lbf, ref Vector3 rbf, ref Vector3 lub, ref Vector3 rub, ref Vector3 luf, ref Vector3 ruf)
		{
			Color oldColor = Gizmos.color;
			if (color != default(Color)) Gizmos.color = color;

			Gizmos.DrawLine(lbb, rbb);
			Gizmos.DrawLine(rbb, lbf);
			Gizmos.DrawLine(lbf, rbf);
			Gizmos.DrawLine(rbf, lbb);

			Gizmos.DrawLine(lub, rub);
			Gizmos.DrawLine(rub, luf);
			Gizmos.DrawLine(luf, ruf);
			Gizmos.DrawLine(ruf, lub);

			Gizmos.DrawLine(lbb, lub);
			Gizmos.DrawLine(rbb, rub);
			Gizmos.DrawLine(lbf, luf);
			Gizmos.DrawLine(rbf, ruf);

			if (color != default(Color)) Gizmos.color = oldColor;
		}

		/// <summary>- Draws a local cube.</summary>
		/// <param name='transform'>- The transform the cube will be local to.</param>
		/// <param name='size'>- The local size of the cube.</param>
		/// <param name='center'>- The local position of the cube.</param>
		/// <param name='color'>- The color of the cube.</param>
		public static void DrawLocalCube(Transform transform, Vector3 size, Color color = default(Color), Vector3 center = default(Vector3))
		{
			size = (size == default(Vector3)) ? Vector3.one : size;
			Vector3
				lbb = transform.TransformPoint(center + ((-size) * 0.5f)),
				rbb = transform.TransformPoint(center + (new Vector3(size.x, -size.y, -size.z) * 0.5f)),
				lbf = transform.TransformPoint(center + (new Vector3(size.x, -size.y, size.z) * 0.5f)),
				rbf = transform.TransformPoint(center + (new Vector3(-size.x, -size.y, size.z) * 0.5f)),
				lub = transform.TransformPoint(center + (new Vector3(-size.x, size.y, -size.z) * 0.5f)),
				rub = transform.TransformPoint(center + (new Vector3(size.x, size.y, -size.z) * 0.5f)),
				luf = transform.TransformPoint(center + ((size) * 0.5f)),
				ruf = transform.TransformPoint(center + (new Vector3(-size.x, size.y, size.z) * 0.5f));

			DrawLocalCube(ref color, ref lbb, ref rbb, ref lbf, ref rbf, ref lub, ref rub, ref luf, ref ruf);
		}

		/// <summary>- Draws a local cube.</summary>
		/// <param name='space'>- The space the cube will be local to.</param>
		/// <param name='size'>- The local size of the cube.</param>
		/// <param name='center'>- The local position of the cube.</param>
		/// <param name='color'>- The color of the cube.</param>
		public static void DrawLocalCube(Matrix4x4 space, Vector3 size = default(Vector3), Color color = default(Color), Vector3 center = default(Vector3))
		{
			size = (size == default(Vector3)) ? Vector3.one : size;
			Vector3
				lbb = space.MultiplyPoint3x4(center + ((-size) * 0.5f)),
				rbb = space.MultiplyPoint3x4(center + (new Vector3(size.x, -size.y, -size.z) * 0.5f)),
				lbf = space.MultiplyPoint3x4(center + (new Vector3(size.x, -size.y, size.z) * 0.5f)),
				rbf = space.MultiplyPoint3x4(center + (new Vector3(-size.x, -size.y, size.z) * 0.5f)),
				lub = space.MultiplyPoint3x4(center + (new Vector3(-size.x, size.y, -size.z) * 0.5f)),
				rub = space.MultiplyPoint3x4(center + (new Vector3(size.x, size.y, -size.z) * 0.5f)),
				luf = space.MultiplyPoint3x4(center + ((size) * 0.5f)),
				ruf = space.MultiplyPoint3x4(center + (new Vector3(-size.x, size.y, size.z) * 0.5f));

			DrawLocalCube(ref color, ref lbb, ref rbb, ref lbf, ref rbf, ref lub, ref rub, ref luf, ref ruf);
		}

		public static void DrawLocalCube(Vector3 position, Quaternion rotation, Vector3 size = default(Vector3), Color color = default(Color))
		{
			size = (size == default(Vector3)) ? Vector3.one : size;
			Matrix4x4 matrix = new Matrix4x4();
			matrix.SetTRS(position, rotation, Vector3.one);
			DrawLocalCube(matrix, size, color, Vector3.zero);
		}

		/// <summary>- Draws a circle.</summary>
		/// <param name='position'>- Where the center of the circle will be positioned.</param>
		/// <param name='up'>- The direction perpendicular to the surface of the circle.</param>
		/// <param name='color'>- The color of the circle.</param>
		/// <param name='radius'>- The radius of the circle.</param>
		public static void DrawCircle(Vector3 position, Vector3 up = default(Vector3), Color color = default(Color), float radius = 1.0f)
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

			Color oldColor = Gizmos.color;
			if (color != default(Color)) Gizmos.color = color;

			for (int i = 0; i <= 90; i++)
			{
				nextPoint = position + matrix.MultiplyPoint3x4(
					new Vector3(
						Mathf.Cos((i * 4) * Mathf.Deg2Rad),
						0f,
						Mathf.Sin((i * 4) * Mathf.Deg2Rad)
						)
					);
				Gizmos.DrawLine(lastPoint, nextPoint);
				lastPoint = nextPoint;
			}
			if (color != default(Color)) Gizmos.color = oldColor;
		}

		/// <summary>- Draws a cylinder.</summary>
		/// <param name='start'>- The position of one end of the cylinder.</param>
		/// <param name='end'>- The position of the other end of the cylinder.</param>
		/// <param name='color'>- The color of the cylinder.</param>
		/// <param name='radius'>- The radius of the cylinder.</param>
		public static void DrawCylinder(Vector3 start, Vector3 end, Color color = default(Color), float radius = 1.0f)
		{
			Vector3
				up = (end - start).normalized * radius,
				forward = Vector3.Slerp(up, -up, 0.5f),
				right = Vector3.Cross(up, forward).normalized * radius;

			//Radial circles
			DrawCircle(start, up, color, radius);
			DrawCircle(end, -up, color, radius);
			DrawCircle((start + end) * 0.5f, up, color, radius);

			Color oldColor = Gizmos.color;
			if (color != default(Color)) Gizmos.color = color;

			//Side lines
			Gizmos.DrawLine(start + right, end + right);
			Gizmos.DrawLine(start - right, end - right);

			Gizmos.DrawLine(start + forward, end + forward);
			Gizmos.DrawLine(start - forward, end - forward);

			//Start endcap
			Gizmos.DrawLine(start - right, start + right);
			Gizmos.DrawLine(start - forward, start + forward);

			//End endcap
			Gizmos.DrawLine(end - right, end + right);
			Gizmos.DrawLine(end - forward, end + forward);

			if (color != default(Color)) Gizmos.color = oldColor;
		}

		/// <summary>- Draws a cone.</summary>
		/// <param name='position'>- The position for the tip of the cone.</param>
		/// <param name='direction'>- The direction for the cone to get wider in.</param>
		/// <param name='color'>- The color of the cone.</param>
		/// <param name='angle'>- The angle of the cone.</param>
		public static void DrawCone(Vector3 position, Vector3 direction, Color color = default(Color), float angle = 45)
		{
			float length = direction.magnitude;
			direction = direction.normalized;
			angle = Mathf.Clamp(angle, 0f, 90f);

			Vector3
				forward = direction,
				up = Vector3.Slerp(forward, -forward, 0.5f),
				right = Vector3.Cross(forward, up).normalized * length,
				slerpedVector = Vector3.Slerp(forward, up, angle / 90.0f);

			Plane farPlane = new Plane(-direction, position + forward);
			Ray distRay = new Ray(position, slerpedVector);

			float dist;
			farPlane.Raycast(distRay, out dist);

			Color oldColor = Gizmos.color;
			if (color != default(Color)) Gizmos.color = color;

			Gizmos.DrawRay(position, slerpedVector.normalized * dist);
			Gizmos.DrawRay(position, Vector3.Slerp(forward, -up, angle / 90.0f).normalized * dist);
			Gizmos.DrawRay(position, Vector3.Slerp(forward, right, angle / 90.0f).normalized * dist);
			Gizmos.DrawRay(position, Vector3.Slerp(forward, -right, angle / 90.0f).normalized * dist);

			DrawCircle(position + forward, direction, color, (forward - (slerpedVector.normalized * dist)).magnitude);
			DrawCircle(position + (forward * 0.5f), direction, color, ((forward * 0.5f) - (slerpedVector.normalized * (dist * 0.5f))).magnitude);

			if (color != default(Color)) Gizmos.color = oldColor;
		}

		/// <summary>- Draws an arrow.</summary>
		/// <param name='position'>- The start position of the arrow.</param>
		/// <param name='direction'>- The direction the arrow will point in.</param>
		/// <param name='color'>- The color of the arrow.</param>
		public static void DrawArrow(Vector3 position, Vector3 direction = default(Vector3), Color color = default(Color), float angle = 15f)
		{
			Color oldColor = Gizmos.color;
			if (Mathf.Approximately(direction.x, 0f) && Mathf.Approximately(direction.y, 0f) && Mathf.Approximately(direction.z, 0f))
				direction = Vector3.forward;
			if (color != default(Color)) Gizmos.color = color;
			Gizmos.DrawRay(position, direction);
			DrawCone(position + direction, (-direction / angle), color, angle);
			if (color != default(Color)) Gizmos.color = oldColor;
		}

		/// <summary>- Draws a capsule.</summary>
		/// <param name='point1'>- The position of one end of the capsule.</param>
		/// <param name='point2'>- The position of the other end of the capsule.</param>
		/// <param name='color'>- The color of the capsule.</param>
		/// <param name='radius'>- The radius of the capsule.</param>
		public static void DrawCapsule(Vector3 point1, Vector3 point2, float radius = 1f, Color color = default(Color))
		{
			Color oldColor = Gizmos.color;
			if (color != default(Color)) Gizmos.color = color;
			if (point1 == point2)
			{
				Gizmos.DrawWireSphere(point1, radius);
			}
			else
			{
				float
					height = (point1 - point2).magnitude,
					sideLength = Mathf.Max(0, (height * 0.5f));

				Vector3
					up = (point2 - point1).normalized * radius,
					forward = Vector3.Slerp(up, -up, 0.5f),
					right = Vector3.Cross(up, forward).normalized * radius,
					middle = (point2 + point1) * 0.5f;

				point1 = middle + ((point1 - middle).normalized * sideLength);
				point2 = middle + ((point2 - middle).normalized * sideLength);

				//Radial circles
				DrawCircle(point1, up, color, radius);
				DrawCircle(point2, -up, color, radius);

				//Side lines
				Gizmos.DrawLine(point1 + right, point2 + right);
				Gizmos.DrawLine(point1 - right, point2 - right);

				Gizmos.DrawLine(point1 + forward, point2 + forward);
				Gizmos.DrawLine(point1 - forward, point2 - forward);

				for (int i = 1; i < 26; i++)
				{
					//Start endcap
					Gizmos.DrawLine(Vector3.Slerp(right, -up, i / 25.0f) + point1, Vector3.Slerp(right, -up, (i - 1) / 25.0f) + point1);
					Gizmos.DrawLine(Vector3.Slerp(-right, -up, i / 25.0f) + point1, Vector3.Slerp(-right, -up, (i - 1) / 25.0f) + point1);
					Gizmos.DrawLine(Vector3.Slerp(forward, -up, i / 25.0f) + point1, Vector3.Slerp(forward, -up, (i - 1) / 25.0f) + point1);
					Gizmos.DrawLine(Vector3.Slerp(-forward, -up, i / 25.0f) + point1, Vector3.Slerp(-forward, -up, (i - 1) / 25.0f) + point1);

					//End endcap
					Gizmos.DrawLine(Vector3.Slerp(right, up, i / 25.0f) + point2, Vector3.Slerp(right, up, (i - 1) / 25.0f) + point2);
					Gizmos.DrawLine(Vector3.Slerp(-right, up, i / 25.0f) + point2, Vector3.Slerp(-right, up, (i - 1) / 25.0f) + point2);
					Gizmos.DrawLine(Vector3.Slerp(forward, up, i / 25.0f) + point2, Vector3.Slerp(forward, up, (i - 1) / 25.0f) + point2);
					Gizmos.DrawLine(Vector3.Slerp(-forward, up, i / 25.0f) + point2, Vector3.Slerp(-forward, up, (i - 1) / 25.0f) + point2);
				}
			}
			if (color != default(Color)) Gizmos.color = oldColor;
		}
		/// <summary>Draw Camera based on give reference.</summary>
		/// <param name="camera"></param>
		/// <param name="color"></param>
		public static void DrawFrustum(Camera camera, Color color = default(Color))
		{
			Color oldColor = Gizmos.color;
			if (color != default(Color)) Gizmos.color = color;

			Gizmos.matrix = Matrix4x4.TRS(camera.transform.position, camera.transform.rotation, Vector3.one);
			Gizmos.DrawFrustum(Vector3.zero, camera.fieldOfView, camera.farClipPlane, camera.nearClipPlane, camera.aspect);
			Gizmos.matrix = Matrix4x4.identity;

			if (color != default(Color)) Gizmos.color = oldColor;
		}
		/// <summary>Draw Plane, based on giving points start to end (forward)</summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="upward"></param>
		/// <param name="height"></param>
		/// <param name="color"></param>
		/// <remarks>pivot point is start point</remarks>
		public static void DrawPlane(Vector3 start, Vector3 end, Vector3 upward, float height = 1f, Color color = default(Color))
		{
			float width = Vector3.Distance(start, end);
			if (Mathf.Approximately(width, 0f))
				return;

			Color oldColor = Gizmos.color;
			if (color != default(Color)) Gizmos.color = color;

			Quaternion rotation =
				Quaternion.LookRotation(end - start, upward) *
				Quaternion.Euler(0f, -90f, 0f);
			Gizmos.matrix = Matrix4x4.TRS(start, rotation, Vector3.one);
			Gizmos.DrawCube(
				new Vector3(width / 2f, height / 2f, 0f),
				new Vector3(width, height, float.Epsilon));
			Gizmos.matrix = Matrix4x4.identity;

			if (color != default(Color)) Gizmos.color = oldColor;
		}
		/// <summary>Draw Plane, based on transform's forward</summary>
		/// <param name="self"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="color"></param>
		public static void DrawPlane(Transform self, float width, float height = 1f, Color color = default(Color))
		{
			DrawPlane(self.position, self.position + (self.forward * width), self.up, height, color);
		}

		public static void DrawSphere(Transform self, Color color = default(Color))
		{
			DrawSphere(self.position, self.localScale.x, color);
		}

		/// <summary>Draw Sphere</summary>
		/// <param name="position"></param>
		/// <param name="radius"></param>
		/// <param name="color"></param>
		public static void DrawSphere(Vector3 position, float radius, Color color = default(Color))
		{
			Color oldColor = Gizmos.color;
			if (color != default(Color)) Gizmos.color = color;
			Gizmos.DrawSphere(position, radius);
			if (color != default(Color)) Gizmos.color = oldColor;
		}

		public static void DrawDirection(Transform self, Color color = default(Color))
		{
			DrawDirection(self.position, Vector3.forward, self.localScale.x, color);
		}

		/// <summary>Draw Direction</summary>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		/// <param name="distance"></param>
		/// <param name="color"></param>
		public static void DrawDirection(Vector3 position, Vector3 direction, float distance = 1f, Color color = default(Color))
		{
			Color oldColor = Gizmos.color;
			if (color != default(Color)) Gizmos.color = color;
			Gizmos.DrawLine(position, position + (direction * distance));
			if (color != default(Color)) Gizmos.color = oldColor;
		}
		#endregion

		#region Handles
		/// <summary>Draw label on current SceneView</summary>
		/// <param name="position"></param>
		/// <param name="text"></param>
		/// <param name="style"></param>
		/// <param name="color"></param>
		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void DrawLabel(Vector3 position, string text, GUIStyle style = default(GUIStyle), Color color = default(Color), float offsetX = 0f, float offsetY = 0f)
		{
#if UNITY_EDITOR
			if (UnityEditor.SceneView.currentDrawingSceneView != null ||
				(Application.isPlaying && Camera.main != null))
			{
				Transform cam = UnityEditor.SceneView.currentDrawingSceneView != null ?
					UnityEditor.SceneView.currentDrawingSceneView.camera.transform : // Scene View
					Camera.main.transform; // Only Game View
				if (Vector3.Dot(cam.forward, position - cam.position) > 0)
				{
					Vector3 pos = position;
					if (offsetX != 0f || offsetY != 0f)
					{
						Vector3 camRightVector = cam.right * offsetX; // base on view
						pos += camRightVector + new Vector3(0f, offsetY, 0f); // base on target
					}

					if (style == default(GUIStyle))
					{
						if (color == default(Color))
							UnityEditor.Handles.Label(pos, text, GUI.skin.textArea);
						else
						{
							style = new GUIStyle(GUI.skin.textArea);
							style.normal.textColor = color;
							UnityEditor.Handles.Label(pos, text, style);
						}
					}
					else
					{
						if (color == default(Color))
							UnityEditor.Handles.Label(pos, text, style);
						else
						{
							Color old = style.normal.textColor;
							style.normal.textColor = color;
							UnityEditor.Handles.Label(pos, text, style);
							style.normal.textColor = old;
						}
					}
				}
			}
#endif
		}
		#endregion

		//Draws just the box at where it is currently hitting.
		public static void DrawBoxCastOnHit(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float hitInfoDistance, Color color = default(Color))
		{
			origin = CastCenterOnCollision(origin, direction, hitInfoDistance);
			DrawBox(origin, halfExtents, orientation, color);
		}

		//Draws the full box from start of cast to its end distance. Can also pass in hitInfoDistance instead of full distance
		public static void DrawBoxCastBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float distance, Color color = default(Color))
		{
			direction.Normalize();
			Box bottomBox = new Box(origin, halfExtents, orientation);
			Box topBox = new Box(origin + (direction * distance), halfExtents, orientation);

			Color oldColor = Gizmos.color;
			if (color != default(Color)) Gizmos.color = color;
			Gizmos.DrawLine(bottomBox.backBottomLeft, topBox.backBottomLeft);
			Gizmos.DrawLine(bottomBox.backBottomRight, topBox.backBottomRight);
			Gizmos.DrawLine(bottomBox.backTopLeft, topBox.backTopLeft);
			Gizmos.DrawLine(bottomBox.backTopRight, topBox.backTopRight);
			Gizmos.DrawLine(bottomBox.frontTopLeft, topBox.frontTopLeft);
			Gizmos.DrawLine(bottomBox.frontTopRight, topBox.frontTopRight);
			Gizmos.DrawLine(bottomBox.frontBottomLeft, topBox.frontBottomLeft);
			Gizmos.DrawLine(bottomBox.frontBottomRight, topBox.frontBottomRight);
			if (color != default(Color)) Gizmos.color = oldColor;

			DrawBox(bottomBox, color);
			DrawBox(topBox, color);
		}

		public static void DrawBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Color color)
		{
			DrawBox(new Box(origin, halfExtents, orientation), color);
		}
		public static void DrawBox(Box box, Color color)
		{
			Color oldColor = Gizmos.color;
			if (color != default(Color)) Gizmos.color = color;
			Gizmos.DrawLine(box.frontTopLeft, box.frontTopRight);
			Gizmos.DrawLine(box.frontTopRight, box.frontBottomRight);
			Gizmos.DrawLine(box.frontBottomRight, box.frontBottomLeft);
			Gizmos.DrawLine(box.frontBottomLeft, box.frontTopLeft);

			Gizmos.DrawLine(box.backTopLeft, box.backTopRight);
			Gizmos.DrawLine(box.backTopRight, box.backBottomRight);
			Gizmos.DrawLine(box.backBottomRight, box.backBottomLeft);
			Gizmos.DrawLine(box.backBottomLeft, box.backTopLeft);

			Gizmos.DrawLine(box.frontTopLeft, box.backTopLeft);
			Gizmos.DrawLine(box.frontTopRight, box.backTopRight);
			Gizmos.DrawLine(box.frontBottomRight, box.backBottomRight);
			Gizmos.DrawLine(box.frontBottomLeft, box.backBottomLeft);
			if (color != default(Color)) Gizmos.color = oldColor;
		}

		public struct Box
		{
			public Vector3 localFrontTopLeft { get; private set; }
			public Vector3 localFrontTopRight { get; private set; }
			public Vector3 localFrontBottomLeft { get; private set; }
			public Vector3 localFrontBottomRight { get; private set; }
			public Vector3 localBackTopLeft { get { return -localFrontBottomRight; } }
			public Vector3 localBackTopRight { get { return -localFrontBottomLeft; } }
			public Vector3 localBackBottomLeft { get { return -localFrontTopRight; } }
			public Vector3 localBackBottomRight { get { return -localFrontTopLeft; } }

			public Vector3 frontTopLeft { get { return localFrontTopLeft + origin; } }
			public Vector3 frontTopRight { get { return localFrontTopRight + origin; } }
			public Vector3 frontBottomLeft { get { return localFrontBottomLeft + origin; } }
			public Vector3 frontBottomRight { get { return localFrontBottomRight + origin; } }
			public Vector3 backTopLeft { get { return localBackTopLeft + origin; } }
			public Vector3 backTopRight { get { return localBackTopRight + origin; } }
			public Vector3 backBottomLeft { get { return localBackBottomLeft + origin; } }
			public Vector3 backBottomRight { get { return localBackBottomRight + origin; } }

			public Vector3 origin { get; private set; }

			public Box(Vector3 origin, Vector3 halfExtents, Quaternion orientation) : this(origin, halfExtents)
			{
				Rotate(orientation);
			}

			public Box(Vector3 origin, Vector3 halfExtents) : this()
			{
				this.localFrontTopLeft = new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
				this.localFrontTopRight = new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
				this.localFrontBottomLeft = new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
				this.localFrontBottomRight = new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);

				this.origin = origin;
			}

			public void Rotate(Quaternion orientation)
			{
				localFrontTopLeft = RotatePointAroundPivot(localFrontTopLeft, Vector3.zero, orientation);
				localFrontTopRight = RotatePointAroundPivot(localFrontTopRight, Vector3.zero, orientation);
				localFrontBottomLeft = RotatePointAroundPivot(localFrontBottomLeft, Vector3.zero, orientation);
				localFrontBottomRight = RotatePointAroundPivot(localFrontBottomRight, Vector3.zero, orientation);
			}
		}

		//This should work for all cast types
		static Vector3 CastCenterOnCollision(Vector3 origin, Vector3 direction, float hitInfoDistance)
		{
			return origin + (direction.normalized * hitInfoDistance);
		}

		static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
		{
			Vector3 direction = point - pivot;
			return pivot + rotation * direction;
		}
	}
}
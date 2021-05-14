using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoundsExtend
{
	public static Bounds GetForward(this Bounds self)
	{
		return new Bounds(new Vector3(
			self.center.x,
			self.center.y,
			self.center.z + self.size.z),
			self.size);
	}

	public static Bounds GetBackward(this Bounds self)
	{
		return new Bounds(new Vector3(
			self.center.x,
			self.center.y,
			self.center.z - self.size.z),
			self.size);
	}

	public static Bounds GetUp(this Bounds self)
	{
		return new Bounds(new Vector3(
			self.center.x,
			self.center.y + self.size.y,
			self.center.z),
			self.size);
	}

	public static Bounds GetDown(this Bounds self)
	{
		return new Bounds(new Vector3(
			self.center.x,
			self.center.y - self.size.y,
			self.center.z),
			self.size);
	}

	public static Bounds GetLeft(this Bounds self)
	{
		return new Bounds(new Vector3(
			self.center.x - self.size.x,
			self.center.y,
			self.center.z),
			self.size);
	}

	public static Bounds GetRight(this Bounds self)
	{
		return new Bounds(new Vector3(
			self.center.x + self.size.x,
			self.center.y,
			self.center.z),
			self.size);
	}

	/// <summary>
	/// Checks if outerBounds encapsulates innerBounds.
	/// </summary>
	/// <param name="outerBounds">Outer bounds.</param>
	/// <param name="innerBounds">Inner bounds.</param>
	/// <returns>True if innerBounds is fully encapsulated by outerBounds.</returns>
	public static bool IsFullyEncapsulate(this Bounds outerBounds, Bounds innerBounds)
	{
		return outerBounds.Contains(innerBounds.min) && outerBounds.Contains(innerBounds.max);
	}

	public static Bounds GetStructuredBounds(Vector3 point, float size)
	{
		return GetStructuredBounds(point, new Vector3(size, size, size));
	}

	public static Bounds GetStructuredBounds(Vector3 point, Vector3 size)
	{
		point = new Vector3(
			Mathf.Sign(point.x) * (Mathf.Abs(point.x) + size.x * 0.5f),
			Mathf.Sign(point.y) * (Mathf.Abs(point.y) + size.y * 0.5f),
			Mathf.Sign(point.z) * (Mathf.Abs(point.z) + size.z * 0.5f));
		Vector3 remainder = new Vector3(
			point.x % size.x,
			point.y % size.y,
			point.z % size.z);
		Vector3 center = point - remainder;
		return new Bounds(center, size);
	}

	/// <summary>Does another bounding box intersect with this bounding box?</summary>
	/// <param name="self"></param>
	/// <param name="other"></param>
	/// <param name="containSurfaceCollision">false = to ignore the surface collision (Which are included in Unity default API).</param>
	/// <returns>ture = intersect each other</returns>
	/// <remarks>
	/// to ignore surface collision, aim for finding neighbors within a grid of bounds,
	/// A quick AABB test to sort out which one are correct neighbors.</remarks>
	public static bool Intersects(this Bounds self, Bounds other, bool containSurfaceCollision = true)
	{
		if (containSurfaceCollision)
		{
			// Unity API.
			return self.Intersects(other);
		}
		else
		{
			Vector3 c = self.center - other.center;
			Vector3 r = self.extents + other.extents;
			// math hack: to ignore surface collision by using "<" instead of "<="
			return
				Mathf.Abs(c.x) < r.x &&
				Mathf.Abs(c.y) < r.y &&
				Mathf.Abs(c.z) < r.z;
		}
	}
}

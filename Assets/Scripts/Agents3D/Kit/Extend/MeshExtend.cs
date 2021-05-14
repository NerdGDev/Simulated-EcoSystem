using UnityEngine;
using System.Collections;

namespace Kit
{
	public static class MeshExtend
	{
		public static Mesh CreateFlat(string _name, float _width, float _height)
		{
			Mesh _mesh = new Mesh ();
			_mesh.name = _name;
			_mesh.vertices = new Vector3[4]{
				new Vector3(-_width, -_height, 0.01f),
				new Vector3(_width, -_height, 0.01f),
				new Vector3(_width, _height, 0.01f),
				new Vector3(-_width, _height, 0.01f)
			};
			_mesh.uv = new Vector2[4] {
				new Vector2 (0, 0),
				new Vector2 (0, 1),
				new Vector2 (1, 1),
				new Vector2 (1, 0)
			};
			_mesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3};
			_mesh.RecalculateNormals ();
			return _mesh;
		}
		
		public static Mesh Clone(this Mesh mesh)
        {
            var copy = new Mesh();
            foreach(var property in typeof(Mesh).GetProperties())
            {
                if(property.GetSetMethod() != null && property.GetGetMethod() != null)
                {
                    property.SetValue(copy, property.GetValue(mesh, null), null);
                }
            }
            return copy;
        }


		/// <summary>Find out the point nearests, based on the vertex.
		/// <see cref="http://answers.unity3d.com/questions/7788/closest-point-on-mesh-collider.html"/> 
		/// </summary>
		/// <returns>The cloeset vertex to point.</returns>
		/// <param name="point">Point.</param>
		public static Vector3 ClosestVertexOnMeshTo(this MeshFilter meshFilter, Vector3 point)
		{
			// convert point to local space
			point = meshFilter.transform.InverseTransformPoint(point);
			float minDistanceSqr = Mathf.Infinity;
			Vector3 nearestVertex = Vector3.zero;
			// scan all vertices to find nearest
			foreach (Vector3 vertex in meshFilter.mesh.vertices)
			{
				Vector3 diff = point-vertex;
				float distSqr = diff.sqrMagnitude;
				if (distSqr < minDistanceSqr)
				{
					minDistanceSqr = distSqr;
					nearestVertex = vertex;
				}
			}
			// convert nearest vertex back to world space
			return meshFilter.transform.TransformPoint(nearestVertex);
		}
	}
}
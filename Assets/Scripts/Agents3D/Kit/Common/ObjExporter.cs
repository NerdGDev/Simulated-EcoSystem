using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

public class ObjExporter
{
	public static string MeshToString(MeshFilter mf)
	{
		Mesh mesh = mf.mesh;
		Renderer renderer = mf.GetComponent<Renderer>();
		Material[] materials = renderer.sharedMaterials;

		StringBuilder sb = new StringBuilder();

		sb.Append("g ").Append(mf.name).Append("\n");
		foreach (Vector3 v in mesh.vertices)
		{
			sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
		}
		sb.Append("\n");
		foreach (Vector3 v in mesh.normals)
		{
			sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
		}
		sb.Append("\n");
		foreach (Vector3 v in mesh.uv)
		{
			sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
		}
		for (int k = 0; k < mesh.subMeshCount; k++)
		{
			sb.Append("\n");
			sb.Append("usemtl ").Append(materials[k].name).Append("\n");
			sb.Append("usemap ").Append(materials[k].name).Append("\n");

			int[] triangles = mesh.GetTriangles(k);
			for (int i = 0; i < triangles.Length; i += 3)
			{
				sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
					triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
			}
		}
		return sb.ToString();
	}

	public static void MeshToFile(MeshFilter mf, string filename)
	{
		using (StreamWriter sw = new StreamWriter(filename))
		{
			sw.Write(MeshToString(mf));
		}
	}
}
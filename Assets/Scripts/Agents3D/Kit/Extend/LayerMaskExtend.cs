using UnityEngine;

namespace Kit
{
	public static class LayerMaskExtend
	{
		public static bool Contain(this LayerMask layerMask, GameObject gameObject)
		{
			return layerMask.Contain(gameObject.layer);
		}

		/// <summary>
		/// Extension method to check if a layer is in a layermask
		/// <see cref="http://answers.unity3d.com/questions/50279/check-if-layer-is-in-layermask.html"/>
		/// </summary>
		/// <param name="layerMask"></param>
		/// <param name="layer"></param>
		/// <returns></returns>
		public static bool Contain(this LayerMask layerMask, int layer)
		{
			return layerMask == (layerMask | (1 << layer));
		}

		/// <summary>A dirty way to convert LayerMask into single layer</summary>
		/// <param name="layerMask"></param>
		/// <returns></returns>
		public static int ConvertToSingleLayer(this LayerMask layerMask)
		{
			return Mathf.CeilToInt(Mathf.Log(layerMask.value, 2));
		}
	}
}
using UnityEngine;

namespace Kit
{
	public static class TextureExtend
	{
		public static void DrawDonut(this Texture2D texture, int radius, int border, Color color)
		{
			texture.DrawDonut(radius,radius,radius,border,color);
		}
		public static void DrawDonut(this Texture2D texture, int centerX, int centerY, int radius, int border, Color color)
		{
			for(int i=0; i<border; i++)
			{
				texture.DrawCircle(centerX,centerY,radius--,color);
			}
		}
		public static void Circle(ref Texture2D texture, int radius, Color col)
		{
            texture.DrawCircle(radius,radius,radius,col);
		}
		public static void DrawCircle(this Texture2D texture, int cx, int cy, int radius, Color col)
		{
			int y = radius;
			int d = 1/4-radius;
			int end = Mathf.CeilToInt(radius/Mathf.Sqrt(2));

			for(int x=0; x<=end; x++)
			{
				texture.SetPixel(cx+x, cy+y, col);
				texture.SetPixel(cx+x, cy-y, col);
				texture.SetPixel(cx-x, cy+y, col);
				texture.SetPixel(cx-x, cy-y, col);
				texture.SetPixel(cx+y, cy+x, col);
				texture.SetPixel(cx-y, cy+x, col);
				texture.SetPixel(cx+y, cy-x, col);
				texture.SetPixel(cx-y, cy-x, col);
				d += 2*x+1;
				if (d > 0)
					d += 2 - 2*y--;
			}
		}
		/// <summary>Fill the specified texture, width, height and color.</summary>
		/// <param name="texture">Texture.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="color">Color.</param>
		/// <remarks>Warning : This will override all color in area.</remarks>
		public static void FillColor(this Texture2D texture, int width, int height, Color color)
		{
			Color32[] color32 = new Color32[width*height];
			for(int i=0; i<color32.Length; i++)
				color32[i]=color;
			texture.SetPixels32(color32);
		}
	}
}
using System;

namespace Xmdsp
{
	public partial class ViewModel
	{
		public struct Color
		{
			int color;
			
			public Color (byte r, byte g, byte b, byte a = 255)
			{
				color = (a << 24) + (r << 16) + (g << 8) + b;
			}
			
			public byte A { get { return (byte) ((color & 0xFF000000) >> 24); } }
			public byte R { get { return (byte) ((color & 0x00FF0000) >> 16); } }
			public byte G { get { return (byte) ((color & 0x0000FF00) >> 8); } }
			public byte B { get { return (byte) ((color & 0x000000FF) >> 0); } }
			
			public static readonly Color Transparent = new Color (0, 0, 0, 0);
			public static readonly Color Black = new Color (0, 0, 0);
			public static readonly Color White = new Color (255, 255, 255);
			
			public Color Darken (double percent)
			{
				return new Color ((byte) (R * percent), (byte) (G * percent), (byte) (B * percent), A);
			}
		}
		
		public struct Rectangle
		{
			public Rectangle (int x, int y, int width, int height)
			{
				X = x;
				Y = y;
				Width = width;
				Height = height;
			}
			
			public readonly int X;
			public readonly int Y;
			public readonly int Width;
			public readonly int Height;
		}
	}
}


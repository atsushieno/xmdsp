using System;
using Xwt;
using Xwt.Drawing;

namespace Xmdsp
{
	public static class DrawingHelper
	{
		// ok, it won't go more than 50. And no need to mess with Dictionary. Array is faster.
		static Font [] fonts_by_size = new Font [50];

		public static Size DrawText (Context ctx, Font font, int size, ViewModel.Color color, string text, double x, double y)
		{
			ctx.SetColor (color.ToXwt ());
			if (fonts_by_size [size] == null)
				fonts_by_size [size] = font.WithSize (size);
			font = fonts_by_size [size];
			var textLayout = new TextLayout () { Font = font, Text = text };
			var numberSize = textLayout.GetSize ();
			ctx.DrawTextLayout (textLayout, x, y);
			ctx.Stroke ();
			return textLayout.GetSize ();
		}
	}
}


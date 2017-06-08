using System;
using SkiaSharp;

namespace Xmdsp
{
	public static class DrawingHelper
	{
		public static void DrawText (SKCanvas ctx, SKPaint paint, int size, Presenter.Color color, string text, double x, double y)
		{
			paint.IsStroke = true;
			paint.Color = color.ToSkia ();
			paint.TextSize = size;
			ctx.DrawText (text, (float) x, (float) y, paint);
		}
	}
}


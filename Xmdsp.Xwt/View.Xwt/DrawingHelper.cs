using System;
using System.Threading;
using Xwt;
using Xwt.Drawing;

namespace Xmdsp
{
	public static class DrawingHelper
	{
		// ok, it won't go more than 50. And no need to mess with Dictionary. Array is faster.
		static Font [] fonts_by_size = new Font [50];

		static bool is_busy;
		static TextLayout tl = new TextLayout ();

		public static Size DrawText (Context ctx, Font font, int size, Presenter.Color color, string text, double x, double y)
		{
			ctx.SetColor (color.ToXwt ());
			if (fonts_by_size [size] == null)
				fonts_by_size [size] = font.WithSize (size);
			font = fonts_by_size [size];
#if false
			// Xwt issue: this should not result in explosive memory use, but
			// it seems that Xwt is failing to release memory at TextLayout.Dispose().
			var textLayout = new TextLayout () { Font = font, Text = text };
			ctx.DrawTextLayout (textLayout, x, y);
			ctx.Stroke ();
			var ret = textLayout.GetSize ();
			textLayout.Dispose ();
			return ret;
#else
			while (is_busy)
				Thread.SpinWait (10);
			is_busy = true;
			tl.Font = font;
			tl.Text = text;
			ctx.DrawTextLayout (tl, x, y);
			var ret = tl.GetSize ();
			is_busy = false;
			return ret;
#endif
		}
	}
}


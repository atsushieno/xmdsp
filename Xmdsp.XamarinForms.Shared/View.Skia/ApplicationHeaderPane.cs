﻿using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace Xmdsp
{
	public class ApplicationHeaderPane : SKCanvasView
	{
		Presenter pm;
		
		public ApplicationHeaderPane (Presenter pm)
		{
			this.pm = pm;
			pm.ScaleChanged += SetSize;
		}

		internal void SetSize ()
		{
			WidthRequest = pm.ApplicationHeaderPane.Width * pm.Scale;
			HeightRequest = pm.ApplicationHeaderPane.Height * pm.Scale;
		}

		protected override void OnPaintSurface (SKPaintSurfaceEventArgs e)
		{
			base.OnPaintSurface (e);
			var ctx = e.Surface.Canvas;
			var dirtyRect = e.Info.Rect;

			ctx.Scale ((float) pm.Scale, (float) pm.Scale);

			var paint = new SKPaint () { StrokeWidth = 1, IsStroke = true, TextSize = 16 };

			paint.Color = pm.Pallette.CommonTextMiddle.ToSkia ();
			ctx.DrawText ("XMDSP", 0, 0, paint);

			// FIXME: measure text size (but how?)
			int leftEnd = 100; // (int) appName.GetSize ().Width;

			ctx.DrawLine (0, 26, leftEnd, 26, paint);

			int rightStart = leftEnd + 5;
			
			var appDesc = "standard MIDI file visualizer and player";
			paint.Color = pm.Pallette.CommonTextDarkest.ToSkia ();
			paint.TextSize = 8;
			ctx.DrawText (appDesc, rightStart, 0, paint);

			var appDetails = "version " + Model.VersionNumbers + " / with Xwt. (C)2013- atsushieno";
			ctx.DrawText (appDetails, rightStart, 11, paint);

			// FIXME: measure text size (but how?)
			int rightEnd = 200; // rightStart + (int) appDetails.GetSize ().Width;
			
			ctx.DrawLine (rightStart, 26, rightEnd, 26, paint);
		}
	}
}


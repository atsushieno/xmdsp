﻿using System;
using Commons.Music.Midi;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace Xmdsp
{
	public class CircularProgressMeter : SKCanvasView
	{
		readonly Presenter pm;
		readonly Presenter.CircularProgressMeterPresenter pmc;
		
		public CircularProgressMeter (Presenter pm)
		{
			this.pm = pm;
			this.pmc = pm.CircularProgressMeter;
			pm.ScaleChanged += SetSize;

			pm.Model.TickProgress += delegate {
				current_position++;
				current_position %= num_lines;
				//QueueDraw ();
			};
		}

		internal void SetSize ()
		{
			WidthRequest = pmc.Width * pm.Scale;
			HeightRequest = pmc.Height * pm.Scale;
		}
		
		int current_position;
		const int num_lines = 18;
		const double delta = 360 / num_lines;
		
		const int start = 4;
		const int end = 18;

		SKPaint paint = new SKPaint ();

		protected override void OnPaintSurface (SKPaintSurfaceEventArgs e)
		{
			base.OnPaintSurface (e);
			var ctx = e.Surface.Canvas;
			var dirtyRect = e.Info.Rect;

			ctx.Scale ((float)pm.Scale, (float)pm.Scale);

			paint.IsStroke = true;
			paint.StrokeWidth = 1;
			paint.Color = (pm.Pallette.CommonTextDarkest.ToSkia ());
			ctx.Translate (pmc.Padding, pmc.Padding);
			for (int i = 0; i < num_lines; i++)
				ctx.DrawLine ((int) (Math.Sin (2 * Math.PI / num_lines * i) * start),
				              (int) (Math.Cos (2 * Math.PI / num_lines * i) * start),
				              (int) (Math.Sin (2 * Math.PI / num_lines * i) * end),
				              (int) (Math.Cos (2 * Math.PI / num_lines * i) * end),
				              paint);
			
			if (pm.Model.Player != null) {
				switch (pm.Model.Player.State) {
				case PlayerState.Playing:
				case PlayerState.FastForward:
					paint.Color = (pm.Pallette.CommonTextMiddle.ToSkia ());
					var p = -current_position;
					ctx.DrawLine ((int) (Math.Sin (2 * Math.PI / num_lines * p) * start),
					              (int) (Math.Cos (2 * Math.PI / num_lines * p) * start),
					              (int) (Math.Sin (2 * Math.PI / num_lines * p) * end),
					              (int) (Math.Cos (2 * Math.PI / num_lines * p) * end),
					              paint);
					break;
				}
			}
			
			ctx.Translate (-pmc.Padding, -pmc.Padding);
		}
	}
}


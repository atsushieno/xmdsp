﻿using System;
using Commons.Music.Midi;
using Xwt;

namespace Xmdsp
{
	public class CircularProgressMeter : Canvas
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
				Application.Invoke (QueueDraw);
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
		
		protected override void OnDraw (Xwt.Drawing.Context ctx, Rectangle dirtyRect)
		{
			ctx.Scale (pm.Scale, pm.Scale);
			ctx.SetLineWidth (1);
			ctx.SetColor (pm.Pallette.CommonTextDarkest.ToXwt ());
			ctx.Translate (pmc.Padding, pmc.Padding);
			for (int i = 0; i < num_lines; i++) {
				ctx.MoveTo ((int) (Math.Sin (2 * Math.PI / num_lines * i) * start), (int) (Math.Cos (2 * Math.PI / num_lines * i) * start));
				ctx.LineTo ((int) (Math.Sin (2 * Math.PI / num_lines * i) * end), (int) (Math.Cos (2 * Math.PI / num_lines * i) * end));
			}
			
			ctx.Stroke ();
			
			if (pm.Model.Player != null) {
				switch (pm.Model.Player.State) {
				case PlayerState.Playing:
					ctx.SetColor (pm.Pallette.CommonTextMiddle.ToXwt ());
					var p = -current_position;
					ctx.MoveTo ((int) (Math.Sin (2 * Math.PI / num_lines * p) * start), (int) (Math.Cos (2 * Math.PI / num_lines * p) * start));
					ctx.LineTo ((int) (Math.Sin (2 * Math.PI / num_lines * p) * end), (int) (Math.Cos (2 * Math.PI / num_lines * p) * end));
					ctx.Stroke ();
					break;
				}
			}
			
			ctx.Translate (-pmc.Padding, -pmc.Padding);
		}
	}
}


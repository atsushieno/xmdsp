using System;
using Xwt;
using Commons.Music.Midi.Player;

namespace Xmdsp
{
	public class CircularProgressMeter : Canvas
	{
		readonly ViewModel vm;
		
		public CircularProgressMeter (ViewModel vm)
		{
			this.vm = vm;
			
			WidthRequest = 60;
			HeightRequest = 60;
			
			// FIXME: this control causes native crash on gtk...
			/*
			vm.Model.PlayTimerTick += delegate {
				var ts = DateTime.Now - last_updated;
				var delta = 2000.0 / num_lines / vm.Model.Player.Bpm * 120;
				if (delta > 0 && ts.TotalMilliseconds > delta) {
					if (last_updated != DateTime.MinValue) {
						current_position += (int) (ts.TotalMilliseconds / delta);
						current_position %= num_lines;
					}
					last_updated += ts;
					Console.WriteLine ("{0}, {1}", current_position, last_updated.Ticks);
					QueueDraw ();
				}
			};
			*/
		}
		
		DateTime last_updated = DateTime.MinValue;
		int current_position;
		const int num_lines = 18;
		const double delta = 360 / num_lines;
		
		protected override void OnDraw (Xwt.Drawing.Context ctx, Rectangle dirtyRect)
		{
			if (dirtyRect.Width < 0 || dirtyRect.Height < 0)
				throw new Exception (string.Format ("Invalid dirtyRect: {0} {1}", dirtyRect.Width, dirtyRect.Height));
			base.OnDraw (ctx, dirtyRect);
			
			int start = 4;
			int end = 18;
			
			ctx.SetLineWidth (1);
			ctx.SetColor (vm.Pallette.CommonTextDarkest.ToXwt ());
			ctx.Translate (32, 32);
			for (int i = 0; i < num_lines; i++) {
				ctx.MoveTo ((int) (Math.Sin (2 * Math.PI / num_lines * i) * start), (int) (Math.Cos (2 * Math.PI / num_lines * i) * start));
				ctx.LineTo ((int) (Math.Sin (2 * Math.PI / num_lines * i) * end), (int) (Math.Cos (2 * Math.PI / num_lines * i) * end));
			}
			
			ctx.Stroke ();
			
			if (vm.Model.Player != null) {
				switch (vm.Model.Player.State) {
				case PlayerState.Playing:
				case PlayerState.FastForward:
					ctx.SetColor (vm.Pallette.CommonTextMiddle.ToXwt ());
					var p = -current_position;
					ctx.MoveTo ((int) (Math.Sin (2 * Math.PI / num_lines * p) * start), (int) (Math.Cos (2 * Math.PI / num_lines * p) * start));
					ctx.LineTo ((int) (Math.Sin (2 * Math.PI / num_lines * p) * end), (int) (Math.Cos (2 * Math.PI / num_lines * p) * end));
					ctx.Stroke ();
					break;
				}
			}
			
			ctx.Translate (-32, -32);
		}
	}
}


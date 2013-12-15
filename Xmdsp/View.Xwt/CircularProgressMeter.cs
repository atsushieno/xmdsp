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
			
			vm.Model.TickProgress += delegate {
				current_position++;
				current_position %= num_lines;
				// FIXME: this cannot be enabled until the cause of native crasher gets resolved.
				//QueueDraw ();
			};
		}
		
		int current_position;
		const int num_lines = 18;
		const double delta = 360 / num_lines;
		
		const int start = 4;
		const int end = 18;
		
		protected override void OnDraw (Xwt.Drawing.Context ctx, Rectangle dirtyRect)
		{
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


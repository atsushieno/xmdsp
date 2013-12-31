using System;
using Gtk;
using Commons.Music.Midi.Player;
using Gdk;

namespace Xmdsp
{
	public class CircularProgressMeter : DrawingArea
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
				QueueDraw ();
			};
		}
		
		int current_position;
		const int num_lines = 18;
		const double delta = 360 / num_lines;
		
		const int start = 4;
		const int end = 18;
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			var ctx = Gdk.CairoHelper.Create (GdkWindow);
			ctx.LineWidth = 1;
			CairoHelper.SetSourceColor (ctx, vm.Pallette.CommonTextDarkest.ToGdk ());
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
					CairoHelper.SetSourceColor (ctx, vm.Pallette.CommonTextMiddle.ToGdk ());
					var p = -current_position;
					ctx.MoveTo ((int) (Math.Sin (2 * Math.PI / num_lines * p) * start), (int) (Math.Cos (2 * Math.PI / num_lines * p) * start));
					ctx.LineTo ((int) (Math.Sin (2 * Math.PI / num_lines * p) * end), (int) (Math.Cos (2 * Math.PI / num_lines * p) * end));
					ctx.Stroke ();
					break;
				}
			}
			
			ctx.Translate (-32, -32);
			
			ctx.Dispose ();
			return true;
		}
	}
}


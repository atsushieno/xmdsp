using System;
using Gtk;
using Gdk;
using Commons.Music.Midi.Player;

using FontFace = Pango.FontFace;

namespace Xmdsp
{
	public class PlayTimeStatusMonitor : DrawingArea
	{
		ViewModel vm;
		
		public PlayTimeStatusMonitor (ViewModel vm, FontFace font, FontFace monospaceFont)
		{
			this.vm = vm;
			
			WidthRequest = vm.PlayTimeStatusMonitor.Width;
			HeightRequest = vm.PlayTimeStatusMonitor.Height;
			
			vm.Model.PlayTimerTick += delegate {
				QueueDraw ();
			};
			vm.Model.PlayerStateChanged += delegate {
				QueueDraw ();
			};
			this.font = font;
			this.monospace_font = monospaceFont;
		}
		
		FontFace font, monospace_font;
		WeakReference cached_player;
		string total_time_string;
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			var ctx = CairoHelper.Create (GdkWindow);

			CairoHelper.SetSourceColor (ctx, vm.Pallette.CommonTextDarkest.ToGdk ());
			ctx.LineWidth = 1;
			for (int i = 0; i < 5; i++) {
				ctx.MoveTo (0, 30 * i + 24 + 1);
				ctx.LineTo (180, 30 * i + 24 + 1);
				ctx.Stroke ();
			}
			
			CairoHelper.SetSourceColor (ctx, vm.Pallette.CommonTextMiddle.ToGdk ());
			for (int i = 0; i < 5; i++) {
				ctx.Rectangle (0, 30 * i, 5, 24);
				ctx.Fill ();
			}
			
			ctx.Translate (10, 0);

			//ctx.SetContextFontFace (font);
			ctx.SetFontSize (10);
			ctx.Translate (0, 10);
			DrawTextLayout (ctx, "Passed", new Point (0, 0));
			DrawTextLayout (ctx, "Time", new Point (10, 12));
			DrawTextLayout (ctx, "Total", new Point (0, 30));
			DrawTextLayout (ctx, "Time", new Point (10, 42));
			DrawTextLayout (ctx, "Tick", new Point (0, 60));
			DrawTextLayout (ctx, "Count", new Point (10, 72));
			DrawTextLayout (ctx, "Tempo", new Point (0, 90));
			DrawTextLayout (ctx, "Meter", new Point (0, 120));
			ctx.Translate (0, -10);
			
			ctx.Translate (60, 0);
			
			string playTime, totalTime, ticks, tempo, meter;
			
			if (vm.Model.Player != null) {
				if (cached_player == null || !cached_player.IsAlive) {
					cached_player = new WeakReference (vm.Model.Player);
					total_time_string = TimeSpan.FromMilliseconds (((MidiPlayer) cached_player.Target).GetTotalPlayTimeMilliseconds ()).ToString ("mm\\:ss");
				}
				totalTime = "   " + total_time_string;
				playTime = "   " + vm.Model.Player.PositionInTime.ToString ("mm\\:ss");
				ticks = vm.Model.Player.PlayDeltaTime.ToString ("D08");
				tempo = "     " + vm.Model.Player.Bpm.ToString ("D03");
				var data = vm.Model.Player.TimeSignature;
				meter = "     " + data [0] + "/" + Math.Pow (2, data [1]);
			} else {
				playTime = "   --:--";
				totalTime = "   --:--";
				ticks = "--------";
				tempo = "     ---";
				meter = "     -/-";
			}
				
			//ctx.SetContextFontFace (monospace_font);
			ctx.SetFontSize (16);
			ctx.Translate (0, 16);
			DrawTextLayout (ctx, playTime, new Point (0, 0));
			DrawTextLayout (ctx, totalTime, new Point (0, 30));
			DrawTextLayout (ctx, ticks, new Point (0, 60));
			DrawTextLayout (ctx, tempo, new Point (0, 90));
			DrawTextLayout (ctx, meter, new Point (0, 120));
			ctx.Translate (0, -16);
			
			ctx.Translate (-70, 0);
			
			ctx.Stroke ();
			
			ctx.Dispose ();
			return true;
		}
		
		void DrawTextLayout (Cairo.Context ctx, string text, Point point)
		{
			ctx.MoveTo (point.X, point.Y);
			ctx.TextPath (text);
		}
	}
}

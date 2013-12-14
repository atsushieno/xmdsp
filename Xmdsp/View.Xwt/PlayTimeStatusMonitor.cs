using System;
using Xwt;
using Xwt.Drawing;
using Commons.Music.Midi.Player;

namespace Xmdsp
{
	public class PlayTimeStatusMonitor : Canvas
	{
		ViewModel vm;
		
		public PlayTimeStatusMonitor (ViewModel vm)
		{
			this.vm = vm;
			
			WidthRequest = vm.PlayTimeStatusMonitor.Width;
			HeightRequest = vm.PlayTimeStatusMonitor.Height;
			
			vm.Model.PlayTimerTick += delegate {
				QueueDraw ();
			};
		}
		
		Font font_label;
		Font font_value;
		WeakReference cached_player;
		string total_time_string;
		
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);

			ctx.SetColor (vm.Pallette.CommonTextDarkest.ToXwt ());
			ctx.SetLineWidth (1);
			for (int i = 0; i < 5; i++) {
				ctx.MoveTo (0, 30 * i + 24 + 1);
				ctx.LineTo (180, 30 * i + 24 + 1);
				ctx.Stroke ();
			}
			
			ctx.SetColor (vm.Pallette.CommonTextMiddle.ToXwt ());
			for (int i = 0; i < 5; i++) {
				ctx.Rectangle (0, 30 * i, 5, 24);
				ctx.Fill ();
			}
			
			ctx.Translate (10, 0);
			
			font_label = font_label ?? this.Font.WithSize (10);
			var font = font_label;
			
			ctx.DrawTextLayout (new TextLayout () { Font = font, Text = "Passed" }, new Point (0, 0));
			ctx.DrawTextLayout (new TextLayout () { Font = font, Text = "Time" }, new Point (10, 12));
			ctx.DrawTextLayout (new TextLayout () { Font = font, Text = "Total" }, new Point (0, 30));
			ctx.DrawTextLayout (new TextLayout () { Font = font, Text = "Time" }, new Point (10, 42));
			ctx.DrawTextLayout (new TextLayout () { Font = font, Text = "Tick" }, new Point (0, 60));
			ctx.DrawTextLayout (new TextLayout () { Font = font, Text = "Count" }, new Point (10, 72));
			ctx.DrawTextLayout (new TextLayout () { Font = font, Text = "Tempo" }, new Point (0, 90));
			ctx.DrawTextLayout (new TextLayout () { Font = font, Text = "Meter" }, new Point (0, 120));
			
			ctx.Translate (60, 0);
			font_value = font_value ?? Font.SystemMonospaceFont.WithSize (16);
			font = font_value;
			
			string playTime, totalTime, ticks, tempo, meter;
			
			if (vm.Model.Player != null) {
				if (cached_player == null || !cached_player.IsAlive) {
					cached_player = new WeakReference (vm.Model.Player);
					total_time_string = TimeSpan.FromMilliseconds (((MidiPlayer) cached_player.Target).GetTotalPlayTimeMilliseconds ()).ToString ("mm\\:ss");
				}
				totalTime = "   " + total_time_string;
				playTime = "   " + vm.Model.PositionInTime.ToString ("mm\\:ss");
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
				
			ctx.DrawTextLayout (new TextLayout () { Font = font, Text = playTime }, new Point (0, 0));
			ctx.DrawTextLayout (new TextLayout () { Font = font, Text = totalTime }, new Point (0, 30));
			ctx.DrawTextLayout (new TextLayout () { Font = font, Text = ticks }, new Point (0, 60));
			ctx.DrawTextLayout (new TextLayout () { Font = font, Text = tempo }, new Point (0, 90));
			ctx.DrawTextLayout (new TextLayout () { Font = font, Text = meter }, new Point (0, 120));
			
			ctx.Translate (-70, 0);
			
			ctx.Stroke ();
		}
	}
}


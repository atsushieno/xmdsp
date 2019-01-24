using System;
using Commons.Music.Midi;
using Xwt;
using Xwt.Drawing;

namespace Xmdsp
{
	public class PlayTimeStatusMonitor : Canvas
	{
		Presenter pm;
		
		public PlayTimeStatusMonitor (Presenter pm)
		{
			this.pm = pm;

			pm.ScaleChanged += SetSize;

			pm.Model.PlayTimerTick += delegate { QueueDraw (); };
			pm.Model.PlayerStateChanged += delegate { QueueDraw (); };
			pm.MidiMessageReceived += e =>
			{
				if (e.StatusByte != MidiEvent.Meta)
					return;
				if (e.MetaType == MidiMetaType.TimeSignature) {
					var data = pm.Model.Player.TimeSignature;
					var timeSigNoSpace = data [0] + "/" + Math.Pow (2, data [1]);
					meter_string = new string (' ', 8 - timeSigNoSpace.Length) + timeSigNoSpace;
				}
			};			
			Action smfLoaded = () => total_time_string = TimeSpan.FromMilliseconds (pm.Model.Music.GetTotalPlayTimeMilliseconds ()).ToString ("mm\\:ss"); 
			pm.Model.SmfLoaded += smfLoaded;
			if (pm.Model.Music != null)
				smfLoaded ();
		}

		internal void SetSize ()
		{
			WidthRequest = pm.PlayTimeStatusMonitor.Width * pm.Scale;
			HeightRequest = pm.PlayTimeStatusMonitor.Height * pm.Scale;
		}
		
		Font font_label;
		Font font_value;
		string total_time_string, meter_string;
		
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);
			ctx.Scale (pm.Scale, pm.Scale);

			ctx.SetColor (pm.Pallette.CommonTextDarkest.ToXwt ());
			ctx.SetLineWidth (1);
			for (int i = 0; i < 5; i++) {
				ctx.MoveTo (0, 30 * i + 24 + 1);
				ctx.LineTo (180, 30 * i + 24 + 1);
				ctx.Stroke ();
			}
			
			ctx.SetColor (pm.Pallette.CommonTextMiddle.ToXwt ());
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
			
			if (pm.Model.Player != null) {
				totalTime = "   " + total_time_string;
				playTime = "   " + pm.Model.Player.PositionInTime.ToString ("mm\\:ss");
				ticks = pm.Model.Player.PlayDeltaTime.ToString ("D08");
				tempo = "     " + pm.Model.Player.Bpm.ToString ("D03");
				meter = meter_string;
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


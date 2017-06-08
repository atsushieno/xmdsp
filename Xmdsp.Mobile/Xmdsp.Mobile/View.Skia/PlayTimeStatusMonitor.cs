using System;
using Commons.Music.Midi;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace Xmdsp
{
	public class PlayTimeStatusMonitor : SKCanvasView
	{
		Presenter pm;
		
		public PlayTimeStatusMonitor (Presenter pm)
		{
			this.pm = pm;

			pm.ScaleChanged += SetSize;

			pm.Model.PlayTimerTick += delegate {
				//Application.Invoke (() => QueueDraw ());
			};
			pm.Model.PlayerStateChanged += delegate {
				//Application.Invoke (() => QueueDraw ());
			};
		}

		internal void SetSize ()
		{
			WidthRequest = pm.PlayTimeStatusMonitor.Width * pm.Scale;
			HeightRequest = pm.PlayTimeStatusMonitor.Height * pm.Scale;
		}
		
		WeakReference cached_player;
		string total_time_string;

		// FIXME: specify font family (but how?)
		protected override void OnPaintSurface (SKPaintSurfaceEventArgs e)
		{
			base.OnPaintSurface (e);
			var ctx = e.Surface.Canvas;
			var dirtyRect = e.Info.Rect;

			ctx.Scale ((float) pm.Scale, (float) pm.Scale);

			var paint = new SKPaint () {
				Color = pm.Pallette.CommonTextDarkest.ToSkia (),
				StrokeWidth = 1,
				IsStroke = true
			};
			for (int i = 0; i < 5; i++) {
				var y = 30 * i + 24 + 1;
				ctx.DrawLine (0, y, 180, y, paint);
			}

			paint.Color = pm.Pallette.CommonTextMiddle.ToSkia ();
			paint.IsStroke = false;
			for (int i = 0; i < 5; i++)
				ctx.DrawRect (new SKRect (0, 30 * i, 5, 24), paint);
			
			ctx.Translate (10, 0);

			paint.TextSize = 10;

			ctx.DrawText ("Passed", 0, 0, paint);
			ctx.DrawText ("Time", 10, 12, paint);
			ctx.DrawText ("Total", 0, 30, paint);
			ctx.DrawText ("Time", 10, 42, paint);
			ctx.DrawText ("Tick", 0, 60, paint);
			ctx.DrawText ("Count", 10, 72, paint);
			ctx.DrawText ("Tempo", 0, 90, paint);
			ctx.DrawText ("Meter", 0, 120, paint);
			
			ctx.Translate (60, 0);
			paint.TextSize = 16;

			string playTime, totalTime, ticks, tempo, meter;
			
			if (pm.Model.Player != null) {
				if (cached_player == null || !cached_player.IsAlive) {
					cached_player = new WeakReference (pm.Model.Player);
					total_time_string = TimeSpan.FromMilliseconds (((MidiPlayer) cached_player.Target).GetTotalPlayTimeMilliseconds ()).ToString ("mm\\:ss");
				}
				totalTime = "   " + total_time_string;
				playTime = "   " + pm.Model.Player.PositionInTime.ToString ("mm\\:ss");
				ticks = pm.Model.Player.PlayDeltaTime.ToString ("D08");
				tempo = "     " + pm.Model.Player.Bpm.ToString ("D03");
				var data = pm.Model.Player.TimeSignature;
				meter = "     " + data [0] + "/" + Math.Pow (2, data [1]);
			} else {
				playTime = "   --:--";
				totalTime = "   --:--";
				ticks = "--------";
				tempo = "     ---";
				meter = "     -/-";
			}
				
			ctx.DrawText ("playTime", 0, 0, paint);
			ctx.DrawText ("totalTime", 0, 30, paint);
			ctx.DrawText ("ticks", 0, 60, paint);
			ctx.DrawText ("tempo", 0, 90, paint);
			ctx.DrawText ("meter", 0, 120, paint);
			
			ctx.Translate (-70, 0);
		}
	}
}


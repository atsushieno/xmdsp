using System;
using System.Collections.Generic;
using Commons.Music.Midi;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace Xmdsp
{
	public class PlayerStatusMonitor : SKCanvasView
	{
		Presenter pm;
		PlayerState state_to_draw = PlayerState.Stopped;
		SKPaint paint;
		
		public PlayerStatusMonitor (Presenter pm)
		{
			this.pm = pm;
			paint = new SKPaint () {
				TextSize = pm.KeyOnMeterList.KeyOnMeterTextSize,
				StrokeWidth = 2
			};

			pm.Model.PlayerStateChanged += state => {
				state_to_draw = state;
				//Application.Invoke (() => QueueDraw ());
			};

			SetSize ();
			pm.ScaleChanged += SetSize;

			actions [PlayerState.Playing] = (ctx,active) => {
				paint.IsStroke = false;
				ctx.DrawLine (4, 2, 9, 6, paint);
				ctx.DrawLine (9, 6, 4, 10, paint);
				ctx.DrawLine (4, 10, 4, 2, paint);
				};
			actions [PlayerState.FastForward] = (ctx,active) => {
				paint.IsStroke = true;
				ctx.DrawLine (3, 2, 6, 6, paint);
				ctx.DrawLine (6, 6, 3, 10, paint);
				ctx.DrawLine (6, 2, 9, 6, paint);
				ctx.DrawLine (9, 6, 6, 10, paint);
				};
			actions [PlayerState.Paused] = (ctx,active) => {
				paint.IsStroke = true;
				ctx.DrawLine (4, 2, 4, 10, paint);
				ctx.DrawLine (8, 2, 8, 10, paint);
				};
			actions [PlayerState.Stopped] = (ctx,active) => {
				ctx.DrawLine (2, 6, 10, 6, paint);
				};

			// FIXME: how to implement those buttons?
			/*
			this.ButtonPressed += (object sender, ButtonEventArgs e) => {
				if (e.Button != PointerButton.Left)
					return;
				if (new Rectangle (coordinates [PlayerState.FastForward], GetButtonSize ()).Contains (e.Position))
					pm.Model.StartFastForward ();
			};
			this.ButtonReleased += (object sender, ButtonEventArgs e) => {
				if (e.Button != PointerButton.Left)
					return;
				for (int i = 0; i < coordinates.Count; i++) {
					var stat = states [i];
					if (new Rectangle (coordinates [stat], GetButtonSize ()).Contains (GetScaledPosition (e.Position))) {
						switch (stat) {
						case PlayerState.Playing: pm.Model.Play (); break;
						case PlayerState.FastForward: pm.Model.StopFastForward (); break;
						case PlayerState.Paused: pm.Model.Pause (); break;
						case PlayerState.Stopped: pm.Model.Stop (); break;
						}
						break;
					}
				}
			};
			*/
		}

		internal void SetSize ()
		{
			WidthRequest = pm.PlayerStatusMonitor.Width * pm.Scale;
			HeightRequest = pm.PlayerStatusMonitor.Height * pm.Scale;
			var textWidth = pm.PlayerStatusMonitor.TextSize * 6;
			coordinates [PlayerState.Playing] = new SKPoint (0, 0);
			coordinates [PlayerState.FastForward] = new SKPoint ((float) (textWidth * pm.Scale), 0);
			coordinates [PlayerState.Paused] = new SKPoint (0, (float) (20 * pm.Scale));
			coordinates [PlayerState.Stopped] = new SKPoint ((float) (textWidth * pm.Scale), (float) (20 * pm.Scale));
		}

		SKSize GetButtonSize ()
		{
			return new SKSize ((float) (16 * pm.Scale), (float) (16 * pm.Scale));
		}

		SKPoint GetScaledPosition (SKPoint point)
		{
			return new SKPoint ((float) (point.X * pm.Scale), (float) (point.Y * pm.Scale));
		}

		PlayerState [] states = new PlayerState[] {PlayerState.Playing, PlayerState.FastForward, PlayerState.Paused, PlayerState.Stopped};
		Dictionary<PlayerState, SKPoint> coordinates = new Dictionary<PlayerState, SKPoint> ();
		Dictionary<PlayerState, Action<SKCanvas,bool>> actions = new Dictionary<PlayerState, Action<SKCanvas,bool>> ();

		PlayerState last_state = PlayerState.Playing; // which is not true at startup

		protected override void OnPaintSurface (SKPaintSurfaceEventArgs e)
		{
			base.OnPaintSurface (e);
			var ctx = e.Surface.Canvas;
			var dirtyRect = e.Info.Rect;

			//if (last_state == state_to_draw)
			//	return;
			last_state = state_to_draw;

			ctx.Scale ((float)pm.Scale, (float)pm.Scale);
			var pmp = pm.PlayerStatusMonitor;

			paint.Color = pm.Pallette.ApplicationBackgroundColor.ToSkia ();
			paint.IsStroke = false;
			ctx.DrawRect (dirtyRect, paint);
			Action<string, PlayerState> func = (label, state) => {
				var co = coordinates [state];
				DrawText (ctx, pmp.TextSize, pm.Pallette.CommonTextMiddle, label, co.X + pmp.BaseIconSize, co.Y);
				DrawItem (ctx, state);
			};
			func ("Play", PlayerState.Playing);
			func ("FF", PlayerState.FastForward);
			func ("Pause", PlayerState.Paused);
			func ("Stop", PlayerState.Stopped);
		}

		void DrawText (SKCanvas ctx, int size, Presenter.Color color, string text, double x, double y)
		{
			DrawingHelper.DrawText (ctx, paint, size, color, text, x, y);
		}

		void DrawItem (SKCanvas ctx, PlayerState target)
		{
			var drawContent = actions [target];
			var offset = coordinates [target];
			bool active = state_to_draw == target;

			paint.StrokeWidth = 1;

			ctx.Translate (offset);
			if (active) {
				var outerRect = new SKRect (0, 0, 16, 16);
				ctx.DrawRect (outerRect, paint);
			}
			
			ctx.Translate (2, 2);

			var rect = new SKRect (0, 0, 12, 12);
			paint.Color = (active ? pm.Pallette.PlayerStateActiveBackground.ToSkia () : pm.Pallette.PlayerStateInactiveBackground.ToSkia ());
			paint.IsStroke = false;
			ctx.DrawRect (rect, paint);
			paint.Color = (active ? pm.Pallette.PlayerStateActiveStroke.ToSkia () : pm.Pallette.PlayerStateInactiveStroke.ToSkia ());
			paint.IsStroke = true;
			ctx.DrawRect (rect, paint);
			ctx.DrawRect (rect, paint);

			paint.Color = (active ? pm.Pallette.PlayerStateActiveStroke.ToSkia () : pm.Pallette.PlayerStateInactiveStroke.ToSkia ());
			drawContent (ctx, active);
			
			ctx.Translate (-2, -2);
			ctx.Translate (-offset.X, -offset.Y);
		}
	}
}


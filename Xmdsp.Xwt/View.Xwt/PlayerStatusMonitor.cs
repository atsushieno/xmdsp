using System;
using Xwt;
using Xwt.Drawing;
using System.Collections.Generic;
using Commons.Music.Midi;

namespace Xmdsp
{
	public class PlayerStatusMonitor : Canvas
	{
		Presenter pm;
		PlayerState state_to_draw = PlayerState.Stopped;
		Font font;
		
		public PlayerStatusMonitor (Presenter pm)
		{
			this.pm = pm;
			font = this.Font.WithSize (pm.KeyOnMeterList.KeyOnMeterTextSize);

			pm.Model.PlayerStateChanged += state => {
				state_to_draw = state;
				QueueDraw ();
			};

			pm.ScaleChanged += SetSize;

			actions [PlayerState.Playing] = (ctx,active) => {
				ctx.MoveTo (4, 2);
				ctx.LineTo (9, 6);
				ctx.LineTo (4, 10);
				ctx.LineTo (4, 2);
				ctx.Fill ();
				};
			actions [PlayerState.Paused] = (ctx,active) => {
				ctx.SetLineWidth (2);
				ctx.MoveTo (4, 2);
				ctx.LineTo (4, 10);
				ctx.MoveTo (8, 2);
				ctx.LineTo (8, 10);
				ctx.Stroke ();
				};
			actions [PlayerState.Stopped] = (ctx,active) => {
				ctx.SetLineWidth (2);
				ctx.MoveTo (2, 6);
				ctx.LineTo (10, 6);
				ctx.Stroke ();
				};
			
			this.ButtonReleased += (object sender, ButtonEventArgs e) => {
				if (e.Button != PointerButton.Left)
					return;
				for (int i = 0; i < coordinates.Count; i++) {
					var stat = states [i];
					if (new Rectangle (GetScaledPosition (coordinates [stat]), GetButtonSize ()).Contains (e.Position)) {
						switch (stat) {
						case PlayerState.Playing: pm.Model.Play (); break;
						case PlayerState.Paused: pm.Model.Pause (); break;
						case PlayerState.Stopped: pm.Model.Stop (); break;
						}
						break;
					}
				}
			};
		}

		internal void SetSize ()
		{
			WidthRequest = pm.PlayerStatusMonitor.Width * pm.Scale;
			HeightRequest = pm.PlayerStatusMonitor.Height * pm.Scale;
			var textWidth = pm.PlayerStatusMonitor.TextSize * 6;
			coordinates [PlayerState.Playing] = new Point (0, 0);
			coordinates [PlayerState.Paused] = new Point (0, 20 * pm.Scale);
			coordinates [PlayerState.Stopped] = new Point (textWidth * pm.Scale, 20 * pm.Scale);
		}

		Size GetButtonSize ()
		{
			return new Size (16 * pm.Scale, 16 * pm.Scale);
		}

		Point GetScaledPosition (Point point)
		{
			return new Point (point.X * pm.Scale, point.Y * pm.Scale); 
		}

		PlayerState [] states = new PlayerState[] {PlayerState.Playing, PlayerState.Paused, PlayerState.Stopped};
		Dictionary<PlayerState, Point> coordinates = new Dictionary<PlayerState, Point> ();
		Dictionary<PlayerState, Action<Context,bool>> actions = new Dictionary<PlayerState, Action<Context,bool>> ();

		PlayerState last_state = PlayerState.Playing; // which is not true at startup

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);
			//if (last_state == state_to_draw)
			//	return;
			last_state = state_to_draw;
			
			ctx.Scale (pm.Scale, pm.Scale);
			var pmp = pm.PlayerStatusMonitor;

			ctx.SetColor (pm.Pallette.ApplicationBackgroundColor.ToXwt ());
			ctx.Rectangle (dirtyRect);
			ctx.Fill ();
			Action<string,PlayerState> func = (label, state) => {
				var co = coordinates [state];
				DrawText (ctx, font, pmp.TextSize, pm.Pallette.CommonTextMiddle, label, co.X + pmp.BaseIconSize, co.Y);
				DrawItem (ctx, state);
			};
			func ("Play", PlayerState.Playing);
			func ("Pause", PlayerState.Paused);
			func ("Stop", PlayerState.Stopped);
		}

		Size DrawText (Context ctx, Font font, int size, Presenter.Color color, string text, double x, double y)
		{
			return DrawingHelper.DrawText (ctx, font, size, color, text, x, y);
		}

		void DrawItem (Context ctx, PlayerState target)
		{
			var drawContent = actions [target];
			var offset = coordinates [target];
			bool active = state_to_draw == target;

			ctx.SetLineWidth (1);

			ctx.Translate (offset);
			if (active) {
				var outerRect = new Rectangle (0, 0, 16, 16);
				ctx.Rectangle (outerRect);
			}
			
			ctx.Translate (2, 2);

			var rect = new Rectangle (0, 0, 12, 12);
			ctx.SetColor (active ? pm.Pallette.PlayerStateActiveBackground.ToXwt () : pm.Pallette.PlayerStateInactiveBackground.ToXwt ());
			ctx.Rectangle (rect);
			ctx.Fill ();
			ctx.SetColor (active ? pm.Pallette.PlayerStateActiveStroke.ToXwt () : pm.Pallette.PlayerStateInactiveStroke.ToXwt ());
			ctx.Rectangle (rect);
			ctx.Stroke ();
			
			ctx.SetColor (active ? pm.Pallette.PlayerStateActiveStroke.ToXwt () : pm.Pallette.PlayerStateInactiveStroke.ToXwt ());
			drawContent (ctx, active);
			
			ctx.Translate (-2, -2);
			ctx.Translate (-offset.X, -offset.Y);
		}
	}
}


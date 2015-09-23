using System;
using Xwt;
using Xwt.Drawing;
using System.Collections.Generic;
using Commons.Music.Midi;

namespace Xmdsp
{
	public class PlayerStatusMonitor : Canvas
	{
		ViewModel vm;
		PlayerState state_to_draw = PlayerState.Stopped;
		Font font;
		
		public PlayerStatusMonitor (ViewModel vm)
		{
			this.vm = vm;
			font = this.Font.WithSize (vm.KeyOnMeterList.KeyOnMeterTextSize);

			vm.Model.PlayerStateChanged += state => {
				state_to_draw = state;
				QueueDraw ();
			};
			
			WidthRequest = vm.PlayerStatusMonitor.Width;
			HeightRequest = vm.PlayerStatusMonitor.Height;
			var textWidth = vm.PlayerStatusMonitor.TextSize * 6;

			coordinates [PlayerState.Playing] = new Point (0, 0);
			coordinates [PlayerState.FastForward] = new Point (textWidth, 0);
			coordinates [PlayerState.Paused] = new Point (0, 20);
			coordinates [PlayerState.Stopped] = new Point (textWidth, 20);
			actions [PlayerState.Playing] = (ctx,active) => {
				ctx.MoveTo (4, 2);
				ctx.LineTo (9, 6);
				ctx.LineTo (4, 10);
				ctx.LineTo (4, 2);
				ctx.Fill ();
				};
			actions [PlayerState.FastForward] = (ctx,active) => {
				ctx.SetLineWidth (2);
				ctx.MoveTo (3, 2);
				ctx.LineTo (6, 6);
				ctx.LineTo (3, 10);
				ctx.MoveTo (6, 2);
				ctx.LineTo (9, 6);
				ctx.LineTo (6, 10);
				ctx.Stroke ();
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
			
			this.ButtonPressed += (object sender, ButtonEventArgs e) => {
				if (e.Button != PointerButton.Left)
					return;
				if (new Rectangle (coordinates [PlayerState.FastForward], new Size (16, 16)).Contains (e.Position))
					vm.Model.StartFastForward ();
			};
			this.ButtonReleased += (object sender, ButtonEventArgs e) => {
				if (e.Button != PointerButton.Left)
					return;
				for (int i = 0; i < coordinates.Count; i++) {
					var stat = states [i];
					if (new Rectangle (coordinates [stat], new Size (16, 16)).Contains (e.Position)) {
						switch (stat) {
						case PlayerState.Playing: vm.Model.Play (); break;
						case PlayerState.FastForward: vm.Model.StopFastForward (); break;
						case PlayerState.Paused: vm.Model.Pause (); break;
						case PlayerState.Stopped: vm.Model.Stop (); break;
						}
						break;
					}
				}
			};
		}

		PlayerState [] states = new PlayerState[] {PlayerState.Playing, PlayerState.FastForward, PlayerState.Paused, PlayerState.Stopped};
		Dictionary<PlayerState, Point> coordinates = new Dictionary<PlayerState, Point> ();
		Dictionary<PlayerState, Action<Context,bool>> actions = new Dictionary<PlayerState, Action<Context,bool>> ();
		
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);
			var vmp = vm.PlayerStatusMonitor;

			ctx.SetColor (vm.Pallette.ApplicationBackgroundColor.ToXwt ());
			ctx.Rectangle (dirtyRect);
			ctx.Fill ();
			Action<string,PlayerState> func = (label, state) => {
				var co = coordinates [state];
				DrawText (ctx, font, vmp.TextSize, vm.Pallette.CommonTextMiddle, label, co.X + vmp.BaseIconSize, co.Y);
				DrawItem (ctx, state);
			};
			func ("Play", PlayerState.Playing);
			func ("FF", PlayerState.FastForward);
			func ("Pause", PlayerState.Paused);
			func ("Stop", PlayerState.Stopped);
		}

		Size DrawText (Context ctx, Font font, int size, ViewModel.Color color, string text, double x, double y)
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
			ctx.SetColor (active ? vm.Pallette.PlayerStateActiveBackground.ToXwt () : vm.Pallette.PlayerStateInactiveBackground.ToXwt ());
			ctx.Rectangle (rect);
			ctx.Fill ();
			ctx.SetColor (active ? vm.Pallette.PlayerStateActiveStroke.ToXwt () : vm.Pallette.PlayerStateInactiveStroke.ToXwt ());
			ctx.Rectangle (rect);
			ctx.Stroke ();
			
			ctx.SetColor (active ? vm.Pallette.PlayerStateActiveStroke.ToXwt () : vm.Pallette.PlayerStateInactiveStroke.ToXwt ());
			drawContent (ctx, active);
			
			ctx.Translate (-2, -2);
			ctx.Translate (-offset.X, -offset.Y);
		}
	}
}


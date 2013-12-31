using System;
using Gtk;
using Gdk;
using Commons.Music.Midi.Player;
using System.Collections.Generic;

namespace Xmdsp
{
	public class PlayerStatusMonitor : DrawingArea
	{
		ViewModel vm;
		PlayerState state_to_draw = PlayerState.Stopped;
		
		public PlayerStatusMonitor (ViewModel vm)
		{
			this.vm = vm;
			vm.Model.PlayerStateChanged += state => {
				state_to_draw = state;
				QueueDraw ();
			};
			
			WidthRequest = vm.PlayerStatusMonitor.Width;
			HeightRequest = vm.PlayerStatusMonitor.Height;

			coordinates [PlayerState.Playing] = new Point (0, 0);
			coordinates [PlayerState.FastForward] = new Point (20, 0);
			coordinates [PlayerState.Paused] = new Point (40, 0);
			coordinates [PlayerState.Stopped] = new Point (60, 0);
			actions [PlayerState.Playing] = (ctx,active) => {
				ctx.MoveTo (4, 2);
				ctx.LineTo (9, 6);
				ctx.LineTo (4, 10);
				ctx.LineTo (4, 2);
				ctx.Fill ();
				};
			actions [PlayerState.FastForward] = (ctx,active) => {
				ctx.LineWidth = 2;
				ctx.MoveTo (3, 2);
				ctx.LineTo (6, 6);
				ctx.LineTo (3, 10);
				ctx.MoveTo (6, 2);
				ctx.LineTo (9, 6);
				ctx.LineTo (6, 10);
				ctx.Stroke ();
				};
			actions [PlayerState.Paused] = (ctx,active) => {
				ctx.LineWidth = 2;
				ctx.MoveTo (4, 2);
				ctx.LineTo (4, 10);
				ctx.MoveTo (8, 2);
				ctx.LineTo (8, 10);
				ctx.Stroke ();
				};
			actions [PlayerState.Stopped] = (ctx,active) => {
				ctx.LineWidth = 2;
				ctx.MoveTo (2, 6);
				ctx.LineTo (10, 6);
				ctx.Stroke ();
				};
			
			/*
			this.ButtonPressed += (object sender, ButtonEventArgs e) => {
				if (e.Button != PointerButton.Left)
					return;
				if (new Rectangle (coordinates [PlayerState.FastForward], new Size (16, 16)).Contains (e.Position))
					vm.Model.StartFastForward ();
			};
			this.MnemonicActivated += (sender, e) => {
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
			*/
		}

		PlayerState [] states = new PlayerState[] {PlayerState.Playing, PlayerState.FastForward, PlayerState.Paused, PlayerState.Stopped};
		Dictionary<PlayerState, Point> coordinates = new Dictionary<PlayerState, Point> ();
		Dictionary<PlayerState, Action<Cairo.Context,bool>> actions = new Dictionary<PlayerState, Action<Cairo.Context,bool>> ();

		protected override bool OnExposeEvent (EventExpose evnt)
		{
			var ctx = Gdk.CairoHelper.Create (GdkWindow);
			
			CairoHelper.SetSourceColor (ctx, vm.Pallette.ApplicationBackgroundColor.ToGdk ());
			foreach (var rect in evnt.Region.GetRectangles ()) {
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Fill ();
			}
			DrawItem (ctx, PlayerState.Playing);
			DrawItem (ctx, PlayerState.FastForward);
			DrawItem (ctx, PlayerState.Paused);
			DrawItem (ctx, PlayerState.Stopped);
			ctx.Dispose ();
			return true;
		}
		
		void DrawItem (Cairo.Context ctx, PlayerState target)
		{
			var drawContent = actions [target];
			var offset = coordinates [target];
			bool active = state_to_draw == target;

			ctx.LineWidth = 1;

			ctx.Translate (offset.X, offset.Y);
			if (active) {
				var outerRect = new Cairo.Rectangle (0, 0, 16, 16);
				ctx.Rectangle (outerRect);
			}
			
			ctx.Translate (2, 2);

			var rect = new Cairo.Rectangle (0, 0, 12, 12);
			CairoHelper.SetSourceColor (ctx, active ? vm.Pallette.PlayerStateActiveBackground.ToGdk () : vm.Pallette.PlayerStateInactiveBackground.ToGdk ());
			ctx.Rectangle (rect);
			ctx.Fill ();
			CairoHelper.SetSourceColor (ctx, active ? vm.Pallette.PlayerStateActiveStroke.ToGdk () : vm.Pallette.PlayerStateInactiveStroke.ToGdk ());
			ctx.Rectangle (rect);
			ctx.Stroke ();
			
			CairoHelper.SetSourceColor (ctx, active ? vm.Pallette.PlayerStateActiveStroke.ToGdk () : vm.Pallette.PlayerStateInactiveStroke.ToGdk ());
			drawContent (ctx, active);
			
			ctx.Translate (-2, -2);
			ctx.Translate (-offset.X, -offset.Y);
		}
	}
}


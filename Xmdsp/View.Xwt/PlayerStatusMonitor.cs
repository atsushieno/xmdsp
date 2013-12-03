using System;
using Xwt;
using Xwt.Drawing;
using Commons.Music.Midi.Player;
using System.Collections.Generic;

namespace Xmdsp
{
	public class PlayerStatusMonitor : Canvas
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

			coodinates [PlayerState.Playing] = new Point (0, 0);
			coodinates [PlayerState.FastForward] = new Point (20, 0);
			coodinates [PlayerState.Paused] = new Point (40, 0);
			coodinates [PlayerState.Stopped] = new Point (60, 0);
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
		}

		Dictionary<PlayerState, Point> coodinates = new Dictionary<PlayerState, Point> ();
		Dictionary<PlayerState, Action<Context,bool>> actions = new Dictionary<PlayerState, Action<Context,bool>> ();
		
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);
			
			ctx.SetColor (vm.Pallette.ApplicationBackgroundColor.ToXwt ());
			ctx.Rectangle (dirtyRect);
			ctx.Fill ();
			DrawItem (ctx, PlayerState.Playing);
			DrawItem (ctx, PlayerState.FastForward);
			DrawItem (ctx, PlayerState.Paused);
			DrawItem (ctx, PlayerState.Stopped);
		}
		
		void DrawItem (Context ctx, PlayerState target)
		{
			var drawContent = actions [target];
			var offset = coodinates [target];
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


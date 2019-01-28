﻿using System;
using Xwt;
using Xwt.Drawing;

namespace Xmdsp
{
	public class ApplicationHeaderPane : Canvas
	{
		Presenter pm;
		
		public ApplicationHeaderPane (Presenter pm)
		{
			this.pm = pm;
			pm.ScaleChanged += SetSize;
		}

		internal void SetSize ()
		{
			WidthRequest = pm.ApplicationHeaderPane.Width * pm.Scale;
			HeightRequest = pm.ApplicationHeaderPane.Height * pm.Scale;
		}
		
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);
			ctx.Scale (pm.Scale, pm.Scale);

			ctx.SetLineWidth (1);
			
			var size = DrawingHelper.DrawText (ctx, Font, 18, pm.Pallette.CommonTextMiddle, "XMDSP", 0, 0);
			
			int leftEnd = (int) size.Width;
			
			ctx.MoveTo (0, 26);
			ctx.LineTo (leftEnd, 26);
			ctx.Stroke ();

			int rightStart = leftEnd + 5;

			string descText = "standard MIDI file visualizer and player";
			DrawingHelper.DrawText (ctx, Font, 8, pm.Pallette.CommonTextMiddle, descText, rightStart, 0);

			var detailsText = "version " + Model.VersionNumbers + " / with Xwt. (C)2013- atsushieno";
			size = DrawingHelper.DrawText (ctx, Font, 8, pm.Pallette.CommonTextMiddle, detailsText, rightStart, 11);
			
			int rightEnd = rightStart + (int) size.Width;
			
			ctx.MoveTo (rightStart, 26);
			ctx.LineTo (rightEnd, 26);
			ctx.Stroke ();
		}
	}
}


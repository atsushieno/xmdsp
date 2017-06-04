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
			
			var appName = new TextLayout ();
			appName.Text = "XMDSP";
			appName.Font = Font.WithSize (18);
			ctx.SetColor (pm.Pallette.CommonTextMiddle.ToXwt ());
			ctx.DrawTextLayout (appName, 0, 0);
			
			int leftEnd = (int) appName.GetSize ().Width;
			
			ctx.MoveTo (0, 26);
			ctx.LineTo (leftEnd, 26);
			ctx.Stroke ();

			int rightStart = leftEnd + 5;
			
			var appDesc = new TextLayout ();
			appDesc.Text = "standard MIDI file visualizer and player";
			appDesc.Font = Font.WithSize (8);
			ctx.SetColor (pm.Pallette.CommonTextDarkest.ToXwt ());
			ctx.DrawTextLayout (appDesc, rightStart, 0);

			var appDetails = new TextLayout ();
			appDetails.Text = "version " + Model.VersionNumbers + " / with Xwt. (C)2013- atsushieno";
			appDetails.Font = Font.WithSize (8);
			ctx.SetColor (pm.Pallette.CommonTextDarkest.ToXwt ());
			ctx.DrawTextLayout (appDetails, rightStart, 11);
			
			int rightEnd = rightStart + (int) appDetails.GetSize ().Width;
			
			ctx.MoveTo (rightStart, 26);
			ctx.LineTo (rightEnd, 26);
			ctx.Stroke ();
		}
	}
}


using System;
using Gtk;
using Gdk;

namespace Xmdsp
{
	public class ApplicationHeaderPane : DrawingArea
	{
		ViewModel vm;
		
		public ApplicationHeaderPane (ViewModel vm)
		{
			this.vm = vm;
			
			WidthRequest = vm.ApplicationHeaderPane.Width;
			HeightRequest = vm.ApplicationHeaderPane.Height;
		}
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			var ctx = Gdk.CairoHelper.Create (GdkWindow);

			ctx.LineWidth = 1;

			string appName = "XMDSP";
			CairoHelper.SetSourceColor (ctx, vm.Pallette.CommonTextMiddle.ToGdk ());
			DrawText (ctx, 18, appName);
			
			int leftEnd = (int) ctx.TextExtents (appName).Width;
			
			ctx.MoveTo (0, 26);
			ctx.LineTo (leftEnd, 26);
			ctx.Stroke ();

			int rightStart = leftEnd + 5;
			
			string appDesc = "standard MIDI file visualizer and player";
			CairoHelper.SetSourceColor (ctx, vm.Pallette.CommonTextDarkest.ToGdk ());
			ctx.MoveTo (rightStart, 0);
			DrawText (ctx, 8, appDesc);

			string appDetails = "version " + Model.VersionNumbers + " / with Xwt. (C)2013- atsushieno";
			CairoHelper.SetSourceColor (ctx, vm.Pallette.CommonTextDarkest.ToGdk ());
			ctx.MoveTo (rightStart, 11);
			DrawText (ctx, 8, appDetails);
			
			int rightEnd = rightStart + (int) ctx.TextExtents (appDetails).Width;
			
			ctx.MoveTo (rightStart, 26);
			ctx.LineTo (rightEnd, 26);
			ctx.Stroke ();

			ctx.Dispose ();
			return true;
		}
		
		void DrawText (Cairo.Context ctx, int size, string text)
		{
			ctx.SetFontSize (size);
			ctx.Translate (0, size);
			ctx.TextPath (text);
			ctx.Translate (0, -size);
		}
	}
}


using System;
using Xwt;
using Xwt.Drawing;
using Commons.Music.Midi;
using System.Collections.Generic;

namespace Xmdsp
{
	public class Keyboard : Canvas
	{
		ViewModel vm;
		
		public Keyboard (ViewModel viewModel)
		{
			this.vm = viewModel;
			var vmk = vm.Keyboard;
			int octaves = vmk.MaxKeys / 12;
			Margin = 0;
			WidthRequest = vmk.WhiteKeyWidth * 7 * (octaves - vmk.VisibleOctaves);
			HeightRequest = vmk.WhiteKeyHeight;
			this.BackgroundColor = vm.Pallette.KeyboardBackgroundColor.ToXwt ();
		}
		
		Queue<SmfMessage> received_messages = new Queue<SmfMessage> ();
		Queue<SmfMessage> drawing_messages = new Queue<SmfMessage> ();
		
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);
			
			if (!RequestDrawForMessage (ctx, dirtyRect) && !DrawMessage (ctx, dirtyRect))
				DrawDefault (ctx, dirtyRect);
		}
		
		bool RequestDrawForMessage (Context ctx, Rectangle dirtyRect)
		{
			if (received_messages.Count == 0)
				return false;
			var vmk = vm.Keyboard;
			var m = received_messages.Dequeue ();
			ViewModel.Rectangle rect;
			if (vmk.IsBlackKey (m.Msb))
				rect = vmk.GetBlackKeyRect (m.Msb);
			else
				rect = vmk.GetWhiteKeyRect (m.Msb);
			drawing_messages.Enqueue (m);
			QueueDraw (new Rectangle (rect.X, rect.Y, rect.Width, rect.Height));
			return true;
		}
		
		bool DrawMessage (Context ctx, Rectangle dirtyRect)
		{
			if (drawing_messages.Count == 0)
				return false;
			var vmk = vm.Keyboard;
			var m = drawing_messages.Dequeue ();
			if (vmk.IsBlackKey (m.Msb)) {
				var rect = vmk.GetBlackKeyRect (m.Msb);
				ctx.SetColor ((m.MessageType == SmfMessage.NoteOn ? vm.Pallette.NoteOnColor : vm.Pallette.BlackKeyFillColor).ToXwt ());
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Fill ();
				ctx.SetColor (vm.Pallette.BlackKeyStrokeColor.ToXwt ());
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Stroke ();
			} else {
				int x = vmk.GetWhiteKeyX (m.Msb);
				ctx.SetColor ((m.MessageType == SmfMessage.NoteOn ? vm.Pallette.NoteOnColor : vm.Pallette.WhiteKeyFillColor).ToXwt ());
				ctx.NewPath ();
				var path = new DrawingPath ();
				var x2 = x + vmk.WhiteKeyWidth;
				var x3 = x + vmk.BlackKeyShiftWidth;
				var yd = vmk.WhiteKeyHeight - vmk.BlackKeyHeight + 1;
				
				//path.MoveTo (x, 0);
				path.MoveTo (x, yd);
				
				path.LineTo (x, vmk.WhiteKeyHeight);
				path.LineTo (x2, vmk.WhiteKeyHeight);
				path.LineTo (x2, yd);
				path.LineTo (x3, yd);
				//path.LineTo (x3, 0);
				path.ClosePath ();
				ctx.AppendPath (path);
				ctx.Fill ();
				//ctx.SetColor (vm.Pallette.WhiteKeyStrokeColor.ToXwt ());
				//ctx.AppendPath (path);
				//ctx.Stroke ();
			}
			return true;
		}
			
		void DrawDefault (Context ctx, Rectangle dirtyRect)
		{			
			if (Bounds.IsEmpty || dirtyRect.IsEmpty)
				return;
			
			Console.WriteLine ("Keyboard.OnDraw()");
			
			var vmk = vm.Keyboard;
			
			Color whiteKeyFillColor = vm.Pallette.WhiteKeyFillColor.ToXwt ();
			Color whiteKeyStrokeColor = vm.Pallette.WhiteKeyStrokeColor.ToXwt ();
			Color blackKeyFillColor = vm.Pallette.BlackKeyFillColor.ToXwt ();
			Color blackKeyStrokeColor = vm.Pallette.BlackKeyStrokeColor.ToXwt ();

			int octaves = vmk.MaxKeys / 12;
			int wwidth = vmk.WhiteKeyWidth;
			int wheight = vmk.WhiteKeyHeight;
			ctx.SetLineWidth (1);
			foreach (var rect in vmk.WhiteKeyRectangles ()) {
				ctx.SetColor (whiteKeyFillColor);
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Fill ();
				ctx.SetColor (whiteKeyStrokeColor);
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Stroke ();
			}
			foreach (var rect in vmk.BlackKeyRectangles ()) {
				ctx.SetColor (blackKeyFillColor);
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Fill ();
				ctx.SetColor (blackKeyStrokeColor);
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Stroke ();
			}
		}
		
		static readonly Rectangle edge = new Rectangle (0, 0, 1, 1);
		
		public void ProcessMidiMessage (SmfMessage m)
		{
			received_messages.Enqueue (m);
			QueueDraw (edge);
		}
	}
}

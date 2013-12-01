using System;
using System.Linq;
using Xwt;
using Xwt.Drawing;
using Commons.Music.Midi;
using System.Collections.Generic;
using System.Threading;

namespace Xmdsp
{
	public class Keyboard
	{
		ViewModel vm;
		int channel;
		
		public Keyboard (ViewModel viewModel, int channel)
		{
			this.vm = viewModel;
			this.channel = channel;
			var vmk = vm.Keyboard;
			int octaves = vmk.MaxKeys / 12;
			key_on_status = new bool [vm.Keyboard.MaxKeys];
		}
		
		bool [] key_on_status;
		
		bool DrawMessage (Context ctx, Rectangle dirtyRect, SmfMessage m)
		{
			var vmk = vm.Keyboard;
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

		internal void DoDraw (Context ctx)
		{
			var vmk = vm.Keyboard;
			
			int yOffset = this.channel * (vmk.Height + vm.KeyboardParameterBlock.Height) + vm.KeyboardParameterBlock.Height;
			ctx.Translate (0, yOffset);
			
			Color noteOnColor = vm.Pallette.NoteOnColor.ToXwt ();
			Color whiteKeyFillColor = vm.Pallette.WhiteKeyFillColor.ToXwt ();
			Color whiteKeyStrokeColor = vm.Pallette.WhiteKeyStrokeColor.ToXwt ();
			Color blackKeyFillColor = vm.Pallette.BlackKeyFillColor.ToXwt ();
			Color blackKeyStrokeColor = vm.Pallette.BlackKeyStrokeColor.ToXwt ();

			int octaves = vmk.MaxKeys / 12;
			int wwidth = vmk.WhiteKeyWidth;
			int wheight = vmk.WhiteKeyHeight;
			ctx.SetLineWidth (1);
			int n = 0;
			foreach (var rect in vmk.WhiteKeyRectangles ()) {
				int key = n / 7 * 12 + white_key_index_to_note [n % 7];
				if (key_on_status [key])
					ctx.SetColor (noteOnColor);
				else
					ctx.SetColor (whiteKeyFillColor);
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Fill ();
				ctx.SetColor (whiteKeyStrokeColor);
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Stroke ();
				n++;
			}
			n = 0;
			foreach (var rect in vmk.BlackKeyRectangles ()) {
				int key = n / 5 * 12 + black_key_index_to_note [n % 5];
				if (key_on_status [key])
					ctx.SetColor (noteOnColor);
				else
					ctx.SetColor (blackKeyFillColor);
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Fill ();
				ctx.SetColor (blackKeyStrokeColor);
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Stroke ();
				n++;
			}
			ctx.Translate (0, -yOffset);
		}
		
		static readonly byte [] white_key_index_to_note = {0, 2, 4, 5, 7 ,9, 11};
		static readonly byte [] black_key_index_to_note = {1, 3, 6, 8, 10};
		
		public void ProcessMidiMessage (SmfMessage m)
		{
			switch (m.MessageType) {
			case SmfMessage.NoteOn:
				key_on_status [m.Msb] = m.Lsb > 0;
				break;
			case SmfMessage.NoteOff:
				key_on_status [m.Msb] = false;
				break;
			}
		}
	}
}

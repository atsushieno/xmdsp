using System;
using System.Linq;
using Gtk;
using Gdk;
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
		
		bool DrawMessage (Cairo.Context ctx, Rectangle dirtyRect, SmfMessage m)
		{
			var vmk = vm.Keyboard;
			if (vmk.IsBlackKey (m.Msb)) {
				var rect = vmk.GetBlackKeyRect (m.Msb);
				CairoHelper.SetSourceColor (ctx, (m.MessageType == SmfMessage.NoteOn ? vm.Pallette.NoteOnColor : vm.Pallette.BlackKeyFillColor).ToGdk ());
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Fill ();
				CairoHelper.SetSourceColor (ctx, vm.Pallette.BlackKeyStrokeColor.ToGdk ());
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Stroke ();
			} else {
				int x = vmk.GetWhiteKeyX (m.Msb);
				CairoHelper.SetSourceColor (ctx, (m.MessageType == SmfMessage.NoteOn ? vm.Pallette.NoteOnColor : vm.Pallette.WhiteKeyFillColor).ToGdk ());
				ctx.NewPath ();
				var x2 = x + vmk.WhiteKeyWidth;
				var x3 = x + vmk.BlackKeyShiftWidth;
				var yd = vmk.WhiteKeyHeight - vmk.BlackKeyHeight + 1;
				
				//path.MoveTo (x, 0);
				ctx.MoveTo (x, yd);
				
				ctx.LineTo (x, vmk.WhiteKeyHeight);
				ctx.LineTo (x2, vmk.WhiteKeyHeight);
				ctx.LineTo (x2, yd);
				ctx.LineTo (x3, yd);
				//path.LineTo (x3, 0);
				ctx.ClosePath ();
				ctx.Fill ();
				/*
				CairoHelper.SetSourceColor (ctx, vm.Pallette.WhiteKeyStrokeColor.ToGdk ());
				ctx.MoveTo (x, yd);
				
				ctx.LineTo (x, vmk.WhiteKeyHeight);
				ctx.LineTo (x2, vmk.WhiteKeyHeight);
				ctx.LineTo (x2, yd);
				ctx.LineTo (x3, yd);
				ctx.Stroke ();
				*/
			}
			return true;
		}
		
		bool dirty = true;

		internal void DoDraw (Cairo.Context ctx)
		{
//			if (!dirty)
//				return;
			
			var vmk = vm.Keyboard;
			
			int yOffset = this.channel * (vmk.Height + vm.KeyboardParameterBlock.Height) + vm.KeyboardParameterBlock.Height;
			ctx.Translate (0, yOffset);
			
			Color noteOnColor = vm.Pallette.NoteOnColor.ToGdk ();
			Color whiteKeyFillColor = vm.Pallette.WhiteKeyFillColor.ToGdk ();
			Color whiteKeyStrokeColor = vm.Pallette.WhiteKeyStrokeColor.ToGdk ();
			Color blackKeyFillColor = vm.Pallette.BlackKeyFillColor.ToGdk ();
			Color blackKeyStrokeColor = vm.Pallette.BlackKeyStrokeColor.ToGdk ();

			int octaves = vmk.MaxKeys / 12;
			int wwidth = vmk.WhiteKeyWidth;
			int wheight = vmk.WhiteKeyHeight;
			ctx.LineWidth = 1;
			int n = 0;
			foreach (var rect in vmk.WhiteKeyRectangles ()) {
				int key = n / 7 * 12 + white_key_index_to_note [n % 7];
				if (key_on_status [key])
					CairoHelper.SetSourceColor (ctx, noteOnColor);
				else
					CairoHelper.SetSourceColor (ctx, whiteKeyFillColor);
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Fill ();
				CairoHelper.SetSourceColor (ctx, whiteKeyStrokeColor);
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Stroke ();
				n++;
			}
			n = 0;
			foreach (var rect in vmk.BlackKeyRectangles ()) {
				int key = n / 5 * 12 + black_key_index_to_note [n % 5];
				if (key_on_status [key])
					CairoHelper.SetSourceColor (ctx, noteOnColor);
				else
					CairoHelper.SetSourceColor (ctx, blackKeyFillColor);
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Fill ();
				CairoHelper.SetSourceColor (ctx, blackKeyStrokeColor);
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Stroke ();
				n++;
			}
			ctx.Translate (0, -yOffset);
			dirty = false;
		}
		
		static readonly byte [] white_key_index_to_note = {0, 2, 4, 5, 7 ,9, 11};
		static readonly byte [] black_key_index_to_note = {1, 3, 6, 8, 10};
		
		public void ProcessMidiMessage (SmfMessage m)
		{
			switch (m.MessageType) {
			case SmfMessage.NoteOn:
				key_on_status [m.Msb] = m.Lsb > 0;
				dirty = true;
				break;
			case SmfMessage.NoteOff:
				key_on_status [m.Msb] = false;
				dirty = true;
				break;
			}
		}
	}
}

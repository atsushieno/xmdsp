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
		Presenter pm;
		int channel;
		
		public Keyboard (Presenter pm, int channel)
		{
			this.pm = pm;
			this.channel = channel;
			var pmk = pm.Keyboard;
			int octaves = pmk.VisibleOctaves;
			key_on_status = new bool [pm.Keyboard.MaxKeys];
			pm.Model.PlayerStateChanged += state => {
				if (state == PlayerState.Stopped)
					ResetKeyOnStatuses ();
			};
		}
		
		bool [] key_on_status;
		
		bool DrawMessage (Context ctx, Rectangle dirtyRect, MidiEvent m)
		{
			var pmk = pm.Keyboard;
			if (pmk.IsBlackKey (m.Msb)) {
				var rect = pmk.GetBlackKeyRect (m.Msb);
				ctx.SetColor ((m.EventType == MidiEvent.NoteOn ? pm.Pallette.NoteOnColor : pm.Pallette.BlackKeyFillColor).ToXwt ());
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Fill ();
				ctx.SetColor (pm.Pallette.BlackKeyStrokeColor.ToXwt ());
				ctx.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.Stroke ();
			} else {
				int x = pmk.GetWhiteKeyX (m.Msb);
				ctx.SetColor ((m.EventType == MidiEvent.NoteOn ? pm.Pallette.NoteOnColor : pm.Pallette.WhiteKeyFillColor).ToXwt ());
				ctx.NewPath ();
				var path = new DrawingPath ();
				var x2 = x + pmk.WhiteKeyWidth;
				var x3 = x + pmk.BlackKeyShiftWidth;
				var yd = pmk.WhiteKeyHeight - pmk.BlackKeyHeight + 1;
				
				//path.MoveTo (x, 0);
				path.MoveTo (x, yd);
				
				path.LineTo (x, pmk.WhiteKeyHeight);
				path.LineTo (x2, pmk.WhiteKeyHeight);
				path.LineTo (x2, yd);
				path.LineTo (x3, yd);
				//path.LineTo (x3, 0);
				path.ClosePath ();
				ctx.AppendPath (path);
				ctx.Fill ();
				path.Dispose ();
				//ctx.SetColor (vm.Pallette.WhiteKeyStrokeColor.ToXwt ());
				//ctx.AppendPath (path);
				//ctx.Stroke ();
			}
			return true;
		}

		internal void DoDraw (Context ctx)
		{
			var pmk = pm.Keyboard;
			
			int yOffset = this.channel * (pmk.Height + pm.KeyboardParameterBlock.Height) + pm.KeyboardParameterBlock.Height;
			ctx.Translate (0, yOffset);
			
			Color noteOnColor = pm.Pallette.NoteOnColor.ToXwt ();
			Color whiteKeyFillColor = pm.Pallette.WhiteKeyFillColor.ToXwt ();
			Color whiteKeyStrokeColor = pm.Pallette.WhiteKeyStrokeColor.ToXwt ();
			Color blackKeyFillColor = pm.Pallette.BlackKeyFillColor.ToXwt ();
			Color blackKeyStrokeColor = pm.Pallette.BlackKeyStrokeColor.ToXwt ();

			int wwidth = pmk.WhiteKeyWidth;
			int wheight = pmk.WhiteKeyHeight;
			ctx.SetLineWidth (1);
			int n = 0;
			foreach (var rect in pmk.WhiteKeyRectangles ()) {
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
			foreach (var rect in pmk.BlackKeyRectangles ()) {
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
		
		public void ProcessMidiMessage (MidiEvent m)
		{
			var n = m.Msb - pm.Keyboard.VisibleMinOctave * 12;
			switch (m.EventType) {
			case MidiEvent.NoteOn:
				key_on_status [n] = m.Lsb > 0;
				break;
			case MidiEvent.NoteOff:
				key_on_status [n] = false;
				break;
			}
		}

		void ResetKeyOnStatuses ()
		{
			for (int i = 0; i < key_on_status.Length; i++)
				key_on_status [i] = false;
		}
	}
}

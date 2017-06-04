using System;
using System.Linq;
using Commons.Music.Midi;
using SkiaSharp;

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
		
		bool DrawMessage (SKCanvas ctx, SKRect dirtyRect, MidiEvent m)
		{
			var pmk = pm.Keyboard;
			if (pmk.IsBlackKey (m.Msb)) {
				var rect = pmk.GetBlackKeyRect (m.Msb);
				var fill = new SKPaint () { Color = (m.EventType == MidiEvent.NoteOn ? pm.Pallette.NoteOnColor : pm.Pallette.BlackKeyFillColor).ToSkia (), IsStroke = false };
				ctx.DrawRect (new SKRect (rect.X, rect.Y, rect.Width, rect.Height), fill);
				var stroke = new SKPaint () { Color = pm.Pallette.BlackKeyStrokeColor.ToSkia (), IsStroke = true };
				ctx.DrawRect (new SKRect (rect.X, rect.Y, rect.Width, rect.Height), stroke);
			} else {
				int x = pmk.GetWhiteKeyX (m.Msb);
				var fill = new SKPaint () { Color = (m.EventType == MidiEvent.NoteOn ? pm.Pallette.NoteOnColor : pm.Pallette.WhiteKeyFillColor).ToSkia (), IsStroke = false };
				var path = new SKPath ();
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
				ctx.DrawPath (path, fill);
				//ctx.SetColor (vm.Pallette.WhiteKeyStrokeColor.ToSkia ());
				//ctx.AppendPath (path);
				//ctx.Stroke ();
			}
			return true;
		}

		internal void DoDraw (SKCanvas ctx)
		{
			var pmk = pm.Keyboard;
			
			int yOffset = this.channel * (pmk.Height + pm.KeyboardParameterBlock.Height) + pm.KeyboardParameterBlock.Height;
			ctx.Translate (0, yOffset);
			
			var noteOnColor = pm.Pallette.NoteOnColor.ToSkia ();
			var whiteKeyFillColor = pm.Pallette.WhiteKeyFillColor.ToSkia ();
			var whiteKeyStrokeColor = pm.Pallette.WhiteKeyStrokeColor.ToSkia ();
			var blackKeyFillColor = pm.Pallette.BlackKeyFillColor.ToSkia ();
			var blackKeyStrokeColor = pm.Pallette.BlackKeyStrokeColor.ToSkia ();

			var stroke = new SKPaint () { StrokeWidth = 1 };
			var fill = new SKPaint () { IsStroke = false };

			int wwidth = pmk.WhiteKeyWidth;
			int wheight = pmk.WhiteKeyHeight;

			int n = 0;
			stroke.Color = whiteKeyStrokeColor;
			foreach (var rect in pmk.WhiteKeyRectangles ()) {
				int key = n / 7 * 12 + white_key_index_to_note [n % 7];
				fill.Color = (key_on_status [key]) ? noteOnColor : whiteKeyFillColor;
				var skrect = new SKRect (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.DrawRect (skrect, fill);
				ctx.DrawRect (skrect, stroke);
				n++;
			}

			n = 0;
			stroke.Color = blackKeyStrokeColor;
			foreach (var rect in pmk.BlackKeyRectangles ()) {
				int key = n / 5 * 12 + black_key_index_to_note [n % 5];
				fill.Color = (key_on_status [key]) ? noteOnColor : blackKeyFillColor;
				var skrect = new SKRect (rect.X, rect.Y, rect.Width, rect.Height);
				ctx.DrawRect (skrect, fill);
				ctx.DrawRect (skrect, stroke);
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

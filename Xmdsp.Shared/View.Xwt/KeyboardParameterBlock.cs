using System;
using Xwt;
using Xwt.Drawing;
using Commons.Music.Midi;
using System.Collections.Generic;

namespace Xmdsp
{
	public class KeyboardParameterBlock
	{
		Presenter pm;
		int channel;		
		Font font;
		
		public KeyboardParameterBlock (Presenter pm, Font font, int channel)
		{
			this.pm = pm;
			this.font = font;
			
			this.channel = channel;
			
			Volume = Expression = Rsd = Csd = Dsd = SoftPedal = Sostenuto = 127;
			Hold = false;
			PortamentoSwitch = true;
		}
		
		int? Volume;
		int? Expression;
		int? Rsd;
		int? Csd;
		int? Dsd;
		bool? Hold;
		bool? PortamentoSwitch;
		int? SoftPedal;
		int? Sostenuto;

		// This processing model is ugly when we consider MV* patterns.
		// The intent of this "dispatch MIDI messages from VM and View preserves the state"
		// was that it could cache the parameters and skip unnecessary text drawing.
		// So far we draw everything so the entire nullable caching is unused.
		internal void DoDraw (Context ctx)
		{
			var pmk = pm.KeyboardParameterBlock;
			
			int yOffset = this.channel * (pmk.Height + pm.KeyboardParameterBlock.Height);
			ctx.Translate (0, yOffset);
			
			int row2Y = pmk.KeyBlockHeaderTextSize + 1;
			
			var midiSize = DrawText (ctx, font, pmk.KeyBlockHeaderTextSize, pm.Pallette.CommonTextDarkest, "MIDI", 0, 0);
			
			var trackSize = DrawText (ctx, font, pmk.KeyBlockHeaderTextSize, pm.Pallette.CommonTextBlightest, "TRACK.", 0, row2Y);
			
			DrawText (ctx, font, pmk.KeyBlockChannelNumberTextSize, pm.Pallette.CommonTextMiddle, (channel + 1).ToString ("D2"), Math.Max (midiSize.Width, trackSize.Width), 0);
			
			var size = DrawText (ctx, font, pmk.KeyBlockParameterTextSize, pm.Pallette.CommonTextDarkest, "VOL:", 100, 0);
			DrawText (ctx, font, pmk.KeyBlockParameterTextSize, pm.Pallette.CommonTextDarkest, "EXP:", 160, 0);
			DrawText (ctx, font, pmk.KeyBlockParameterTextSize, pm.Pallette.CommonTextDarkest, "RSD:", 100, row2Y);
			DrawText (ctx, font, pmk.KeyBlockParameterTextSize, pm.Pallette.CommonTextDarkest, "CSD:", 160, row2Y);
			DrawText (ctx, font, pmk.KeyBlockParameterTextSize, pm.Pallette.CommonTextDarkest, "DSD:", 220, row2Y);
			
			DrawText (ctx, font, pmk.KeyBlockParameterTextSize, pm.Pallette.CommonTextDarkest, "So:", 320, 0);
			DrawText (ctx, font, pmk.KeyBlockParameterTextSize, pm.Pallette.CommonTextDarkest, "SP:", 320, row2Y);
			
			UpdateParameters (ctx, font);
			
			ctx.Translate (0, -yOffset);
		}

		Size DrawText (Context ctx, Font font, int size, Presenter.Color color, string text, double x, double y)
		{
			return DrawingHelper.DrawText (ctx, font, size, color, text, x, y);
		}

		void DrawBoolSwitch (Context ctx, Font font, bool value, string label, int x, int y)
		{
			var pmk = pm.KeyboardParameterBlock;
			ctx.SetColor ((value ? pm.Pallette.CommonTextMiddle : pm.Pallette.KeyParameterBackgroundColor).ToXwt ());
			ctx.Rectangle (x - 1, y, pmk.KeyBlockParameterTextSize + 2, pmk.KeyBlockParameterTextSize + 3);
			ctx.Fill ();
			DrawText (ctx, font, pmk.KeyBlockParameterTextSize, value ? pm.Pallette.CommonTextBlightest : pm.Pallette.CommonTextDarkest, label, x, y);
		}
		
		void UpdateParameters (Context ctx, Font font)
		{
			var pmk = pm.KeyboardParameterBlock;			
			int row2Y = pmk.KeyBlockHeaderTextSize + 1;
			
			DrawText (ctx, font, pmk.KeyBlockParameterTextSize, pm.Pallette.CommonTextMiddle, ((int) Volume).ToString ("D3"), 130, 0);
			DrawText (ctx, font, pmk.KeyBlockParameterTextSize, pm.Pallette.CommonTextMiddle, ((int) Expression).ToString ("D3"), 190, 0);
			DrawText (ctx, font, pmk.KeyBlockParameterTextSize, pm.Pallette.CommonTextMiddle, ((int) Rsd).ToString ("D3"), 130, row2Y);
			DrawText (ctx, font, pmk.KeyBlockParameterTextSize, pm.Pallette.CommonTextMiddle, ((int) Csd).ToString ("D3"), 190, row2Y);
			DrawText (ctx, font, pmk.KeyBlockParameterTextSize, pm.Pallette.CommonTextMiddle, ((int) Dsd).ToString ("D3"), 250, row2Y);
			DrawBoolSwitch (ctx, font, (bool) Hold, "H", 300, 0);
			DrawBoolSwitch (ctx, font, (bool) PortamentoSwitch, "P", 300, row2Y);
			DrawText (ctx, font, pmk.KeyBlockParameterTextSize, pm.Pallette.CommonTextMiddle, ((int) Sostenuto).ToString ("D3"), 350, 0);
			DrawText (ctx, font, pmk.KeyBlockParameterTextSize, pm.Pallette.CommonTextMiddle, ((int) SoftPedal).ToString ("D3"), 350, row2Y);
		}

		public void ProcessMidiMessage (SmfEvent m)
		{
			switch (m.EventType) {
			case SmfEvent.CC:
				switch (m.Msb) {
				case SmfCC.Volume:
					Volume = m.Lsb;
					break;
				case SmfCC.Expression:
					Expression = m.Lsb;
					break;
				case SmfCC.Effect1:
					Rsd = m.Lsb;
					break;
				case SmfCC.Effect2:
					Csd = m.Lsb;
					break;
				case SmfCC.Effect3:
					Dsd = m.Lsb;
					break;
				case SmfCC.Hold:
					Hold = m.Lsb > 63;
					break;
				case SmfCC.PortamentoSwitch:
					PortamentoSwitch = m.Lsb > 63;
					break;
				case SmfCC.Sostenuto:
					Sostenuto = m.Lsb;
					break;
				case SmfCC.SoftPedal:
					SoftPedal = m.Lsb;
					break;
				}
				break;
			}
		}
	}
}

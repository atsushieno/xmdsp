using System;
using Xwt;
using Xwt.Drawing;
using Commons.Music.Midi;
using System.Collections.Generic;

namespace Xmdsp
{
	public class KeyboardParameterBlock
	{
		ViewModel vm;
		int channel;		
		Font font;
		
		public KeyboardParameterBlock (ViewModel viewModel, Font font, int channel)
		{
			vm = viewModel;
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
			var vmk = vm.KeyboardParameterBlock;
			
			int yOffset = this.channel * (vmk.Height + vm.KeyboardParameterBlock.Height);
			ctx.Translate (0, yOffset);
			
			int row2Y = vmk.KeyBlockHeaderTextSize + 1;
			
			var midiSize = DrawText (ctx, font, vmk.KeyBlockHeaderTextSize, vm.Pallette.CommonTextDarkest, "MIDI", 0, 0);
			
			var trackSize = DrawText (ctx, font, vmk.KeyBlockHeaderTextSize, vm.Pallette.CommonTextBlightest, "TRACK.", 0, row2Y);
			
			DrawText (ctx, font, vmk.KeyBlockChannelNumberTextSize, vm.Pallette.CommonTextMiddle, (channel + 1).ToString ("D2"), Math.Max (midiSize.Width, trackSize.Width), 0);
			
			var size = DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.CommonTextDarkest, "VOL:", 100, 0);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.CommonTextDarkest, "EXP:", 160, 0);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.CommonTextDarkest, "RSD:", 100, row2Y);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.CommonTextDarkest, "CSD:", 160, row2Y);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.CommonTextDarkest, "DSD:", 220, row2Y);
			
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.CommonTextDarkest, "So:", 320, 0);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.CommonTextDarkest, "SP:", 320, row2Y);
			
			UpdateParameters (ctx, font);
			
			ctx.Translate (0, -yOffset);
		}

		Size DrawText (Context ctx, Font font, int size, ViewModel.Color color, string text, double x, double y)
		{
			return DrawingHelper.DrawText (ctx, font, size, color, text, x, y);
		}

		void DrawBoolSwitch (Context ctx, Font font, bool value, string label, int x, int y)
		{
			var vmk = vm.KeyboardParameterBlock;
			ctx.SetColor ((value ? vm.Pallette.CommonTextMiddle : vm.Pallette.KeyParameterBackgroundColor).ToXwt ());
			ctx.Rectangle (x - 1, y, vmk.KeyBlockParameterTextSize + 2, vmk.KeyBlockParameterTextSize + 3);
			ctx.Fill ();
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, value ? vm.Pallette.CommonTextBlightest : vm.Pallette.CommonTextDarkest, label, x, y);
		}
		
		void UpdateParameters (Context ctx, Font font)
		{
			var vmk = vm.KeyboardParameterBlock;			
			int row2Y = vmk.KeyBlockHeaderTextSize + 1;
			
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.CommonTextMiddle, ((int) Volume).ToString ("D3"), 130, 0);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.CommonTextMiddle, ((int) Expression).ToString ("D3"), 190, 0);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.CommonTextMiddle, ((int) Rsd).ToString ("D3"), 130, row2Y);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.CommonTextMiddle, ((int) Csd).ToString ("D3"), 190, row2Y);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.CommonTextMiddle, ((int) Dsd).ToString ("D3"), 250, row2Y);
			DrawBoolSwitch (ctx, font, (bool) Hold, "H", 300, 0);
			DrawBoolSwitch (ctx, font, (bool) PortamentoSwitch, "P", 300, row2Y);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.CommonTextMiddle, ((int) Sostenuto).ToString ("D3"), 350, 0);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.CommonTextMiddle, ((int) SoftPedal).ToString ("D3"), 350, row2Y);
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

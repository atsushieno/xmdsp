using System;
using Gtk;
using Gdk;
using Commons.Music.Midi;

using FontFace = Pango.FontFace;

namespace Xmdsp
{
	public class KeyboardParameterBlock
	{
		ViewModel vm;
		int channel;
		FontFace font;
		
		public KeyboardParameterBlock (ViewModel viewModel, FontFace font, int channel)
		{
			vm = viewModel;
			this.font = font;
			
			this.channel = channel;
			
			Volume = Expression = Rsd = Csd = Dsd = SoftPedal = Sostenuto = 127;
			Hold = false;
			PortamentoSwitch = true;
		}
		
		public int? Volume;
		public int? Expression;
		public int? Rsd;
		public int? Csd;
		public int? Dsd;
		public bool? Hold;
		public bool? PortamentoSwitch;
		public int? SoftPedal;
		public int? Sostenuto;
		
		bool dirty = true;
		int platform_text_shift;
		
		internal void DoDraw (Cairo.Context ctx)
		{
//			if (!dirty)
//				return;
			
			platform_text_shift = vm.KeyboardParameterBlock.KeyBlockParameterTextSize;
		
			var vmk = vm.KeyboardParameterBlock;
			
			int yOffset = this.channel * (vmk.Height + vm.KeyboardParameterBlock.Height) + platform_text_shift;
			ctx.Translate (0, yOffset);
			
			int row2Y = vmk.KeyBlockHeaderTextSize + 1;
			
			var midiSize = DrawText (ctx, font, vmk.KeyBlockHeaderTextSize, vm.Pallette.KeyParameterTextDarkest, "MIDI", 0, 0);
			
			var trackSize = DrawText (ctx, font, vmk.KeyBlockHeaderTextSize, vm.Pallette.KeyParameterTextBlightest, "TRACK.", 0, row2Y);
			
			DrawText (ctx, font, vmk.KeyBlockChannelNumberTextSize, vm.Pallette.KeyParameterTextMiddle, (channel + 1).ToString ("D2"), Math.Max (midiSize.Width, trackSize.Width), 0);
			
			var size = DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextDarkest, "VOL:", 100, 0);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextDarkest, "EXP:", 160, 0);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextDarkest, "RSD:", 100, row2Y);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextDarkest, "CSD:", 160, row2Y);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextDarkest, "DSD:", 220, row2Y);
			
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextDarkest, "So:", 320, 0);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextDarkest, "SP:", 320, row2Y);
			
			UpdateParameters (ctx, font);
			
			ctx.Translate (0, -yOffset);
		}
		
		Cairo.TextExtents DrawText (Cairo.Context ctx, FontFace font, int size, ViewModel.Color color, string text, double x, double y)
		{
			Gdk.CairoHelper.SetSourceColor (ctx, color.ToGdk ());
			//font.FontExtents = new FontExtents (font.FontExtents.Ascent, font.FontExtents.Descent, size, font.FontExtents.MaxXAdvance, font.FontExtents.MaxYAdvance);
			//ctx.SetScaledFont (font);
			ctx.MoveTo (x, y);
			ctx.TextPath (text);
			ctx.Stroke ();
			return ctx.TextExtents (text);
		}
		
		void DrawBoolSwitch (Cairo.Context ctx, FontFace font, bool value, string label, int x, int y)
		{
			var vmk = vm.KeyboardParameterBlock;
			CairoHelper.SetSourceColor (ctx, (value ? vm.Pallette.KeyParameterTextMiddle : vm.Pallette.KeyParameterBackgroundColor).ToGdk ());
			ctx.Translate (0, -platform_text_shift);
			ctx.Rectangle (x - 1, y, vmk.KeyBlockParameterTextSize + 2, vmk.KeyBlockParameterTextSize + 3);
			ctx.Translate (0, platform_text_shift);
			ctx.Fill ();
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, value ? vm.Pallette.KeyParameterTextBlightest : vm.Pallette.KeyParameterTextDarkest, label, x, y);
		}
		
		internal void UpdateParameters (Cairo.Context ctx, FontFace font)
		{
			var vmk = vm.KeyboardParameterBlock;			
			int row2Y = vmk.KeyBlockHeaderTextSize + 1;

			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextMiddle, ((int) Volume).ToString ("D3"), 130, 0);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextMiddle, ((int) Expression).ToString ("D3"), 190, 0);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextMiddle, ((int) Rsd).ToString ("D3"), 130, row2Y);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextMiddle, ((int) Csd).ToString ("D3"), 190, row2Y);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextMiddle, ((int) Dsd).ToString ("D3"), 250, row2Y);
			DrawBoolSwitch (ctx, font, (bool) Hold, "H", 300, 0);
			DrawBoolSwitch (ctx, font, (bool) PortamentoSwitch, "P", 300, row2Y);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextMiddle, ((int) Sostenuto).ToString ("D3"), 350, 0);
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextMiddle, ((int) SoftPedal).ToString ("D3"), 350, row2Y);
			/*
			if (Volume != null) {
				DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextMiddle, ((int) Volume).ToString ("D3"), 130, 0);
				Volume = null;
			}
			if (Expression != null) {
				DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextMiddle, ((int) Expression).ToString ("D3"), 190, 0);
				Expression = null;
			}
			if (Rsd != null) {
				DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextMiddle, ((int) Rsd).ToString ("D3"), 130, row2Y);
				Rsd = null;
			}
			if (Csd != null) {
				DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextMiddle, ((int) Csd).ToString ("D3"), 190, row2Y);
				Csd = null;
			}
			if (Dsd != null) {
				DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextMiddle, ((int) Dsd).ToString ("D3"), 250, row2Y);
				Dsd = null;
			}
			if (Hold != null) {
				DrawBoolSwitch (ctx, font, (bool) Hold, "H", 300, 0);
				Hold = null;
			}
			if (PortamentoSwitch != null) {
				DrawBoolSwitch (ctx, font, (bool) PortamentoSwitch, "P", 300, row2Y);
				PortamentoSwitch = null;
			}
			if (Sostenuto != null) {
				DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextMiddle, ((int) Sostenuto).ToString ("D3"), 350, 0);
				Sostenuto = null;
			}
			if (SoftPedal != null) {
				DrawText (ctx, font, vmk.KeyBlockParameterTextSize, vm.Pallette.KeyParameterTextMiddle, ((int) SoftPedal).ToString ("D3"), 350, row2Y);
				SoftPedal = null;
			}
			*/
		}
		
		public void ProcessMidiMessage (SmfMessage m)
		{
			switch (m.MessageType) {
			case SmfMessage.CC:
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
			dirty = true;
		}
	}
}

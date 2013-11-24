using System;
using Xwt;
using Xwt.Drawing;
using Commons.Music.Midi;

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
		
		internal void DoDraw (Context ctx)
		{
			if (!dirty)
				return;
			
			var vmk = vm.KeyboardParameterBlock;
			
			int yOffset = this.channel * (vmk.Height + vm.KeyboardParameterBlock.Height);
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
		
		Size DrawText (Context ctx, Font font, int size, ViewModel.Color color, string text, double x, double y)
		{
			ctx.SetColor (color.ToXwt ());
			font = font.WithSize (size);
			var textLayout = new TextLayout () { Font = font, Text = text, };
			var numberSize = textLayout.GetSize ();
			ctx.DrawTextLayout (textLayout, x, y);
			ctx.Stroke ();
			return textLayout.GetSize ();
		}
		
		void DrawBoolSwitch (Context ctx, Font font, bool value, string label, int x, int y)
		{
			var vmk = vm.KeyboardParameterBlock;
			ctx.SetColor ((value ? vm.Pallette.KeyParameterTextMiddle : vm.Pallette.KeyParameterBackgroundColor).ToXwt ());
			ctx.Rectangle (x - 1, y, vmk.KeyBlockParameterTextSize + 2, vmk.KeyBlockParameterTextSize + 3);
			ctx.Fill ();
			DrawText (ctx, font, vmk.KeyBlockParameterTextSize, value ? vm.Pallette.KeyParameterTextBlightest : vm.Pallette.KeyParameterTextDarkest, label, x, y);
		}
		
		internal void UpdateParameters (Context ctx, Font font)
		{
			var vmk = vm.KeyboardParameterBlock;			
			int row2Y = vmk.KeyBlockHeaderTextSize + 1;
			
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

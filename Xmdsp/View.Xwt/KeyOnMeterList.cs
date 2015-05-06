using System;
using Xwt;
using System.Collections.Generic;
using Xwt.Drawing;
using Commons.Music.Midi;

namespace Xmdsp
{
	public class KeyOnMeterList : Canvas
	{
		ViewModel vm;
		int [] keyon_meter_progress;
		int current_progress;
		int [] program;
		int [] bank_select;
		Font font;
		
		const int progress_max = 16;
		
		public KeyOnMeterList (ViewModel vm)
		{
			this.vm = vm;
			font = this.Font.WithSize (vm.KeyOnMeterList.KeyOnMeterTextSize);
			keyon_meter_progress = new int [vm.MaxChannels];
			program = new int [vm.MaxChannels];
			bank_select = new int [vm.MaxChannels];

			for (int i = 0; i < vm.MaxChannels; i++)
				keyon_meter_progress [i] = progress_max;
			
			vm.Model.MidiMessageReceived += m => {
				switch (m.EventType) {
				case SmfEvent.NoteOn:
					if (m.Lsb > 0)
						keyon_meter_progress [m.Channel] = current_progress = 0;
					break;
				case SmfEvent.Program:
					program [m.Channel] = m.Msb;
					break;
				case SmfEvent.CC:
					switch (m.Msb) {
						case SmfCC.BankSelectLsb:
							bank_select [m.Channel] = m.Lsb;
						break;
					}
					break;
				}
			};

			WidthRequest = vm.KeyOnMeterList.Width;
			HeightRequest = vm.KeyOnMeterList.Height;

			vm.Model.TickProgress += delegate {
				if (!dirty && current_progress++ < progress_max) {
					dirty = true;
					QueueDraw ();
				}
			};
		}

		Size DrawText (Context ctx, Font font, int size, ViewModel.Color color, string text, double x, double y)
		{
			return DrawingHelper.DrawText (ctx, font, size, color, text, x, y);
		}
		
		bool dirty = true;

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			if (!dirty)
				return;
			var vmk = vm.KeyOnMeterList;
			DrawText (ctx, font, vmk.KeyOnMeterTextSize, vm.Pallette.CommonTextMiddle, "PRG", 0, vmk.MeterHeight);
			DrawText (ctx, font, vmk.KeyOnMeterTextSize, vm.Pallette.CommonTextMiddle, "BNK", 0, vmk.MeterHeight + vmk.KeyOnMeterTextSize + vmk.LineGapSize);

			dirty = false;
			ctx.SetColor (vm.Pallette.CommonTextMiddle.ToXwt ());
			ctx.SetLineWidth (1);
			var lineHeight = vmk.MeterHeight / progress_max;
			for (int i = 0; i < keyon_meter_progress.Length; i++) {
				var x = (vmk.ItemWidth) * i + vmk.MetersOffset;
				ctx.Rectangle (x, 0, vmk.MeterWidth, vmk.MeterHeight);
				for (int p = keyon_meter_progress [i]; p < progress_max; p++) {
					var y = p * lineHeight;
					ctx.MoveTo (x, y);
					ctx.LineTo (x + vmk.MeterWidth, y);
				}
				var tx = x;
				var ty = vmk.MeterHeight;
				DrawText (ctx, font, vmk.KeyOnMeterTextSize, vm.Pallette.CommonTextMiddle, program [i].ToString ("D03"), tx, ty);
				DrawText (ctx, font, vmk.KeyOnMeterTextSize, vm.Pallette.CommonTextMiddle, bank_select [i].ToString ("D03"), tx, ty + vmk.KeyOnMeterTextSize + vmk.LineGapSize);
			}
			ctx.Stroke ();

			for (int i = 0; i < keyon_meter_progress.Length; i++)
				if (keyon_meter_progress [i] < progress_max)
					keyon_meter_progress [i]++;
		}
	}
}


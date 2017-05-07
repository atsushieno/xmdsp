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
		int [] pan;
		Font font;
		int prog_bnk_offset_y;
		int panpot_offset_y;
		int prog_bnk_height;
		int panpot_height;
		
		const int progress_max = 16;
		
		public KeyOnMeterList (ViewModel vm)
		{
			this.vm = vm;
			var vmk = vm.KeyOnMeterList;
			font = this.Font.WithSize (vm.KeyOnMeterList.KeyOnMeterTextSize);
			keyon_meter_progress = new int [vm.MaxChannels];
			program = new int [vm.MaxChannels];
			bank_select = new int [vm.MaxChannels];
			pan = new int [vm.MaxChannels];

			for (int i = 0; i < vm.MaxChannels; i++) {
				keyon_meter_progress [i] = progress_max;
				pan [i] = 64;
			}
			
			prog_bnk_offset_y = vmk.MeterHeight;
			prog_bnk_height = (vmk.KeyOnMeterTextSize + vmk.LineGapSize) * 2;
			panpot_offset_y = vmk.MeterHeight + (vmk.KeyOnMeterTextSize + vmk.LineGapSize) * 2;
			panpot_height = vmk.PanpotOuterRadius * 2;

			vm.ScaleChanged += SetSize;

			vm.Model.MidiMessageReceived += m => {
				switch (m.EventType) {
				case SmfEvent.NoteOn:
					if (m.Lsb > 0)
						keyon_meter_progress [m.Channel] = current_progress = 0;
					break;
				case SmfEvent.Program:
					program [m.Channel] = m.Msb;
					dirty_prog = true;
					Application.Invoke (() => QueueDraw (new Rectangle (0, prog_bnk_offset_y, vmk.Width, prog_bnk_height)));
					break;
				case SmfEvent.CC:
					switch (m.Msb) {
					case SmfCC.Pan:
						pan [m.Channel] = m.Lsb;
						dirty_pan = true;
						Application.Invoke (() => QueueDraw (new Rectangle (0, panpot_offset_y, vmk.Width, panpot_height)));
						break;
					case SmfCC.BankSelectLsb:
						bank_select [m.Channel] = m.Lsb;
						dirty_prog = true;
						Application.Invoke (() => QueueDraw (new Rectangle (0, prog_bnk_offset_y, vmk.Width, prog_bnk_height)));
						break;
					}
					break;
				}
			};

			vm.Model.TickProgress += delegate {
				if (!dirty_keyon && current_progress++ < progress_max) {
					dirty_keyon = true;
					// FIXME: enable this once I figured out why the other QueueDraw() overload doesn't work.
//					QueueDraw (new Rectangle (0, 0, vmk.Width, vmk.MeterHeight));
					Application.Invoke (() => QueueDraw ());
				}
			};
		}

		internal void SetSize ()
		{
			WidthRequest = vm.KeyOnMeterList.Width * vm.Scale;
			HeightRequest = vm.KeyOnMeterList.Height * vm.Scale;

		}

		Size DrawText (Context ctx, Font font, int size, ViewModel.Color color, string text, double x, double y)
		{
			return DrawingHelper.DrawText (ctx, font, size, color, text, x, y);
		}
		
		bool dirty_keyon = true, dirty_prog = true, dirty_pan = true;

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			ctx.Scale (vm.Scale, vm.Scale);
			//if (dirty_keyon)
				DrawKeyOn (ctx);
			//if (dirty_prog)
				DrawProgramBank (ctx);
			//if (dirty_pan)
				DrawPanpot (ctx);
		}

		void DrawProgramBank (Context ctx)
		{
			dirty_prog = false;
			var vmk = vm.KeyOnMeterList;

			DrawText (ctx, font, vmk.KeyOnMeterTextSize, vm.Pallette.CommonTextMiddle, "PRG", 0, vmk.MeterHeight);
			DrawText (ctx, font, vmk.KeyOnMeterTextSize, vm.Pallette.CommonTextMiddle, "BNK", 0, vmk.MeterHeight + vmk.KeyOnMeterTextSize + vmk.LineGapSize);
			for (int i = 0; i < keyon_meter_progress.Length; i++) {
				var tx = (vmk.ItemWidth) * i + vmk.MetersOffset;
				var ty = prog_bnk_offset_y;
				DrawText (ctx, font, vmk.KeyOnMeterTextSize, vm.Pallette.CommonTextMiddle, program [i].ToString ("D03"), tx, ty);
				DrawText (ctx, font, vmk.KeyOnMeterTextSize, vm.Pallette.CommonTextMiddle, bank_select [i].ToString ("D03"), tx, ty + vmk.KeyOnMeterTextSize + vmk.LineGapSize);
			}
		}

		void DrawPanpot (Context ctx)
		{
			dirty_pan = false;
			var vmk = vm.KeyOnMeterList;

			var y = panpot_offset_y;
			DrawText (ctx, font, vmk.KeyOnMeterTextSize, vm.Pallette.CommonTextMiddle, "PAN", 0, y);

			ctx.SetColor (vm.Pallette.CommonTextMiddle.ToXwt ());
			for (int i = 0; i < keyon_meter_progress.Length; i++) {
				var x = (vmk.ItemWidth) * i + vmk.MetersOffset;
				var h = vmk.ItemWidth / 2 - vmk.LineGapSize;
				ctx.NewPath ();
				var cx = x + h;
				var cy = y + h + vmk.PanpotOffset;
				ctx.Arc (cx, cy, h, 0, 360);
				ctx.Stroke ();
				var panAngle = ((64 - pan [i]) / 90.0) * 45 + 90;
				ctx.MoveTo (cx, cy);
				ctx.Arc (cx, cy, h * 0.7, panAngle + 150, panAngle - 150);
				ctx.ClosePath ();
				ctx.Fill ();
			}
		}

		void DrawKeyOn (Context ctx)
		{
			dirty_keyon = false;
			var vmk = vm.KeyOnMeterList;

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
			}
			ctx.Stroke ();

			for (int i = 0; i < keyon_meter_progress.Length; i++)
				if (keyon_meter_progress [i] < progress_max)
					keyon_meter_progress [i]++;
		}
	}
}


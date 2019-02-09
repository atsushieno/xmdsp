using System;
using Xwt;
using System.Collections.Generic;
using System.Linq;
using Xwt.Drawing;
using Commons.Music.Midi;

namespace Xmdsp
{
	public class KeyOnMeterList : Canvas
	{
		Presenter pm;
		int [] keyon_meter_progress;
		int current_progress;
		int [] program;
		int [] bank_select;
		int [] pan;
		bool [] masked;
		Rectangle [] mask_rectangles;
		Font font;
		int prog_bnk_offset_y;
		int panpot_offset_y;
		int mask_offset_y;
		int prog_bnk_height;
		int panpot_height;
		
		public KeyOnMeterList (Presenter pm)
		{
			this.pm = pm;
			var pmk = pm.KeyOnMeterList;
			font = this.Font.WithSize (pm.KeyOnMeterList.KeyOnMeterTextSize);
			keyon_meter_progress = new int [pm.MaxChannels];
			program = new int [pm.MaxChannels];
			bank_select = new int [pm.MaxChannels];
			pan = new int [pm.MaxChannels];
			masked = new bool [pm.MaxChannels];

			for (int i = 0; i < pm.MaxChannels; i++) {
				keyon_meter_progress [i] = pmk.TotalProgressSteps;
				pan [i] = 64;
			}
			
			prog_bnk_offset_y = pmk.MeterHeight;
			prog_bnk_height = (pmk.KeyOnMeterTextSize + pmk.LineGapSize) * 2;
			panpot_offset_y = pmk.MeterHeight + (pmk.KeyOnMeterTextSize + pmk.LineGapSize) * 2;
			panpot_height = pmk.PanpotOuterRadius * 2;
			mask_offset_y = panpot_offset_y + panpot_height + pmk.LineGapSize * 2 + pmk.MaskOffsetY;
			mask_rectangles = new Rectangle [pm.MaxChannels];
			for (int i = 0; i < pm.MaxChannels; i++) {
				var x = (pmk.ItemWidth) * i + pmk.MetersOffset;
				mask_rectangles [i] = new Rectangle (x, mask_offset_y, pmk.MaskWidth, pmk.MaskWidth);
			}

			pm.ScaleChanged += SetSize;

			pm.MidiMessageReceived += m => {
				switch (m.EventType) {
				case MidiEvent.NoteOn:
					if (m.Lsb > 0)
						keyon_meter_progress [m.Channel] = current_progress = (127 - m.Lsb) / (128 / pmk.TotalProgressSteps);
					break;
				case MidiEvent.Program:
					program [m.Channel] = m.Msb;
					dirty_prog = true;
					Application.Invoke (() => QueueDraw (new Rectangle (0, prog_bnk_offset_y, pmk.Width, prog_bnk_height)));
					break;
				case MidiEvent.CC:
					switch (m.Msb) {
					case MidiCC.Pan:
						pan [m.Channel] = m.Lsb;
						dirty_pan = true;
						Application.Invoke (() => QueueDraw (new Rectangle (0, panpot_offset_y, pmk.Width, panpot_height)));
						break;
					case MidiCC.BankSelectLsb:
						bank_select [m.Channel] = m.Lsb;
						dirty_prog = true;
						Application.Invoke (() => QueueDraw (new Rectangle (0, prog_bnk_offset_y, pmk.Width, prog_bnk_height)));
						break;
					}
					break;
				}
			};

			pm.Model.TickProgress += delegate {
				if (!dirty_keyon && current_progress++ < pmk.TotalProgressSteps) {
					dirty_keyon = true;
					// FIXME: enable this once I figured out why the other QueueDraw() overload doesn't work.
					//Application.Invoke (() => QueueDraw (new Rectangle (0, 0, pmk.Width, pmk.Height)));
					Application.Invoke (QueueDraw);
				}
			};
			
			this.ButtonReleased += (object sender, ButtonEventArgs e) => {
				for (int i = 0; i < mask_rectangles.Length; i++) {
					var maskRect = new Rectangle (
						GetScaledPosition (mask_rectangles [i].TopLeft),
						GetScaledPosition (mask_rectangles [i].BottomRight));
					if (maskRect.Contains (e.Position)) {
						// left click = simple toggle
						if (e.Button == PointerButton.Left)
							masked [i] = !masked [i];
						// right click = toggle between solo and reset
						else if (e.Button == PointerButton.Right) {
							var currentHasAnyMutedChannel = masked.Any (b => b);
							for (int x = 0; x < masked.Length; x++)
								masked [x] = currentHasAnyMutedChannel ? false : i != x;
						}

						this.pm.SetChannelMasks (masked);
						break;
					}
				}
			};
		}

		Point GetScaledPosition (Point point)
		{
			return new Point (point.X * pm.Scale, point.Y * pm.Scale); 
		}

		internal void SetSize ()
		{
			WidthRequest = pm.KeyOnMeterList.Width * pm.Scale;
			HeightRequest = pm.KeyOnMeterList.Height * pm.Scale;

		}

		Size DrawText (Context ctx, Font font, int size, Presenter.Color color, string text, double x, double y)
		{
			return DrawingHelper.DrawText (ctx, font, size, color, text, x, y);
		}
		
		bool dirty_keyon = true, dirty_prog = true, dirty_pan = true, dirty_mask = true;

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			ctx.Scale (pm.Scale, pm.Scale);
			//if (dirty_keyon)
				DrawKeyOn (ctx);
			//if (dirty_prog)
				DrawProgramBank (ctx);
			//if (dirty_pan)
				DrawPanpot (ctx);
			//if (dirty_mask)
				DrawChannelMask (ctx);
		}

		void DrawProgramBank (Context ctx)
		{
			dirty_prog = false;
			var pmk = pm.KeyOnMeterList;

			DrawText (ctx, font, pmk.KeyOnMeterTextSize, pm.Pallette.CommonTextMiddle, "PROG", 0, pmk.MeterHeight);
			DrawText (ctx, font, pmk.KeyOnMeterTextSize, pm.Pallette.CommonTextMiddle, "BANK", 0, pmk.MeterHeight + pmk.KeyOnMeterTextSize + pmk.LineGapSize);
			for (int i = 0; i < keyon_meter_progress.Length; i++) {
				var tx = (pmk.ItemWidth) * i + pmk.MetersOffset;
				var ty = prog_bnk_offset_y;
				DrawText (ctx, font, pmk.KeyOnMeterTextSize, pm.Pallette.CommonTextMiddle, program [i].ToString ("D03"), tx, ty);
				DrawText (ctx, font, pmk.KeyOnMeterTextSize, pm.Pallette.CommonTextMiddle, bank_select [i].ToString ("D03"), tx, ty + pmk.KeyOnMeterTextSize + pmk.LineGapSize);
			}
		}

		void DrawPanpot (Context ctx)
		{
			dirty_pan = false;
			var pmk = pm.KeyOnMeterList;

			var y = panpot_offset_y;
			DrawText (ctx, font, pmk.KeyOnMeterTextSize, pm.Pallette.CommonTextMiddle, "PAN", 0, y);

			ctx.SetColor (pm.Pallette.CommonTextMiddle.ToXwt ());
			for (int i = 0; i < keyon_meter_progress.Length; i++) {
				var x = (pmk.ItemWidth) * i + pmk.MetersOffset;
				var h = pmk.ItemWidth / 2 - pmk.LineGapSize;
				ctx.NewPath ();
				var cx = x + h;
				var cy = y + h + pmk.PanpotOffset;
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
			var pmk = pm.KeyOnMeterList;

			ctx.SetColor (pm.Pallette.CommonTextMiddle.ToXwt ());
			ctx.SetLineWidth (1);
			var steps = pmk.TotalProgressSteps;
			var lineHeight = pmk.MeterHeight / steps;
			for (int i = 0; i < keyon_meter_progress.Length; i++) {
				var x = (pmk.ItemWidth) * i + pmk.MetersOffset;
				ctx.Rectangle (x, 0, pmk.MeterWidth, pmk.MeterHeight);
				for (int p = keyon_meter_progress [i]; p < steps; p++) {
					var y = p * lineHeight;
					ctx.MoveTo (x, y);
					ctx.LineTo (x + pmk.MeterWidth, y);
				}
			}
			ctx.Stroke ();

			for (int i = 0; i < keyon_meter_progress.Length; i++)
				if (keyon_meter_progress [i] < steps)
					keyon_meter_progress [i]++;
		}

		void DrawChannelMask (Context ctx)
		{
			dirty_mask = false;
			var pmk = pm.KeyOnMeterList;

			ctx.SetLineWidth (1);

			DrawText (ctx, font, pmk.KeyOnMeterTextSize, pm.Pallette.CommonTextMiddle, "MASK", 0, mask_offset_y);
			for (int i = 0; i < keyon_meter_progress.Length; i++) {
				var color = masked [i]
					? pm.Pallette.CommonTextDarkest.ToXwt ()
					: pm.Pallette.CommonTextMiddle.ToXwt ();
				ctx.SetColor (color);
				ctx.Rectangle (mask_rectangles [i]);
				ctx.Fill ();
			}
		}
	}
}


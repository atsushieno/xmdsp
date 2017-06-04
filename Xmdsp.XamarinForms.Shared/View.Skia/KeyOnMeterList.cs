using Commons.Music.Midi;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace Xmdsp
{
	public class KeyOnMeterList : SKCanvasView
	{
		Presenter pm;
		int [] keyon_meter_progress;
		int current_progress;
		int [] program;
		int [] bank_select;
		int [] pan;
		int prog_bnk_offset_y;
		int panpot_offset_y;
		int prog_bnk_height;
		int panpot_height;
		SKPaint paint;
		
		public KeyOnMeterList (Presenter pm)
		{
			this.pm = pm;
			var pmk = pm.KeyOnMeterList;
			paint = new SKPaint () { TextSize = pm.KeyOnMeterList.KeyOnMeterTextSize };
			keyon_meter_progress = new int [pm.MaxChannels];
			program = new int [pm.MaxChannels];
			bank_select = new int [pm.MaxChannels];
			pan = new int [pm.MaxChannels];

			for (int i = 0; i < pm.MaxChannels; i++) {
				keyon_meter_progress [i] = pmk.TotalProgressSteps;
				pan [i] = 64;
			}
			
			prog_bnk_offset_y = pmk.MeterHeight;
			prog_bnk_height = (pmk.KeyOnMeterTextSize + pmk.LineGapSize) * 2;
			panpot_offset_y = pmk.MeterHeight + (pmk.KeyOnMeterTextSize + pmk.LineGapSize) * 2;
			panpot_height = pmk.PanpotOuterRadius * 2;

			pm.ScaleChanged += SetSize;

			pm.Model.MidiMessageReceived += m => {
				switch (m.EventType) {
				case MidiEvent.NoteOn:
					if (m.Lsb > 0)
						keyon_meter_progress [m.Channel] = current_progress = 0;
					break;
				case MidiEvent.Program:
					program [m.Channel] = m.Msb;
					dirty_prog = true;
					//QueueDraw (new SKRect (0, prog_bnk_offset_y, pmk.Width, prog_bnk_height));
					break;
				case MidiEvent.CC:
					switch (m.Msb) {
					case MidiCC.Pan:
						pan [m.Channel] = m.Lsb;
						dirty_pan = true;
						//QueueDraw (new SKRect (0, panpot_offset_y, pmk.Width, panpot_height));
						break;
					case MidiCC.BankSelectLsb:
						bank_select [m.Channel] = m.Lsb;
						dirty_prog = true;
						//QueueDraw (new SKRect (0, prog_bnk_offset_y, pmk.Width, prog_bnk_height));
						break;
					}
					break;
				}
			};

			pm.Model.TickProgress += delegate {
				if (!dirty_keyon && current_progress++ < pmk.TotalProgressSteps) {
					dirty_keyon = true;
					// FIXME: enable this once I figured out why the other QueueDraw() overload doesn't work.
//					QueueDraw (new Rectangle (0, 0, pmk.Width, pmk.MeterHeight));
					//QueueDraw ();
				}
			};
		}

		internal void SetSize ()
		{
			WidthRequest = pm.KeyOnMeterList.Width * pm.Scale;
			HeightRequest = pm.KeyOnMeterList.Height * pm.Scale;

		}

		void DrawText (SKCanvas ctx, SKPaint font, int size, Presenter.Color color, string text, double x, double y)
		{
			DrawingHelper.DrawText (ctx, font, size, color, text, x, y);
		}
		
		bool dirty_keyon = true, dirty_prog = true, dirty_pan = true;

		protected override void OnPaintSurface (SKPaintSurfaceEventArgs e)
		{
			base.OnPaintSurface (e);
			var ctx = e.Surface.Canvas;
			var dirtyRect = e.Info.Rect;

			ctx.Scale ((float)pm.Scale, (float)pm.Scale);
			//if (dirty_keyon)
				DrawKeyOn (ctx);
			//if (dirty_prog)
				DrawProgramBank (ctx);
			//if (dirty_pan)
				DrawPanpot (ctx);
		}

		void DrawProgramBank (SKCanvas ctx)
		{
			dirty_prog = false;
			var pmk = pm.KeyOnMeterList;

			DrawText (ctx, paint, pmk.KeyOnMeterTextSize, pm.Pallette.CommonTextMiddle, "PRG", 0, pmk.MeterHeight);
			DrawText (ctx, paint, pmk.KeyOnMeterTextSize, pm.Pallette.CommonTextMiddle, "BNK", 0, pmk.MeterHeight + pmk.KeyOnMeterTextSize + pmk.LineGapSize);
			for (int i = 0; i < keyon_meter_progress.Length; i++) {
				var tx = (pmk.ItemWidth) * i + pmk.MetersOffset;
				var ty = prog_bnk_offset_y;
				DrawText (ctx, paint, pmk.KeyOnMeterTextSize, pm.Pallette.CommonTextMiddle, program [i].ToString ("D03"), tx, ty);
				DrawText (ctx, paint, pmk.KeyOnMeterTextSize, pm.Pallette.CommonTextMiddle, bank_select [i].ToString ("D03"), tx, ty + pmk.KeyOnMeterTextSize + pmk.LineGapSize);
			}
		}

		void DrawPanpot (SKCanvas ctx)
		{
			dirty_pan = false;
			var pmk = pm.KeyOnMeterList;

			var y = panpot_offset_y;
			DrawText (ctx, paint, pmk.KeyOnMeterTextSize, pm.Pallette.CommonTextMiddle, "PAN", 0, y);

			paint.Color = (pm.Pallette.CommonTextMiddle.ToSkia ());
			for (int i = 0; i < keyon_meter_progress.Length; i++) {
				var x = (pmk.ItemWidth) * i + pmk.MetersOffset;
				var h = pmk.ItemWidth / 2 - pmk.LineGapSize;
				var path = new SKPath ();
				var cx = x + h;
				var cy = y + h + pmk.PanpotOffset;
				path.AddArc (new SKRect (cx, cy, h, h), 0, 360);
				ctx.DrawPath (path, paint);
				var panAngle = ((64 - pan [i]) / 90.0) * 45 + 90;
				path = new SKPath ();
				path.AddArc (new SKRect (cx, cy, (float) (h * 0.7), (float) (h * 0.7)), (float)(panAngle + 150), (float)(panAngle - 150));
				paint.IsStroke = false;
				ctx.DrawPath (path, paint);
			}
		}

		void DrawKeyOn (SKCanvas ctx)
		{
			dirty_keyon = false;
			var pmk = pm.KeyOnMeterList;

			paint.Color = pm.Pallette.CommonTextMiddle.ToSkia ();
			paint.StrokeWidth = 1;
			paint.IsStroke = true;
			var steps = pmk.TotalProgressSteps;
			var lineHeight = pmk.MeterHeight / steps;
			for (int i = 0; i < keyon_meter_progress.Length; i++) {
				var x = (pmk.ItemWidth) * i + pmk.MetersOffset;
				ctx.DrawRect (new SKRect (x, 0, pmk.MeterWidth, pmk.MeterHeight), paint);
				for (int p = keyon_meter_progress [i]; p < steps; p++) {
					var y = p * lineHeight;
					ctx.DrawLine (x, y, x + pmk.MeterWidth, y, paint);
				}
			}

			for (int i = 0; i < keyon_meter_progress.Length; i++)
				if (keyon_meter_progress [i] < steps)
					keyon_meter_progress [i]++;
		}
	}
}


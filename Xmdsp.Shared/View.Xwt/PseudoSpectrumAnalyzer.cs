using System;
using Xwt;
using Xwt.Drawing;

namespace Xmdsp
{
	/*
	public class PseudoSpectrumAnalyzer : Canvas
	{
		ViewModel vm;
		int current_progress;
		bool dirty_keyon;

		public PseudoSpectrumAnalyzer (ViewModel vm)
		{
			this.vm = vm;
			vm.Model.TickProgress += delegate {
				if (!dirty_keyon && current_progress++ < vm.PseudoSpectrumAnalyzer.TotalProgressSteps) {
					dirty_keyon = true;
// FIXME: enable this once I figured out why the other QueueDraw() overload doesn't work.
//					QueueDraw (new Rectangle (0, 0, vmk.Width, vmk.MeterHeight));
					Application.Invoke (() => QueueDraw ());
				}
			};
		}

		void DrawKeyOn (Context ctx)
		{
			dirty_keyon = false;
			var vmp = vm.PseudoSpectrumAnalyzer;



			ctx.SetColor (vm.Pallette.CommonTextMiddle.ToXwt ());
			ctx.SetLineWidth (vmp.LineThickness);
			var steps = vmp.TotalProgressSteps;
			var lineHeight = vmp.MeterHeight / steps;
			for (int i = 0; i < keyon_meter_progress.Length; i++) {
				var x = (vmp.LineSpacing) * i + vmp.MetersOffset;
				ctx.Rectangle (x, 0, vmp.MeterWidth, vmp.MeterHeight);
				for (int p = keyon_meter_progress [i]; p < steps; p++) {
					var y = p * lineHeight;
					ctx.MoveTo (x, y);
					ctx.LineTo (x + vmp.MeterWidth, y);
				}
			}
			ctx.Stroke ();

			for (int i = 0; i < keyon_meter_progress.Length; i++)
				if (keyon_meter_progress [i] < steps)
					keyon_meter_progress [i]++;
		}
	}
	*/
}

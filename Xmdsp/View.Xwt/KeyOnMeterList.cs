using System;
using Gtk;
using Gdk;
using System.Collections.Generic;
using Commons.Music.Midi;

namespace Xmdsp
{
	public class KeyOnMeterList : DrawingArea
	{
		ViewModel vm;
		int [] keyon_meter_progress;		
		int current_progress;
		
		const int progress_max = 16;
		
		public KeyOnMeterList (ViewModel vm)
		{
			this.vm = vm;
			keyon_meter_progress = new int [vm.MaxChannels];
			
			for (int i = 0; i < vm.MaxChannels; i++)
				keyon_meter_progress [i] = progress_max;
			
			vm.Model.MidiMessageReceived += m => {
				if (m.EventType == SmfEvent.NoteOn && m.Lsb > 0)
					keyon_meter_progress [m.Channel] = current_progress = 0;
			};
			vm.Model.TickProgress += delegate {
				if (current_progress++ < progress_max)
					QueueDraw ();
			};
			
			WidthRequest = vm.KeyOnMeterList.Width;
			HeightRequest = vm.KeyOnMeterList.Height;
		}
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			var ctx = CairoHelper.Create (GdkWindow);
			var vmk = vm.KeyOnMeterList;
			CairoHelper.SetSourceColor (ctx, vm.Pallette.CommonTextDarkest.ToGdk ());
			ctx.LineWidth = 1;
			var lineHeight = vmk.MeterHeight / progress_max;
			for (int i = 0; i < keyon_meter_progress.Length; i++) {
				var x = (vmk.ItemWidth) * i;
				ctx.Rectangle (x, 0, vmk.MeterWidth, vmk.MeterHeight);
				for (int p = keyon_meter_progress [i]; p < progress_max; p++) {
					var y = p * lineHeight + (lineHeight / 2);
					ctx.MoveTo (x, y);
					ctx.LineTo (x + vmk.MeterWidth, y);
				}
			}
			ctx.Stroke ();

			for (int i = 0; i < keyon_meter_progress.Length; i++)
				if (keyon_meter_progress [i] < progress_max)
					keyon_meter_progress [i]++;
			
			ctx.Dispose ();
			return true;
		}
	}
}


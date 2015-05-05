﻿using System;
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

			WidthRequest = vm.KeyOnMeterList.Width;
			HeightRequest = vm.KeyOnMeterList.Height;

			vm.Model.TickProgress += delegate {
				if (!dirty && current_progress++ < progress_max) {
					dirty = true;
					QueueDraw ();
				}
			};
		}
		
		bool dirty = true;

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			if (!dirty)
				return;
			dirty = false;
			var vmk = vm.KeyOnMeterList;
			ctx.SetColor (vm.Pallette.CommonTextDarkest.ToXwt ());
			ctx.SetLineWidth (1);
			var lineHeight = vmk.MeterHeight / progress_max;
			for (int i = 0; i < keyon_meter_progress.Length; i++) {
				var x = (vmk.ItemWidth) * i;
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

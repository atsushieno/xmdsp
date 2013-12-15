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
		List<KeyOnMeter> keyon_meters = new List<KeyOnMeter> ();
		
		public KeyOnMeterList (ViewModel vm)
		{
			this.vm = vm;
			
			for (int i = 0; i < vm.MaxChannels; i++)
				keyon_meters.Add (new KeyOnMeter (vm, i));
			
			vm.Model.MidiMessageReceived += m => {
				switch (m.EventType) {
				case SmfEvent.NoteOn:
					keyon_meters [m.Channel].KeyOn (m);
					break;
				}
			};
			
			WidthRequest = vm.KeyOnMeterList.Width;
			HeightRequest = vm.KeyOnMeterList.Height;
		}
		
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);
			
			foreach (var k in keyon_meters)
				k.DoDraw (ctx);
		}
	}
}


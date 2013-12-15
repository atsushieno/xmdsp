using System;
using Xwt;
using System.Collections.Generic;
using Xwt.Drawing;
using Commons.Music.Midi;

namespace Xmdsp
{
	class KeyOnMeter
	{
		ViewModel vm;
		int index;
		DateTime current_keyon = DateTime.MinValue;
		
		public KeyOnMeter (ViewModel vm, int index)
		{
			this.vm = vm;
			this.index = index;
		}

		public void KeyOn (SmfEvent m)
		{
			current_keyon = DateTime.Now;
		}
		
		public void DoDraw (Context ctx)
		{
			var vmk = vm.KeyOnMeter;
			ctx.SetColor (vm.Pallette.CommonTextDarkest.ToXwt ());
			ctx.SetLineWidth (1);
			ctx.Rectangle ((vmk.Width) * index, 0, vmk.MeterWidth, vmk.MeterHeight);
			ctx.Stroke ();
		}
	}
}


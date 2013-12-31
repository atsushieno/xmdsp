using System;
using Gtk;
using Commons.Music.Midi;
using Gdk;
using System.Threading;
using Commons.Music.Midi.Player;

namespace Xmdsp
{
	public class KeyboardList : DrawingArea
	{
		ViewModel vm;
		KeyboardBlock [] keyboards;
		
		public KeyboardList (ViewModel viewModel)
		{
			vm = viewModel;
			keyboards = new KeyboardBlock [vm.MaxChannels];
			
			for (int i = 0; i < vm.MaxChannels; i++)
				keyboards [i] = new KeyboardBlock (vm, /*font*/null, i);
			
			//GdkWindow.Background = vm.Pallette.KeyboardBackgroundColor.ToGdk ();
			WidthRequest = vm.KeyboardList.Width;
			HeightRequest = vm.KeyboardList.Height;
				
			vm.MidiMessageReceived += (SmfEvent m) => {
				switch (m.EventType) {
				case SmfEvent.NoteOn:
					keyboards [m.Channel].Keyboard.ProcessMidiMessage (m);
					break;
				case SmfEvent.NoteOff:
					keyboards [m.Channel].Keyboard.ProcessMidiMessage (m);
					break;
				case SmfEvent.CC:
					keyboards [m.Channel].KeyParameters.ProcessMidiMessage (m);
					break;
				}
				if (!dirty) {
					dirty = true;
					QueueDraw ();
				}
			};
		}
		
		bool dirty = true;
		DateTime last = DateTime.MinValue;
		static readonly TimeSpan duration = TimeSpan.FromMilliseconds (50);
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			var ctx = CairoHelper.Create (GdkWindow);
			dirty = false;
			int w, h;
			GdkWindow.GetSize (out w, out h);
			GdkWindow.BeginPaintRect (new Gdk.Rectangle (0, 0, w, h));
			foreach (var kb in keyboards) {
				kb.Keyboard.DoDraw (ctx);
				kb.KeyParameters.DoDraw (ctx);
			}
			GdkWindow.EndPaint ();
			ctx.Dispose ();
			return true;
		}
	}
}


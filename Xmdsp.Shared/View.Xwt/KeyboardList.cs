using System;
using Xwt;
using Commons.Music.Midi;
using Xwt.Drawing;

namespace Xmdsp
{
	public class KeyboardList : Canvas
	{
		ViewModel vm;
		KeyboardBlock [] keyboards;
		
		public KeyboardList (ViewModel viewModel)
		{
			vm = viewModel;
			keyboards = new KeyboardBlock [vm.MaxChannels];
			var font = this.Font.WithSize (vm.KeyboardParameterBlock.KeyBlockHeaderTextSize);
			
			for (int i = 0; i < vm.MaxChannels; i++)
				keyboards [i] = new KeyboardBlock (vm, font, i);
			
			Margin = 0;
			this.BackgroundColor = vm.Pallette.KeyboardBackgroundColor.ToXwt ();
			vm.ScaleChanged += SetSize;

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
					Application.Invoke (QueueDraw);
				}
			};
		}

		internal void SetSize ()
		{
			WidthRequest = vm.KeyboardList.Width * vm.Scale;
			HeightRequest = vm.KeyboardList.Height * vm.Scale;
		}

		bool dirty = true;
		DateTime last = DateTime.MinValue;
		static readonly TimeSpan duration = TimeSpan.FromMilliseconds (50);
			
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			dirty = false;
			ctx.Scale (vm.Scale, vm.Scale);
			foreach (var kb in keyboards) {
				kb.Keyboard.DoDraw (ctx);
				kb.KeyParameters.DoDraw (ctx);
			}
		}
	}
}


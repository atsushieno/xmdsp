using System;
using Xwt;
using Commons.Music.Midi;
using Xwt.Drawing;

namespace Xmdsp
{
	public class KeyboardList : Canvas
	{
		Presenter pm;
		KeyboardBlock [] keyboards;
		
		public KeyboardList (Presenter pm)
		{
			this.pm = pm;
			keyboards = new KeyboardBlock [pm.MaxChannels];
			var font = this.Font.WithSize (pm.KeyboardParameterBlock.KeyBlockHeaderTextSize);
			
			for (int i = 0; i < pm.MaxChannels; i++)
				keyboards [i] = new KeyboardBlock (pm, font, i);
			
			Margin = 0;
			this.BackgroundColor = pm.Pallette.KeyboardBackgroundColor.ToXwt ();
			pm.ScaleChanged += SetSize;

			pm.MidiMessageReceived += (MidiEvent m) => {
				switch (m.EventType) {
				case MidiEvent.NoteOn:
					keyboards [m.Channel].Keyboard.ProcessMidiMessage (m);
					break;
				case MidiEvent.NoteOff:
					keyboards [m.Channel].Keyboard.ProcessMidiMessage (m);
					break;
				case MidiEvent.CC:
				case MidiEvent.Pitch:
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
			WidthRequest = pm.KeyboardList.Width * pm.Scale;
			HeightRequest = pm.KeyboardList.Height * pm.Scale;
		}

		bool dirty = true;
		DateTime last = DateTime.MinValue;
		static readonly TimeSpan duration = TimeSpan.FromMilliseconds (50);
			
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			dirty = false;
			ctx.Scale (pm.Scale, pm.Scale);
			foreach (var kb in keyboards) {
				kb.Keyboard.DoDraw (ctx);
				kb.KeyParameters.DoDraw (ctx);
			}
		}
	}
}


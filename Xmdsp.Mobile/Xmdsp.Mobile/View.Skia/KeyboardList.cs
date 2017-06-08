using System;
using Commons.Music.Midi;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace Xmdsp
{
	public class KeyboardList : SKCanvasView
	{
		Presenter pm;
		KeyboardBlock [] keyboards;
		
		public KeyboardList (Presenter pm)
		{
			this.pm = pm;
			keyboards = new KeyboardBlock [pm.MaxChannels];
			paint.TextSize = (pm.KeyboardParameterBlock.KeyBlockHeaderTextSize);
			
			for (int i = 0; i < pm.MaxChannels; i++)
				keyboards [i] = new KeyboardBlock (pm, i);
			
			Margin = 0;
			this.BackgroundColor = pm.Pallette.KeyboardBackgroundColor.ToForms ();
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
					keyboards [m.Channel].KeyParameters.ProcessMidiMessage (m);
					break;
				}
				if (!dirty) {
					dirty = true;
					//Application.Invoke (QueueDraw);
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
		SKPaint paint = new SKPaint ();

		protected override void OnPaintSurface (SKPaintSurfaceEventArgs e)
		{
			base.OnPaintSurface (e);
			var ctx = e.Surface.Canvas;
			var dirtyRect = e.Info.Rect;

			ctx.Scale ((float) pm.Scale, (float) pm.Scale);
			dirty = false;
			foreach (var kb in keyboards) {
				kb.Keyboard.DoDraw (ctx);
				kb.KeyParameters.DoDraw (ctx, paint);
			}
		}
	}
}


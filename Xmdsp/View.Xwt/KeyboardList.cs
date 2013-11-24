using System;
using Xwt;
using Commons.Music.Midi;
using Xwt.Drawing;
using System.Threading;
using Commons.Music.Midi.Player;

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
			WidthRequest = vm.KeyboardList.Width;
			HeightRequest = vm.KeyboardList.Height;
				
			vm.MidiMessageReceived += (SmfMessage m) => {
				switch (m.MessageType) {
				case SmfMessage.NoteOn:
					keyboards [m.Channel].Keyboard.ProcessMidiMessage (m);
					dirty = true;
					break;
				case SmfMessage.NoteOff:
					keyboards [m.Channel].Keyboard.ProcessMidiMessage (m);
					dirty = true;
					break;
				case SmfMessage.CC:
					keyboards [m.Channel].KeyParameters.ProcessMidiMessage (m);
					dirty = true;
					break;
				}
			};
		}
		
		ImageBuilder back_image_builder;
		Context back_context;
			
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			if (Bounds.IsEmpty || dirtyRect.IsEmpty)
				return;
			
			if (back_context != null)
				ctx.DrawImage (back_image_builder.ToBitmap (ImageFormat.ARGB32), 0, 0);
			else {
				back_image_builder = new ImageBuilder (this.Bounds.Width, this.Bounds.Height);
				back_context = back_image_builder.Context;
				DrawAll (back_context);
				new Thread (RunLoop).Start ();
			}
		}
		
		void RunLoop ()
		{
			while (true) {
				Thread.Sleep (200);
				if (!vm.Model.IsApplicationActive)
					break;
				if (dirty)
					DrawAll (back_context);
			}
		}
		
		void DrawAll (Context ctx)
		{
			foreach (var kb in keyboards) {
				kb.Keyboard.DoDraw (ctx);
				kb.KeyParameters.DoDraw (ctx);
			}
			QueueDraw ();
			dirty = false;
		}
		
		bool dirty = true;
	}
}


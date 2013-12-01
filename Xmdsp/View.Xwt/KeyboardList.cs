using System;
using Gtk;
using Commons.Music.Midi;
using System.Threading;
using Commons.Music.Midi.Player;
using Cairo;

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
			//var font = this.Font.WithSize (vm.KeyboardParameterBlock.KeyBlockHeaderTextSize);
			
			for (int i = 0; i < vm.MaxChannels; i++)
				keyboards [i] = new KeyboardBlock (vm, null/*font*/, i);
						
			//this.BackgroundColor = vm.Pallette.KeyboardBackgroundColor.ToGdk ();
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
			
				if (dirty)
					QueueDraw ();
			};
		}

		bool surface_flipped;
		ImageSurface surface1, surface2;
		Context context1, context2;
		
		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			var ctx = Gdk.CairoHelper.Create (GdkWindow);
			DrawAll (ctx);
			/*
			if (surface1 != null) {
				ctx.SetSourceSurface (surface_flipped ? surface2 : surface1, 0, 0);
				surface_flipped = !surface_flipped;
			} else {
				surface1 = new ImageSurface (Format.Argb32, this.WidthRequest, this.HeightRequest);
				surface2 = new ImageSurface (Format.Argb32, this.WidthRequest, this.HeightRequest);
				context1 = new Context (surface1);
				context2 = new Context (surface2);
				DrawAll (ctx);
				new Thread (RunLoop).Start ();
			}
			*/
			ctx.Dispose ();
			return true; // ?
		}
		
		void RunLoop ()
		{
			while (true) {
				Thread.Sleep (200);
				if (!vm.Model.IsApplicationActive)
					break;
				if (dirty)
					DrawAll (surface_flipped ? context2 : context1);
			}
		}
		
		void DrawAll (Cairo.Context ctx)
		{
			int w, h;
			GdkWindow.GetSize (out w, out h);
			GdkWindow.BeginPaintRect (new Gdk.Rectangle (0, 0, w, h));
			foreach (var kb in keyboards) {
				kb.Keyboard.DoDraw (ctx);
				kb.KeyParameters.DoDraw (ctx);
			}
			GdkWindow.EndPaint ();
			//QueueDraw ();
			dirty = false;
		}
		
		bool dirty = true;
	}
}


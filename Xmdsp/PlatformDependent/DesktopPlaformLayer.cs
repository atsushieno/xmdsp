#define USE_RTMIDI
using System;
using System.IO;
using System.Linq;
using Commons.Music.Midi;
using System.Collections.Generic;

namespace Xmdsp
{
	public class DesktopPlatformLayer : PlatformLayer
	{
		public override Stream GetResourceStream (string identifier)
		{
			return File.OpenRead (identifier);
		}
		
		public override IEnumerable<Model.MidiDeviceInfo> AllMidiDevices {
			get { return MidiAccessManager.Default.Outputs.Select (d => new Model.MidiDeviceInfo () { Id = d.Id, Name = d.Name }); }
		}

		IMidiOutput midi_output;
		string current_device = null;
		
		public override string MidiOutputDeviceId {
			get { return current_device; }
			set {
				current_device = value;
				if (midi_output != null)
					midi_output.CloseAsync ().Wait ();
				midi_output = null;
			}
		}
		
		void OpenOutputDevice ()
		{
			if (midi_output != null)
				midi_output.CloseAsync ().Wait ();
			string dev = current_device == null ? AllMidiDevices.Last ().Id : current_device;
			midi_output = MidiAccessManager.Default.OpenOutputAsync (dev).Result;
		}
		
		public override MidiPlayer CreateMidiPlayer (SmfMusic music)
		{
			if (midi_output == null)
				OpenOutputDevice ();
			return new MidiPlayer (music, midi_output);
		}
		
		public override void Shutdown ()
		{
			if (midi_output != null)
				midi_output.CloseAsync ().Wait ();
			midi_output = null;
		}
	}
}


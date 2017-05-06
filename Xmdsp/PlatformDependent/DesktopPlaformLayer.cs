#define USE_RTMIDI
using System;
using System.IO;
using System.Linq;
using Commons.Music.Midi;
using System.Collections.Generic;
using Commons.Music.Midi.RtMidi;
using Commons.Music.Midi.WinMM;

namespace Xmdsp
{
	public class DesktopPlatformLayer : PlatformLayer
	{
		public override Stream GetResourceStream (string identifier)
		{
			return File.OpenRead (identifier);
		}

		IMidiAccess midi_access = Environment.OSVersion.Platform == PlatformID.Unix ? (IMidiAccess) new RtMidiAccess () : new WinMMMidiAccess ();
		
		public override IEnumerable<Model.MidiDeviceInfo> AllMidiDevices {
			get { return midi_access.Outputs.Select (d => new Model.MidiDeviceInfo () { Id = d.Id, Name = d.Name }); }
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
			midi_output = midi_access.OpenOutputAsync (dev).Result;
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


using System;
using System.IO;
using System.Linq;
using Commons.Music.Midi;
using Commons.Music.Midi.Player;
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
			get { return PortMidiSharp.MidiDeviceManager.AllDevices.Where (d => d.IsOutput).Select (d => new Model.MidiDeviceInfo () { ID = d.ID, Name = d.Name }); }
		}
		
		PortMidiSharp.MidiOutput midi_output;
		int current_device;
		
		public override int MidiOutputDeviceIndex {
			get { return current_device; }
			set {
				current_device = value;
				if (midi_output != null)
					midi_output.Close ();
				midi_output = null;
			}
		}
		
		void OpenOutputDevice ()
		{
			if (midi_output != null)
				midi_output.Close ();
			midi_output = PortMidiSharp.MidiDeviceManager.OpenOutput (current_device);
		}
		
		public override MidiPlayer CreateMidiPlayer (SmfMusic music)
		{
			if (midi_output == null)
				OpenOutputDevice ();
			return new PortMidiPlayer (midi_output, music);
		}
		
		public override void Shutdown ()
		{
			// do particularly nothing (PortMidi shuts down at AppDomainUnload.
		}
	}
}


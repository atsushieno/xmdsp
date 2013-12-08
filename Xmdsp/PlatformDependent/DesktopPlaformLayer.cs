#define USE_RTMIDI
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
#if USE_RTMIDI
			get { return RtMidiSharp.MidiDeviceManager.AllDevices.Where (d => d.IsOutput).Select (d => new Model.MidiDeviceInfo () { ID = d.ID, Name = d.Name }); }
#else
			get { return PortMidiSharp.MidiDeviceManager.AllDevices.Where (d => d.IsOutput).Select (d => new Model.MidiDeviceInfo () { ID = d.ID, Name = d.Name }); }
#endif
		}
		
#if USE_RTMIDI
		RtMidiSharp.RtMidiOutputDevice midi_output;
#else
		PortMidiSharp.MidiOutput midi_output;
#endif
		int current_device = -1;
		
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
			int dev = current_device < 0 ? AllMidiDevices.First ().ID : current_device;
#if USE_RTMIDI
			midi_output = RtMidiSharp.MidiDeviceManager.OpenOutput (dev);
#else
			midi_output = PortMidiSharp.MidiDeviceManager.OpenOutput (dev);
#endif
		}
		
		public override MidiPlayer CreateMidiPlayer (SmfMusic music)
		{
			if (midi_output == null)
				OpenOutputDevice ();
#if USE_RTMIDI
			return new RtMidiPlayer (midi_output, music);
#else
			return new PortMidiPlayer (midi_output, music);
#endif
		}
		
		public override void Shutdown ()
		{
			// do particularly nothing (PortMidi shuts down at AppDomainUnload.
		}
	}
}


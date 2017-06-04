using System;
using System.IO;
using System.Collections.Generic;
using Commons.Music.Midi;
using System.Linq;

namespace Xmdsp
{
	public abstract class PlatformLayer
	{
		public abstract IMidiAccess CreateMidiAccess ();
		public abstract Stream GetResourceStream (string identfier);

		IMidiAccess midi_access;

		IMidiAccess MidiAccess {
			get { return midi_access = midi_access ?? CreateMidiAccess (); }
		}

		public virtual IEnumerable<Model.MidiDeviceInfo> AllMidiDevices {
			get { return MidiAccess.Outputs.Select (d => new Model.MidiDeviceInfo () { Id = d.Id, Name = d.Name }); }
		}

		IMidiOutput midi_output;
		string current_device = null;

		public virtual string MidiOutputDeviceId {
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
			midi_output = MidiAccess.OpenOutputAsync (dev).Result;
		}

		public virtual MidiPlayer CreateMidiPlayer (MidiMusic music)
		{
			if (midi_output == null)
				OpenOutputDevice ();
			return new MidiPlayer (music, midi_output);
		}

		public virtual void Shutdown ()
		{
			if (midi_output != null)
				midi_output.CloseAsync ().Wait ();
			midi_output = null;
		}
	}
}
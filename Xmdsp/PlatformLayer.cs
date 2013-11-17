using System;
using System.IO;
using System.Collections.Generic;
using Commons.Music.Midi;
using Commons.Music.Midi.Player;

namespace Xmdsp
{
	public abstract class PlatformLayer
	{
		public PlatformLayer ()
		{
			
		}
		
		public abstract int MidiOutputDeviceIndex { get; set; }
		public abstract Stream GetResourceStream (string identfier);
		public abstract IEnumerable<Model.MidiDeviceInfo> AllMidiDevices { get; }
		public abstract MidiPlayer CreateMidiPlayer (SmfMusic music);
		public abstract void Shutdown ();
	}
}


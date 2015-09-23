using System;
using System.IO;
using System.Collections.Generic;
using Commons.Music.Midi;

namespace Xmdsp
{
	public abstract class PlatformLayer
	{
		public PlatformLayer ()
		{
			
		}
		
		public abstract string MidiOutputDeviceId { get; set; }
		public abstract Stream GetResourceStream (string identfier);
		public abstract IEnumerable<Model.MidiDeviceInfo> AllMidiDevices { get; }
		public abstract MidiPlayer CreateMidiPlayer (SmfMusic music);
		public abstract void Shutdown ();
	}
}


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
		public override IMidiAccess CreateMidiAccess ()
		{
			return Environment.OSVersion.Platform == PlatformID.Unix ? (IMidiAccess) new RtMidiAccess () : new WinMMMidiAccess ();
		}

		public override Stream GetResourceStream (string identifier)
		{
			return File.OpenRead (identifier);
		}
	}
}


#define USE_RTMIDI
using System;
using System.IO;
using System.Linq;
using Commons.Music.Midi;
using System.Collections.Generic;
using Commons.Music.Midi.CoreMidiApi;

namespace Xmdsp
{
	public class DesktopPlatformLayer : PlatformLayer
	{
		public override IMidiAccess CreateMidiAccess ()
		{
			return new CoreMidiAccess ();
		}

		public override Stream GetResourceStream (string identifier)
		{
			return File.OpenRead (identifier);
		}
	}
}


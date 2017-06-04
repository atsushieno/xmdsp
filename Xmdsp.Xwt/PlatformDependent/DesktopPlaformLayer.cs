#define USE_RTMIDI
using System;
using System.IO;
using System.Linq;
using Commons.Music.Midi;
using System.Collections.Generic;

namespace Xmdsp
{
	public partial class DesktopPlatformLayer : PlatformLayer
	{
		public override Stream GetResourceStream (string identifier)
		{
			return File.OpenRead (identifier);
		}
	}
}


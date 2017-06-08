#define USE_RTMIDI
using System;
using System.IO;
using System.Linq;
using Commons.Music.Midi;
using System.Collections.Generic;

namespace Xmdsp
{
	public partial class FormsPlatformLayer : PlatformLayer
	{
		public override Stream GetResourceStream (string identifier)
		{
			throw new NotImplementedException ();
			//return File.OpenRead (identifier);
		}
	}
}


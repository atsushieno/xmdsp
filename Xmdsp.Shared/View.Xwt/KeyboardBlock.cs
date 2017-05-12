using System;
using Xwt;
using Xwt.Drawing;

namespace Xmdsp
{
	public class KeyboardBlock
	{
		Presenter pm;
		
		public KeyboardBlock (Presenter pm, Font font, int channel)
		{
			this.pm = pm;
			Keyboard = new Keyboard (pm, channel);
			KeyParameters = new KeyboardParameterBlock (pm, font, channel);
		}

		public Keyboard Keyboard { get; private set; }
		public KeyboardParameterBlock KeyParameters { get; private set; }
	}
}


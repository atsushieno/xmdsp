using System;
using Xwt;
using Xwt.Drawing;

namespace Xmdsp
{
	public class KeyboardBlock
	{
		ViewModel vm;
		
		public KeyboardBlock (ViewModel viewModel, Font font, int channel)
		{
			vm = viewModel;
			Keyboard = new Keyboard (vm, channel);
			KeyParameters = new KeyboardParameterBlock (vm, font, channel);
		}

		public Keyboard Keyboard { get; private set; }
		public KeyboardParameterBlock KeyParameters { get; private set; }
	}
}


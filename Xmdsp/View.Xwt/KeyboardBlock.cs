using System;
using Gtk;
using Gdk;
using Pango;

namespace Xmdsp
{
	public class KeyboardBlock
	{
		ViewModel vm;
		
		public KeyboardBlock (ViewModel viewModel, FontFace font, int channel)
		{
			vm = viewModel;
			Keyboard = new Keyboard (vm, channel);
			KeyParameters = new KeyboardParameterBlock (vm, font, channel);
		}

		public Keyboard Keyboard { get; private set; }
		public KeyboardParameterBlock KeyParameters { get; private set; }
	}
}


using System;
using Xwt;

namespace Xmdsp
{
	public class KeyboardBlock : VBox
	{
		ViewModel vm;
		
		public KeyboardBlock (ViewModel viewModel, int channel)
		{
			vm = viewModel;
			Keyboard = new Keyboard (vm);
			KeyParameters = new KeyboardParameterBlock (vm, channel);
			WidthRequest = Keyboard.WidthRequest + KeyParameters.WidthRequest;
			HeightRequest = Keyboard.HeightRequest + KeyParameters.HeightRequest;
			this.PackStart (KeyParameters, true);
			this.PackStart (Keyboard, true);
		}

		public Keyboard Keyboard { get; private set; }
		public KeyboardParameterBlock KeyParameters { get; private set; }
	}
}


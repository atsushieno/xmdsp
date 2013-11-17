using System;
using Xwt;
using Commons.Music.Midi;

namespace Xmdsp
{
	public class KeyboardList : VBox
	{
		ViewModel vm;
		KeyboardBlock [] keyboards;
		
		public KeyboardList (ViewModel viewModel)
		{
			vm = viewModel;
			keyboards = new KeyboardBlock [vm.MaxChannels];
			for (int i = 0; i < vm.MaxChannels; i++)
				PackStart ((keyboards [i] = new KeyboardBlock (vm, i)), true);
				
			vm.MidiMessageReceived += (SmfMessage m) => {
				switch (m.MessageType) {
				case SmfMessage.NoteOn:
					keyboards [m.Channel].Keyboard.ProcessMidiMessage (m);
					break;
				case SmfMessage.NoteOff:
					keyboards [m.Channel].Keyboard.ProcessMidiMessage (m);
					break;
				}
			};
		}
	}
}


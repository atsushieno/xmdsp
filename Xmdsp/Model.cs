using System;
using Commons.Music.Midi;
using System.Collections.Generic;
using Commons.Music.Midi.Player;

namespace Xmdsp
{
	public class Model : IDisposable
	{
		public class MidiDeviceInfo
		{
			public int ID { get; set; }
			public string Name { get; set; }
		}
		
		public Model ()
		{
			//MidiMachine = new MidiMachine ();
			Platform = new DesktopPlatformLayer ();
			IsApplicationActive = true;
		}
		
		public void Dispose ()
		{
			IsApplicationActive = false;
			EnsurePlayerStopped ();
			Platform.Shutdown ();
		}

		public bool IsApplicationActive { get; set; }
		
		public event MidiEventAction MidiMessageReceived;
		//public MidiMachine MidiMachine { get; private set; }
		
		public PlatformLayer Platform { get; private set; }
		
		SmfMusic current_music;
		MidiPlayer current_player;
		
		void EnsurePlayerStopped ()
		{
			if (current_player != null)
				current_player.Dispose ();
		}
		
		public void LoadSmf (string filename)
		{
			EnsurePlayerStopped ();
			using (var stream = Platform.GetResourceStream (filename)) {
				var r = new SmfReader (stream);
				r.Parse ();
				current_music = r.Music;
			}
		}
		
		public void Play ()
		{
			if (current_music == null)
				return; // nothing to play
			EnsurePlayerStopped ();
			current_player = Platform.CreateMidiPlayer (current_music);
			current_player.EventReceived += MidiMessageReceived;
			current_player.PlayAsync ();
		}
		
		public PlayerState PlayerState {
			get { return current_player != null ? current_player.State : PlayerState.Stopped; }
		}
	}
}


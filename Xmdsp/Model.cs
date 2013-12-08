using System;
using Commons.Music.Midi;
using System.Collections.Generic;
using Commons.Music.Midi.Player;

namespace Xmdsp
{
	public class Model : IDisposable
	{
		public const string VersionNumbers = "0.03";
		
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
		
		public MidiPlayer Player {
			get { return current_player; }
		}
		
		public PlatformLayer Platform { get; private set; }

		SmfMusic current_music;
		MidiPlayer current_player;
		
		public event Action<PlayerState> PlayerStateChanged;
		
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
			current_player.Finished += () => {
				if (PlayerStateChanged != null)
					PlayerStateChanged (PlayerState.Stopped);
			};
			current_player.EventReceived += MidiMessageReceived;
			current_player.PlayAsync ();
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Playing);
		}
		
		public void Pause ()
		{
			if (current_player == null)
				return; // ignore
			current_player.PauseAsync ();
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Paused);
		}
		
		public void Stop ()
		{
			if (current_player == null)
				return; // ignore
			current_player.Dispose ();
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Stopped);
		}
		
		public void StartFastForward ()
		{
			if (current_player == null)
				return; // ignore
			if (current_player.State == PlayerState.Paused)
				current_player.PlayAsync ();
			current_player.SetTempoRatio (2.0);
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.FastForward);
		}
		
		public void StopFastForward ()
		{
			if (current_player == null || current_player.State != PlayerState.Playing)
				return; // ignore
			current_player.SetTempoRatio (1.0);
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Playing);
		}
	}
}


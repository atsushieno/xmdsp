using System;
using Commons.Music.Midi;
using System.Collections.Generic;
using Commons.Music.Midi.Player;
using System.Timers;

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
		
		public event Action PlayStarted;
		
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

		// either play or resume
		public void Play ()
		{
			if (current_music == null)
				return; // nothing to play
			if (current_player != null && current_player.State == PlayerState.Paused)
				Resume ();
			else
				DoPlay ();
		}
		
		void DoPlay ()
		{
			EnsurePlayerStopped ();
			current_player = Platform.CreateMidiPlayer (current_music);
			current_player.Finished += () => {
				if (PlayerStateChanged != null)
					PlayerStateChanged (PlayerState.Stopped);
			};
			current_player.EventReceived += MidiMessageReceived;
			timer_resumed = PlayStartedTime = DateTime.Now;
			timer_offset = TimeSpan.Zero;
			current_player.PlayAsync ();
			if (PlayStarted != null)
				PlayStarted ();
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Playing);
			
			timer = new Timer (timer_fps);
			timer.Elapsed += delegate {
				if (PlayTimerTick != null)
					PlayTimerTick ();
			};
			timer.Enabled = true;
		}
		
		public void Resume ()
		{
			if (current_player == null)
				return; // nothing to play
			current_player.PlayAsync ();
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Playing);
			timer_resumed = DateTime.Now;
			timer.Start ();
		}
		
		public void Pause ()
		{
			if (current_player == null)
				return; // ignore
			current_player.PauseAsync ();
			timer_offset += DateTime.Now - timer_resumed;
			timer.Enabled = false;
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
			timer.Enabled = false;
			timer.Dispose ();
			timer = null;
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
		
		public event Action PlayTimerTick;
		Timer timer;
		const long timer_fps = 50;
		DateTime timer_resumed;
		TimeSpan timer_offset;
		double tempo_ratio = 1.0;
		
		public DateTime PlayStartedTime { get; private set; }
		public TimeSpan PositionInTime {
			get { return GetTimerOffsetWithTempoRatio () + timer_offset; }
		}
		
		TimeSpan GetTimerOffsetWithTempoRatio ()
		{
			return TimeSpan.FromMilliseconds ((DateTime.Now - timer_resumed).TotalMilliseconds * tempo_ratio);
		}
		
		public void ProcessChangeTempoRatio (double ratio)
		{
			timer_offset += GetTimerOffsetWithTempoRatio ();
			timer_resumed = DateTime.Now;
			tempo_ratio = ratio;
		}
	}
}


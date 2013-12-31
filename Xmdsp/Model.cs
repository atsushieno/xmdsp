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
			PlayStartedTime = DateTime.Now;
			current_player.PlayAsync ();
			if (PlayStarted != null)
				PlayStarted ();
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Playing);
			
			timer = new Timer (timer_fps);
			timer.Elapsed += OnTimerElapsed;
			timer.Enabled = true;
		}
		
		public void Resume ()
		{
			if (current_player == null)
				return; // nothing to play
			current_player.PlayAsync ();
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Playing);
			timer.Enabled = true;
		}
		
		public void Pause ()
		{
			if (current_player == null)
				return; // ignore
			current_player.PauseAsync ();
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
			current_player = null;
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
			Player.SetTempoRatio (1.0);
			current_player.SetTempoRatio (1.0);
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Playing);
		}
		
		public void ProcessChangeTempoRatio (double ratio)
		{
			if (current_player == null)
				return; // ignore
			Player.SetTempoRatio (ratio);
		}
		
		public event Action PlayTimerTick;
		public event Action TickProgress;
		
		Timer timer;
		const long timer_fps = 80;
		
		public DateTime PlayStartedTime { get; private set; }
		
		DateTime time_last_tick_based_progress = DateTime.MinValue;
		const double tick_progress_ratio = 2000.0 / 16; // 16 events per 192 ticks (2 sec. on BPM 120)
		
		void OnTimerElapsed (object o, ElapsedEventArgs e)
		{
			if (PlayTimerTick != null)
				PlayTimerTick ();
			if (TickProgress == null)
				return;
			var ts = DateTime.Now - time_last_tick_based_progress;
			var delta = tick_progress_ratio / (Player.Bpm * Player.TempoChangeRatio) * 120;
			if (ts.TotalMilliseconds > delta) {
				time_last_tick_based_progress = DateTime.Now;
				TickProgress ();
			}
		}
	}
}


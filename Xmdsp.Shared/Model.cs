using System;
using Commons.Music.Midi;
using System.Collections.Generic;
using System.Threading;

namespace Xmdsp
{
	public class Model : IDisposable
	{
		public const string VersionNumbers = "0.1.0";
		
		public class MidiDeviceInfo
		{
			public string Id { get; set; }
			public string Name { get; set; }
		}
		
		public Model (PlatformLayer platformLayer)
		{
			//MidiMachine = new MidiMachine ();
			Platform = platformLayer;
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

		MidiMusic current_music;
		MidiPlayer current_player;

		public event Action<PlayerState> PlayerStateChanged;

		public Action<Action> StartTimer;
		public Action PauseTimer;
		public Action ResumeTimer;
		public Action StopTimer;

		void EnsurePlayerStopped ()
		{
			if (current_player != null)
				current_player.Dispose ();
		}

		public void LoadSmf (string filename)
		{
			EnsurePlayerStopped ();
			Action doLoadSmf = () => {
				using (var stream = Platform.GetResourceStream (filename))
					current_music = MidiMusic.Read (stream);
			};
			Platform.StartWatchingFile (filename, delegate {
				doLoadSmf ();
				Stop ();
				Play ();
			});
			doLoadSmf ();
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

			StartTimer (OnTimerElapsed);
		}

		public void Resume ()
		{
			if (current_player == null)
				return; // nothing to play
			current_player.PlayAsync ();
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Playing);
			ResumeTimer ();
		}

		public void Pause ()
		{
			if (current_player == null)
				return; // ignore
			current_player.PauseAsync ();
			if (PauseTimer != null)
				PauseTimer ();
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Paused);
		}

		public void Stop ()
		{
			if (current_player == null)
				return; // ignore
			current_player.PauseAsync ();
			current_player.Dispose ();
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Stopped);
			current_player = null;
			if (StopTimer != null)
				StopTimer ();
		}

		public void StartFastForward ()
		{
			if (current_player == null)
				return; // ignore
			if (current_player.State == PlayerState.Paused)
				current_player.PlayAsync ();
			current_player.TempoChangeRatio = 2.0;
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.FastForward);
		}
		
		public void StopFastForward ()
		{
			if (current_player == null || current_player.State != PlayerState.Playing)
				return; // ignore
			Player.TempoChangeRatio = 1.0;
			current_player.TempoChangeRatio = 1.0;
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Playing);
		}
		
		public void ProcessChangeTempoRatio (double ratio)
		{
			if (current_player == null)
				return; // ignore
			Player.TempoChangeRatio = ratio;
		}
		
		public event Action PlayTimerTick;
		public event Action TickProgress;
		
		public DateTime PlayStartedTime { get; private set; }
		
		DateTime time_last_tick_based_progress = DateTime.MinValue;
		const double tick_progress_ratio = 2000.0 / 16; // 16 events per 192 ticks (2 sec. on BPM 120)
		
		void OnTimerElapsed ()
		{
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

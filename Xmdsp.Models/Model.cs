using System;
using Commons.Music.Midi;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

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
			DefaultConfiguration = new LoadableConfiguration (this);
			LoadDefault ();
			Platform.MidiOutputDeviceId = DefaultConfiguration.LastSelectedDevice;
			if (!string.IsNullOrEmpty (DefaultConfiguration.LastPlayedFile))
				LoadSmf (DefaultConfiguration.LastPlayedFile);
			StartTimer = Platform.StartTimer;
			StopTimer = Platform.StopTimer;
		}
		
		public void Dispose ()
		{
			IsApplicationActive = false;
			Stop ();
			Platform.Shutdown ();
		}

		public class LoadableConfiguration
		{
			public LoadableConfiguration (Model parentModel)
			{
				model = parentModel;
			}

			Model model;

			string last_played_file, last_selected_device;

			internal bool OnLoading { get; set; }

			public string LastPlayedFile {
				get => last_played_file;
				set {
					last_played_file = value;
					if (!OnLoading)
						Save ();
				}
			}
			public string LastSelectedDevice {
				get => last_selected_device;
				set {
					last_selected_device = value;
					if (!OnLoading)
						Save ();
				}
			}

			void Save ()
			{
				var doc = new XDocument (
					new XElement ("config",
						new XElement ("last-played-file", last_played_file),
						new XElement ("last-selected-device", last_selected_device)));
				model.Platform.SaveConfigurationString (doc.ToString ());
			}
		}

		public void LoadDefault ()
		{
			try {
				var xml = Platform.LoadConfigurationString ();
				var doc = string.IsNullOrEmpty (xml) ? null : XDocument.Parse (xml);
				DefaultConfiguration.OnLoading = true;
				DefaultConfiguration.LastPlayedFile = doc?.XPathSelectElement ("/config/last-played-file")?.Value;
				DefaultConfiguration.LastSelectedDevice = doc?.XPathSelectElement ("/config/last-selected-device")?.Value;
				DefaultConfiguration.OnLoading = false;
			} catch (Exception ex) {
				// FIXME: we need some error reporting system
				Console.WriteLine ($"Error while loading default settings: {ex.Message}");
				Console.WriteLine ("Details: " + ex);
			}
		}

		public LoadableConfiguration DefaultConfiguration { get; private set; }

		public bool IsApplicationActive { get; set; }

		public event Action PlayStarted;
		
		public event MidiEventAction MidiMessageReceived;
		//public MidiMachine MidiMachine { get; private set; }
		
		public IMusicPlayer Player => current_player;

		public MidiMusic Music => current_music;
		
		public PlatformLayer Platform { get; private set; }

		MidiMusic current_music;
		IMusicPlayer current_player;

		public event Action<PlayerState> PlayerStateChanged;

		public Action<Action> StartTimer;
		public Action PauseTimer;
		public Action ResumeTimer;
		public Action StopTimer;

		public event Action SmfLoaded;

		public void LoadSmf (string filename)
		{
			Stop ();
			Action doLoadSmf = () => {
				try {
					using (var stream = Platform.GetResourceStream (filename))
						// merge tracks for optimization (many operations run merger at run-time if they are not in format 0).
						current_music = SmfTrackMerger.Merge (MidiMusic.Read (stream));
					DefaultConfiguration.LastPlayedFile = filename;
					cached_total_ticks = Music.GetTotalTicks ();
					if (SmfLoaded != null)
						SmfLoaded ();
				} catch (Exception ex) {
					// FIXME: there should be some error reporting system...
					Console.WriteLine ("Error while loading MIDI file: " + ex.Message);
					Console.WriteLine ("Details: " + ex);
				}
			};
			Platform.StartWatchingFile (filename, delegate {
				Stop ();
				doLoadSmf ();
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
			if (current_player != null)
				current_player.Dispose ();
			current_player = Platform.CreateMidiPlayer (current_music);
			if (current_channel_mask != null)
				current_player.SetChannelMask (current_channel_mask);
			current_player.Finished += () => {
				StopTimer ();
				if (PlayerStateChanged != null)
					PlayerStateChanged (PlayerState.Stopped);
			};
			current_player.EventReceived += MidiMessageReceived;
			PlayStartedTime = DateTime.Now;
			
			if (next_start_from_ticks != 0) {
				current_player.SeekAsync (next_start_from_ticks);
				next_start_from_ticks = 0;
			}

			current_player.PlayAsync ();
			if (PlayStarted != null)
				PlayStarted ();
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Playing);

			StopTimer ();
			StartTimer (OnTimerElapsed);
		}

		public void Resume ()
		{
			if (current_player == null)
				return; // nothing to play
			current_player.PlayAsync ();
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Playing);
			if (ResumeTimer != null)
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
			int iter = 0;
			// is it hacky...?
			while (current_player.State != PlayerState.Stopped) {
				Thread.SpinWait (10000);
				if (iter++ > 100)
					break;
			}
			if (PlayerStateChanged != null)
				PlayerStateChanged (PlayerState.Stopped);
			current_player.Dispose ();
			current_player = null;
			if (StopTimer != null)
				StopTimer ();
		}

		private int next_start_from_ticks;

		public void SeekByDeltaTime (int ticks)
		{
			if (current_player == null) {
				next_start_from_ticks = ticks;
				return;
			}
			current_player.SeekAsync (ticks);
		}

		public void SeekByPercent (double percent)
		{
			SeekByDeltaTime ((int) (percent / 100.0 * cached_total_ticks));
		}

		public void ProcessChangeTempoRatio (double ratio)
		{
			if (current_player == null)
				return; // ignore
			Player.TempoChangeRatio = ratio;
		}
		
		public event Action PlayTimerTick;
		public event Action TickProgress;

		private int cached_total_ticks;
		// 0..1
		public double PlayerProgress => current_player == null ? 0 : current_player.PlayDeltaTime / (double) cached_total_ticks;
		
		public DateTime PlayStartedTime { get; private set; }
		public string MidiOutputDeviceId {
			get => Platform.MidiOutputDeviceId;
			set {
				Platform.MidiOutputDeviceId = value;
				DefaultConfiguration.LastSelectedDevice = value;
			}
		}

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

		bool [] current_channel_mask;

		public void SetChannelMasks (bool [] channelMask)
		{
			if (current_player != null)
				current_player.SetChannelMask (channelMask);
			current_channel_mask = (bool []) channelMask.Clone ();
		}
	}
}

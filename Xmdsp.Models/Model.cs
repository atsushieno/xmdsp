using System;
using Commons.Music.Midi;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

			string last_played_file, last_selected_device, base_color, bg_color, note_on_color;

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

			public string BaseColor {
				get => base_color;
				set
				{
					base_color = value;
					if (!OnLoading)
						Save ();
				}
			}

			public string BackgroundColor {
				get => bg_color;
				set
				{
					bg_color = value;
					if (!OnLoading)
						Save ();
				}
			}
			
			public string NoteOnColor {
				get => note_on_color;
				set
				{
					note_on_color = value;
					if (!OnLoading)
						Save ();
				}
			}

			void Save ()
			{
				var doc = new XDocument (
					new XElement ("config",
						new XElement ("last-played-file", last_played_file),
						new XElement ("last-selected-device", last_selected_device),
						new XElement ("base-color", base_color),
						new XElement ("note-on-color", note_on_color)));
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
				ReportError ($"Error while loading default settings: {ex.Message}");
				ReportError ("Details: " + ex);
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

		public event Action<string> Report;

		internal void ReportError (string message)
		{
			if (Report != null)
				Report (message);
		}

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
		
		public IEnumerable<Tuple<string,TimeSpan,int>> CurrentMusicMarkers {
			get {
				foreach (var m in Music.GetMetaEventsOfType (MidiMetaType.Marker)) {
					var markerText = "";
					try {
						markerText = Encoding.Default.GetString (m.Event.Data);
					}
					catch (ArgumentException) {
						markerText = Encoding.UTF8.GetString (m.Event.Data);
					}

					var milliseconds = Music.GetTimePositionInMillisecondsForTick (m.DeltaTime);
					yield return new Tuple<string,TimeSpan,int> (markerText,
						TimeSpan.FromMilliseconds (milliseconds), m.DeltaTime);
				}
			}
		}

		public void JumpToMarker (int i)
		{
			var markers = CurrentMusicMarkers.ToArray ();
			if (i < markers.Length) {
				var m = markers [i];
				SeekByDeltaTime (m.Item3);
			}
		}
		
		public MediaPlayerQueue Queue { get; private set; } = new MediaPlayerQueue ();

		public class MediaPlayerQueue
		{
			public IList<MediaFile> QueuedFiles { get; private set; } = new List<MediaFile> ();
		}

		public class MediaFile
		{
			public MediaFile (string filePath)
			{
				FullPath = Path.GetFullPath (filePath);
				FileName = Path.GetFileName (FullPath);
				using (var s = File.OpenRead (FullPath)) {
					var music = MidiMusic.Read (s);
					var data = music.GetMetaEventsOfType (MidiMetaType.TrackName).FirstOrDefault().Event.Data;
					if (data != null)
						Title = Encoding.UTF8.GetString (data);
					TotalPlayTime = music.GetTotalPlayTimeMilliseconds ();
				}
			}

			public string FileName { get; private set; }
			public string FullPath { get; private set; }
			public string Title { get; private set; }
			public int TotalPlayTime { get; private set; }
		}

		public event EventHandler PalletteChanged;

		public const string DefaultBaseColor = "#FF80A0FF";
		public const string DefaultBackgroundColor = "#FF000000";
		public const string DefaultNoteOnColor = "#FFFF4080";

		public string BaseColor {
			get => DefaultConfiguration.BaseColor ?? DefaultBaseColor;
			set {
				DefaultConfiguration.BaseColor = value;
				if (PalletteChanged != null)
					PalletteChanged (this, EventArgs.Empty);
			}
		}

		public string BackgroundColor {
			get => DefaultConfiguration.BackgroundColor ?? DefaultBackgroundColor;
			set {
				DefaultConfiguration.BackgroundColor = value;
				if (PalletteChanged != null)
					PalletteChanged (this, EventArgs.Empty);
			}
		}

		public string NoteOnColor {
			get => DefaultConfiguration.NoteOnColor ?? DefaultNoteOnColor;
			set {
				DefaultConfiguration.NoteOnColor = value;
				if (PalletteChanged != null)
					PalletteChanged (this, EventArgs.Empty);
			}
		}
	}
}

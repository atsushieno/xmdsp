using System;
using System.IO;
using System.Collections.Generic;
using Commons.Music.Midi;
using System.Linq;
using System.IO.IsolatedStorage;
using System.Timers;

namespace Xmdsp
{
	public abstract class PlatformLayer
	{
		public abstract Stream GetResourceStream (string identfier);

		IMidiAccess midi_access;

		IMidiAccess MidiAccess {
			get { return midi_access = midi_access ?? MidiAccessManager.Default; }
		}

		public virtual IEnumerable<Model.MidiDeviceInfo> AllMidiDevices {
			get { return MidiAccess.Outputs.Select (d => new Model.MidiDeviceInfo () { Id = d.Id, Name = d.Name }); }
		}

		IMidiOutput midi_output;
		string current_device = null;

		public virtual string MidiOutputDeviceId {
			get { return current_device; }
			set {
				current_device = value;
				if (midi_output != null)
					midi_output.CloseAsync ().Wait ();
				midi_output = null;
			}
		}

		void OpenOutputDevice ()
		{
			if (midi_output != null)
				midi_output.CloseAsync ().Wait ();
			string dev = string.IsNullOrEmpty (current_device) ? AllMidiDevices.Last ().Id : current_device;
			midi_output = MidiAccess.OpenOutputAsync (dev).Result;
		}

		public virtual IMusicPlayer CreateMidiPlayer (MidiMusic music)
		{
			if (midi_output == null)
				OpenOutputDevice ();
			return new MidiMusicPlayer (music, midi_output);
		}

		public virtual void Shutdown ()
		{
			StopWatchingFile ();

			if (midi_output != null)
				midi_output.CloseAsync ().Wait ();
			midi_output = null;
		}

		public bool WatchFileChanges { get; set; } = true;

		public virtual string LoadConfigurationString ()
		{
			using (var storage = IsolatedStorageFile.GetUserStoreForAssembly ()) {
				if (!storage.FileExists ("xmdsp.config"))
					return "";
				using (var reader = new StreamReader (storage.OpenFile ("xmdsp.config", FileMode.Open)))
					return reader.ReadToEnd ();
			}
		}

		public virtual void SaveConfigurationString (string s)
		{
			using (var storage = IsolatedStorageFile.GetUserStoreForAssembly ()) {
				using (var writer = new StreamWriter (storage.OpenFile ("xmdsp.config", FileMode.Create)))
					writer.Write (s);
			}
		}

		FileSystemWatcher fs_watcher;

		public virtual void StartWatchingFile (string file, Action action)
		{
			StopWatchingFile ();
			fs_watcher = new FileSystemWatcher ();
			var fp = Path.GetFullPath (file);
			fs_watcher.Path = Path.GetDirectoryName (fp);
			fs_watcher.Changed += (o, e) => {
				if (WatchFileChanges)
				if (e.FullPath == fp)
					action ();
			};
			fs_watcher.EnableRaisingEvents = true;
		}

		public virtual void StopWatchingFile ()
		{
			if (fs_watcher == null)
				return;
			fs_watcher.EnableRaisingEvents = false;
			fs_watcher.Dispose ();
		}

		const long timer_fps = 30;
		Timer timer;

		public virtual void StartTimer (Action onTimerElapsed)
		{
			timer = new Timer (1000.0 / timer_fps);
			timer.Elapsed += (o, e) => onTimerElapsed ();
			timer.Enabled = true;
		}

		void ResumeTimer ()
		{
			timer.Enabled = true;
		}

		void PauseTimer ()
		{
			timer.Enabled = false;
		}

		public virtual void StopTimer ()
		{
			if (timer == null)
				return;
			timer.Enabled = false;
			timer.Dispose ();
			timer = null;
		}
	}
}

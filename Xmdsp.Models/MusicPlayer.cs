using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Music.Midi;

namespace Xmdsp
{
	public interface IMusicPlayer : IDisposable
	{
		PlayerState State { get; }

		event MidiEventAction EventReceived; 
		event Action Finished;

		void PlayAsync ();
		void PauseAsync ();
		void SeekAsync (int ticks);
		
		double TempoChangeRatio { get; set; }
		
		int Bpm { get; }
		
		byte [] TimeSignature { get; }
		TimeSpan PositionInTime { get; }
		int PlayDeltaTime { get; }
	}

	// Plays multiple SMF in parallel, to support more than 16 channels at a time.
	// The music files must have consistent time signature, meta events and so on.
	public class MultiFileMidiPlayer : IMusicPlayer
	{
		public class MusicPortMapping
		{
			public MidiMusic Music { get; set; }
			public IMidiOutput Output { get; set; }
		}

		public MultiFileMidiPlayer (IEnumerable<MusicPortMapping> mappings)
		{
			foreach (var p in mappings)
				players.Add (new MidiMusicPlayer (p.Music, p.Output));
		}

		private IList<MidiMusicPlayer> players = new List<MidiMusicPlayer> ();

		private MusicPortMapping map;

		public void Dispose ()
		{
			foreach (var p in players) p.Dispose ();
		}

		public PlayerState State => players.First ().State;
		public event MidiEventAction EventReceived {
			add {
				foreach (var p in players)
					p.EventReceived += value;
			}
			remove {
				foreach (var p in players)
					p.EventReceived -= value;
			}
		}
		public event Action Finished {
			add {
				foreach (var p in players)
					p.Finished += value;				
			}
			remove {
				foreach (var p in players)
					p.Finished += value;				
			}
		}
		public void PlayAsync ()
		{
			foreach (var p in players) p.PlayAsync ();
		}

		public void PauseAsync ()
		{
			foreach (var p in players) p.PauseAsync ();
		}

		public void SeekAsync (int ticks)
		{
			foreach (var p in players) p.SeekAsync (ticks);
		}

		public double TempoChangeRatio {
			get => players.First ().TempoChangeRatio;
			set { foreach (var p in players) p.TempoChangeRatio = value; }
		}

		public int Bpm => players.First ().Bpm;
		public byte [] TimeSignature => players.First ().TimeSignature;
		public TimeSpan PositionInTime => players.First ().PositionInTime;
		public int PlayDeltaTime => players.First ().PlayDeltaTime;
	}

	public class MidiMusicPlayer : IMusicPlayer
	{
		MidiPlayer player;
		private double _tempoChangeRatio;
		private TimeSpan _positionInTime;

		public MidiMusicPlayer (MidiMusic music, IMidiOutput output)
		{
			this.player = new MidiPlayer (music, output);
		}

		public int Bpm => player.Bpm;

		public double TempoChangeRatio {
			get => player.TempoChangeRatio;
			set => player.TempoChangeRatio = value;
		}

		public byte [] TimeSignature => player.TimeSignature;

		public TimeSpan PositionInTime => player.PositionInTime;

		public int PlayDeltaTime => player.PlayDeltaTime;

		public PlayerState State => player.State;

		public void Dispose () => player.Dispose ();

		public event Action Finished {
			add { player.Finished += value; }
			remove { player.Finished -= value; }
		}

		public void PlayAsync () => player.PlayAsync ();

		public void PauseAsync () => player.PauseAsync ();

		public void SeekAsync (int ticks) => player.SeekAsync (ticks);

		public event MidiEventAction EventReceived {
			add { player.EventReceived += value; }
			remove { player.EventReceived -= value; }
		}
	}	
}
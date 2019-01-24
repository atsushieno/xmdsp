using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Music.Midi;

namespace Xmdsp
{
	public partial class Presenter
	{
		public int MaxChannels { get; set; }

		public MainWindowPresenter MainWindow { get; private set; }
		public PalletteDefinition Pallette { get; private set; }
		public KeyboardListPresenter KeyboardList { get; private set; }
		public KeyboardPresenter Keyboard { get; private set; }
		public KeyboardParameterBlockPresenter KeyboardParameterBlock { get; private set; }

		public ApplicationHeaderPanePresenter ApplicationHeaderPane { get; private set; }
		public PlayerStatusMonitorPresenter PlayerStatusMonitor { get; private set; }
		public PlayTimeStatusMonitorPresenter PlayTimeStatusMonitor { get; private set; }

		public KeyOnMeterListPresenter KeyOnMeterList { get; private set; }
		public CircularProgressMeterPresenter CircularProgressMeter { get; private set; }
		public PseudoSpectrumAnalyzerPresenter PseudoSpectrumAnalyzer { get; private set; }

		public event MidiEventAction MidiMessageReceived;

		public Presenter (Model model)
		{
			Model = model;
			model.MidiMessageReceived += OnMessageReceived;
			MainWindow = new MainWindowPresenter (this);
			Pallette = new PalletteDefinition (this);
			Keyboard = new KeyboardPresenter (this);
			KeyboardParameterBlock = new KeyboardParameterBlockPresenter (this);
			KeyboardList = new KeyboardListPresenter (this);
			ApplicationHeaderPane = new ApplicationHeaderPanePresenter (this);
			PlayerStatusMonitor = new PlayerStatusMonitorPresenter (this);
			PlayTimeStatusMonitor = new PlayTimeStatusMonitorPresenter (this);
			KeyOnMeterList = new KeyOnMeterListPresenter (this);
			CircularProgressMeter = new CircularProgressMeterPresenter (this);

			MaxChannels = 16;

			Scale = 1.0;
		}

		public Model Model { get; private set; }
		double scale;
		public double Scale {
			get { return scale; }
			set {
				scale = value;
				if (ScaleChanged != null)
					ScaleChanged ();
			}
		}

		public event Action ScaleChanged;

		void OnMessageReceived (MidiEvent m)
		{
			if (MidiMessageReceived != null)
				MidiMessageReceived (m);
		}

		public class MainWindowPresenter
		{
			Presenter pm;

			public MainWindowPresenter (Presenter pm)
			{
				this.pm = pm;
			}

			public int Width => 840;
			public int Height => 600;
			public int Padding => 0;
		}

		public class KeyboardListPresenter
		{
			Presenter pm;

			public KeyboardListPresenter (Presenter pm)
			{
				this.pm = pm;
			}

			public int Width
			{
				get { return Math.Max (pm.Keyboard.Width, pm.KeyboardParameterBlock.Width); }
			}

			public int Height
			{
				get { return pm.MaxChannels * (pm.Keyboard.Height + pm.KeyboardParameterBlock.Height); }
			}
		}

		public class KeyboardPresenter
		{
			Presenter pm;

			public KeyboardPresenter (Presenter pm)
			{
				this.pm = pm;
				MaxKeys = 128;
				VisibleOctaves = (MaxKeys / 12) - 3;
				VisibleMinOctave = 1;

				WhiteKeyWidth = 7;
				WhiteKeyHeight = 18;
				BlackKeyWidth = WhiteKeyWidth - 1;
				BlackKeyHeight = 10;
				BlackKeyShiftWidth = WhiteKeyWidth / 2;
			}

			public int Width
			{
				get { return WhiteKeyWidth * 7 * (MaxKeys / 12 - VisibleOctaves); }
			}

			public int Height
			{
				get { return WhiteKeyHeight + 2; }
			}

			public int MaxKeys { get; private set; }
			public int VisibleOctaves { get; private set; }
			public int VisibleMinOctave { get; private set; }

			public int WhiteKeyWidth { get; private set; }
			public int BlackKeyWidth { get; private set; }
			public int WhiteKeyHeight { get; private set; }
			public int BlackKeyHeight { get; private set; }
			public int BlackKeyShiftWidth { get; private set; }

			public IEnumerable<Rectangle> WhiteKeyRectangles ()
			{
				int octaves = MaxKeys / 12;
				int width = WhiteKeyWidth;
				int height = WhiteKeyHeight;
				for (int oct = 0; oct < octaves; oct++)
					for (int wkey = 0; wkey < 7; wkey++)
						yield return new Rectangle ((oct * 7 + wkey) * width, 0, width, height);
			}

			static readonly bool [] is_blackkey = { false, true, false, true, false, false, true, false, true, false, true, false };
			static readonly int [] key_pos = { 0, 0, 1, 1, 2, 3, 3, 4, 4, 5, 5, 6 };

			public bool IsBlackKey (int key)
			{
				return is_blackkey [key % 12];
			}

			public int GetWhiteKeyX (int key)
			{
				int oct = key / 12;
				int wkey = key_pos [key % 12];
				return (oct * 7 + wkey) * WhiteKeyWidth;
			}

			public Rectangle GetWhiteKeyRect (int key)
			{
				int oct = key / 12;
				int wkey = key % 12 - 1;
				int width = WhiteKeyWidth;
				int height = WhiteKeyHeight;
				return new Rectangle ((oct * 7 + wkey) * width, 0, width, height);
			}

			public Rectangle GetBlackKeyRect (int key)
			{
				int oct = key / 12;
				int wkey = key % 12 - 1;
				int wwidth = WhiteKeyWidth;
				int bwidth = BlackKeyWidth;
				int height = BlackKeyHeight;
				return new Rectangle ((oct * 7 + wkey) * wwidth + BlackKeyShiftWidth, 0, bwidth, height);
			}

			static readonly bool [] has_sharp = { true, true, false, true, true, true, false };

			public IEnumerable<Rectangle> BlackKeyRectangles ()
			{
				int octaves = MaxKeys / 12;
				int wwidth = WhiteKeyWidth;
				int bwidth = BlackKeyWidth;
				int height = BlackKeyHeight;
				for (int oct = 0; oct < octaves; oct++)
					for (int wkey = 0; wkey < 7; wkey++)
						if (has_sharp [wkey])
							yield return new Rectangle ((oct * 7 + wkey) * wwidth + BlackKeyShiftWidth, 0, bwidth, height);
			}
		}

		public class KeyboardParameterBlockPresenter
		{
			Presenter pm;

			public KeyboardParameterBlockPresenter (Presenter pm)
			{
				this.pm = pm;
				KeyBlockHeaderTextSize = 8;
				KeyBlockParameterTextSize = 8;
				KeyBlockChannelNumberTextSize = 14;
			}

			public int Width
			{
				get { return ColumnPositions [ColumnPositions.Length - 1]; }
			}

			public int Height
			{
				get { return 2 + Math.Max (Math.Max (KeyBlockParameterTextSize * 2 + 2, KeyBlockHeaderTextSize * 2 + 2), KeyBlockChannelNumberTextSize + 2); }
			}

			public int KeyBlockParameterTextSize { get; private set; }
			public int KeyBlockHeaderTextSize { get; private set; }
			public int KeyBlockChannelNumberTextSize { get; private set; }

			public int [] ColumnPositions = { 0, 100, 160, 220, 320, 360 };
		}

		public class ApplicationHeaderPanePresenter
		{
			Presenter pm;

			public ApplicationHeaderPanePresenter (Presenter pm)
			{
				this.pm = pm;
				ApplicationNameTextSize = 24;
			}

			public int ApplicationNameTextSize { get; private set; }

			public int Width => 250;

			public int Height => ApplicationNameTextSize + 2 + 12;
		}

		public class PlayerStatusMonitorPresenter
		{
			Presenter pm;

			public PlayerStatusMonitorPresenter (Presenter pm)
			{
				this.pm = pm;
				BaseIconSize = 16;
				TextSize = 9;
			}

			public int BaseIconSize { get; private set; }

			public int Width
			{
				get { return Math.Max (140, (BaseIconSize + 4) * 4); }
			}

			public int Height
			{
				get { return BaseIconSize + 4; }
			}

			public int TextSize { get; private set; }
		}

		public class PlayTimeStatusMonitorPresenter
		{
			Presenter pm;

			public PlayTimeStatusMonitorPresenter (Presenter pm)
			{
				this.pm = pm;
				ItemHeight = 30;
			}

			public int ItemHeight { get; set; }

			public int Width
			{
				get { return 180; }
			}

			public int Height
			{
				get { return ItemHeight * 5; }
			}
		}

		public class KeyOnMeterListPresenter
		{
			Presenter pm;

			public KeyOnMeterListPresenter (Presenter pm)
			{
				this.pm = pm;

				KeyOnMeterTextSize = 8;
				LineGapSize = 2;

				ItemWidth = 22;
				ItemHeight = 70;
				MetersOffset = KeyOnMeterTextSize * 3;
				MeterWidth = 18;
				MeterHeight = 64;
				PanpotOuterRadius = 8;
				PanpotInnerObjectRadius = 6;
				PanpotOffset = 4;
			}

			public int TotalProgressSteps => 16;

			public int Width
			{
				get { return MetersOffset + ItemWidth * pm.MaxChannels; }
			}

			public int Height
			{
				get { return ItemHeight + KeyOnMeterTextSize * 2 + LineGapSize + PanpotOuterRadius * 2 + PanpotOffset; }
			}

			public int ItemWidth { get; private set; }
			public int ItemHeight { get; private set; }

			public int MetersOffset { get; private set; }
			public int MeterWidth { get; private set; }
			public int MeterHeight { get; private set; }
			public int KeyOnMeterTextSize { get; private set; }
			public int LineGapSize { get; private set; }
			public int PanpotOuterRadius { get; private set; }
			public int PanpotInnerObjectRadius { get; private set; }
			public int PanpotOffset { get; private set; }
		}

		public class CircularProgressMeterPresenter
		{
			private Presenter pm;

			public CircularProgressMeterPresenter (Presenter pm)
			{
				this.pm = pm;
			}

			public int Width => 60;
			public int Height => 60;

			public int Padding = 32;
		}

		public class PseudoSpectrumAnalyzerPresenter
		{
			public int Width => 300;
			public int Height => 240;

			public int LineThickness => 1;
			public int CellWidth => 16;
			public int CellHeight => 8;
		}

		public class PalletteDefinition
		{
			private Presenter pm;

			public PalletteDefinition (Presenter pm)
			{
				this.pm = pm;
				ApplicationBackgroundColor = Color.Black;
				KeyboardBackgroundColor = Color.Transparent;
				WhiteKeyFillColor = Color.White;
				WhiteKeyStrokeColor = Color.Black;
				BlackKeyFillColor = Color.Black;
				BlackKeyStrokeColor = Color.White.Darken (0.5);

				NoteOnColor = new Color (255, 255, 0);

				var baseColor = new Color (128, 128, 255);
				KeyParameterBackgroundColor = Color.Transparent;
				CommonTextDarkest = baseColor.Darken (0.5);
				CommonTextMiddle = baseColor.Darken (0.75);
				CommonTextBlightest = Color.White;

				PlayerStateInactiveBackground = Color.Black;
				PlayerStateActiveBackground = CommonTextMiddle;
				PlayerStateInactiveStroke = CommonTextDarkest;
				PlayerStateActiveStroke = baseColor;
			}

			//public event Action PalletteChanged;

			public Color ApplicationBackgroundColor { get; private set; }
			public Color KeyboardBackgroundColor { get; private set; }
			public Color WhiteKeyFillColor { get; private set; }
			public Color WhiteKeyStrokeColor { get; private set; }
			public Color BlackKeyFillColor { get; private set; }
			public Color BlackKeyStrokeColor { get; private set; }

			public Color NoteOnColor { get; private set; }

			public Color KeyParameterBackgroundColor { get; private set; }

			public Color PlayerStateActiveBackground { get; private set; }
			public Color PlayerStateInactiveBackground { get; private set; }
			public Color PlayerStateActiveStroke { get; private set; }
			public Color PlayerStateInactiveStroke { get; private set; }

			public Color CommonTextDarkest { get; private set; }
			public Color CommonTextMiddle { get; private set; }
			public Color CommonTextBlightest { get; private set; }
		}

		public void SeekByPercent (double progressSliderValue)
		{
			Model.SeekByPercent (progressSliderValue);
		}

		public void SeekByDeltaTime (int deltaTime)
		{
			Model.SeekByDeltaTime (deltaTime);
		}
	}
}

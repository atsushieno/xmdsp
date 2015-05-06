using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Music.Midi;

namespace Xmdsp
{
	public partial class ViewModel
	{
		public int MaxChannels { get; set; }

		public PalletteDefinition Pallette { get; private set; }
		public KeyboardListViewModel KeyboardList { get; private set; }
		public KeyboardViewModel Keyboard { get; private set; }
		public KeyboardParameterBlockViewModel KeyboardParameterBlock { get; private set; }

		public ApplicationHeaderPaneViewModel ApplicationHeaderPane { get; private set; }
		public PlayerStatusMonitorViewModel PlayerStatusMonitor { get; private set; }
		public PlayTimeStatusMonitorViewModel PlayTimeStatusMonitor { get; private set; }

		public KeyOnMeterListViewModel KeyOnMeterList { get; private set; }

		public event MidiEventAction MidiMessageReceived;

		public ViewModel (Model model)
		{
			Model = model;
			model.MidiMessageReceived += OnMessageReceived;
			Pallette = new PalletteDefinition ();
			Keyboard = new KeyboardViewModel (this);
			KeyboardParameterBlock = new KeyboardParameterBlockViewModel (this);
			KeyboardList = new KeyboardListViewModel (this);
			ApplicationHeaderPane = new ApplicationHeaderPaneViewModel (this);
			PlayerStatusMonitor = new PlayerStatusMonitorViewModel (this);
			PlayTimeStatusMonitor = new PlayTimeStatusMonitorViewModel (this);
			KeyOnMeterList = new KeyOnMeterListViewModel (this);

			MaxChannels = 16;
		}

		public Model Model { get; private set; }

		void OnMessageReceived (SmfEvent m)
		{
			if (MidiMessageReceived != null)
				MidiMessageReceived (m);
		}

		public class KeyboardListViewModel
		{
			ViewModel vm;

			public KeyboardListViewModel (ViewModel vm)
			{
				this.vm = vm;
			}

			public int Width
			{
				get { return Math.Max (vm.Keyboard.Width, vm.KeyboardParameterBlock.Width); }
			}

			public int Height
			{
				get { return vm.MaxChannels * (vm.Keyboard.Height + vm.KeyboardParameterBlock.Height); }
			}
		}

		public class KeyboardViewModel
		{
			ViewModel vm;

			public KeyboardViewModel (ViewModel vm)
			{
				this.vm = vm;
				MaxKeys = 128;
				VisibleOctaves = (MaxKeys / 12) - 0;

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

		public class KeyboardParameterBlockViewModel
		{
			ViewModel vm;

			public KeyboardParameterBlockViewModel (ViewModel vm)
			{
				this.vm = vm;
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

		public class ApplicationHeaderPaneViewModel
		{
			ViewModel vm;

			public ApplicationHeaderPaneViewModel (ViewModel vm)
			{
				this.vm = vm;
				ApplicationNameTextSize = 24;
			}

			public int ApplicationNameTextSize { get; private set; }

			public int Width {
				get { return 300; }
			}
			
			public int Height {
				get { return ApplicationNameTextSize + 2 + 24; }
			}
		}

		public class PlayerStatusMonitorViewModel
		{
			ViewModel vm;

			public PlayerStatusMonitorViewModel (ViewModel vm)
			{
				this.vm = vm;
				BaseIconSize = 16;
			}

			public int BaseIconSize { get; private set; }

			public int Width {
				get { return Math.Max (140, (BaseIconSize + 4) * 4); }
			}

			public int Height {
				get { return BaseIconSize + 4; }
			}
		}

		public class PlayTimeStatusMonitorViewModel
		{
			ViewModel vm;

			public PlayTimeStatusMonitorViewModel (ViewModel vm)
			{
				this.vm = vm;
				ItemHeight = 30;
			}

			public int ItemHeight { get; set; }

			public int Width {
				get { return 200; }
			}

			public int Height {
				get { return ItemHeight * 5; }
			}
		}

		public class KeyOnMeterListViewModel
		{
			ViewModel vm;

			public KeyOnMeterListViewModel (ViewModel vm)
			{
				this.vm = vm;

				KeyOnMeterTextSize = 8;
				LineGapSize = 2;

				ItemWidth = 22;
				ItemHeight = 70;
				MetersOffset = KeyOnMeterTextSize * 3;
				MeterWidth = 18;
				MeterHeight = 64;
			}

			public int Width {
				get { return MetersOffset + ItemWidth * vm.MaxChannels; }
			}
			public int Height {
				get { return ItemHeight + KeyOnMeterTextSize * 2 + LineGapSize; }
			}

			public int ItemWidth { get; private set; }
			public int ItemHeight { get; private set; }

			public int MetersOffset { get; private set; }
			public int MeterWidth { get; private set; }
			public int MeterHeight { get; private set; }
			public int KeyOnMeterTextSize { get; private set; }
			public int LineGapSize { get; private set; }
		}

		public class PalletteDefinition
		{
			public PalletteDefinition ()
			{
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
	}
}

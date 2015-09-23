using System;
using Commons.Music.Midi;
using Xwt;
using Xwt.Drawing;

namespace Xmdsp
{
	public class MainWindow : Window
	{
		readonly Model model;
		readonly ViewModel vm;
		
		public MainWindow ()
		{
			model = new Model ();
			vm = new ViewModel (model);
			
			Title = "xmdsp";
			// FIXME: calculate sizes
			Width = 840;
			Height = 600;
			Padding = 0;
			Icon = Image.FromResource (GetType ().Assembly, "xmdsp_icon.png");
			SetupMenu ();
			
			this.CloseRequested += delegate { ShutdownApplication (); };
			
			var mainPane = new HBox () { BackgroundColor = vm.Pallette.ApplicationBackgroundColor.ToXwt () };
			var rightPane = new VBox () { BackgroundColor = vm.Pallette.ApplicationBackgroundColor.ToXwt () };
			rightPane.PackStart (new ApplicationHeaderPane (vm), false);
			var rightSecondPane = new HBox () { BackgroundColor = vm.Pallette.ApplicationBackgroundColor.ToXwt () };
			rightSecondPane.PackStart (new CircularProgressMeter (vm), false);
			rightSecondPane.PackStart (new PlayerStatusMonitor (vm), false);
			rightSecondPane.PackStart (new PlayTimeStatusMonitor (vm), false);
			rightPane.PackStart (rightSecondPane, false);
			rightPane.PackStart (new KeyOnMeterList (vm), false);
			
			mainPane.PackStart (new KeyboardList (vm), true);
			mainPane.PackStart (rightPane, true);
			Content = mainPane;
		}
		
		void ShutdownApplication ()
		{
			model.Dispose ();
			Application.Exit ();			
		}
		
		void SetupMenu ()
		{
			Menu menu = new Menu ();
			
			var file = new MenuItem ("_File");
			file.SubMenu = new Menu ();
			
			MenuItem open = new MenuItem ("_Open");
			open.Clicked += delegate {
				var dlg = new OpenFileDialog ();
				dlg.Filters.Add (new FileDialogFilter ("Standard MIDI Files", "*.mid", "*.MID", "*.Mid"));
				dlg.Filters.Add (new FileDialogFilter ("All Files", "*"));
				if (dlg.Run ()) {
					model.LoadSmf (dlg.FileName);
					model.Play ();
				}
			};
			file.SubMenu.Items.Add (open);
			MenuItem close = new MenuItem ("_Close");
			close.Clicked += delegate { ShutdownApplication (); };
			file.SubMenu.Items.Add (close);
			menu.Items.Add (file);
			
			var device = new MenuItem ("_Device");
			device.SubMenu = new Menu ();
			device.Clicked += delegate {
				device.SubMenu.Items.Clear ();
				foreach (var dev in model.Platform.AllMidiDevices) {
					var dmi = new MenuItem (dev.Name);
					dmi.Clicked += delegate { model.Platform.MidiOutputDeviceId = dev.Id; };
					device.SubMenu.Items.Add (dmi);
				}
			};
			menu.Items.Add (device);
			
			var player = new MenuItem ("_Player");
			player.SubMenu = new Menu ();
			player.Clicked += delegate {
				player.SubMenu.Items.Clear ();
				var state = model.Player == null ? PlayerState.Stopped : model.Player.State;
				switch (state) {
				case PlayerState.Playing:
				case PlayerState.FastForward:
					MenuItem pause = new MenuItem ("_Pause");
					pause.Clicked += delegate { model.Pause (); };
					player.SubMenu.Items.Add (pause);
					break;
				default:
					MenuItem play = new MenuItem ("_Play");
					play.Clicked += delegate { model.Play (); };
					player.SubMenu.Items.Add (play);
					break;
				}
				MenuItem stop = new MenuItem ("_Stop");
				stop.Clicked += delegate { model.Stop (); };
				player.SubMenu.Items.Add (stop);
				
				foreach (var item in player.SubMenu.Items)
					item.Sensitive = model.Player != null;
			};
			menu.Items.Add (player);
			
			this.MainMenu = menu;
		}
	}
}


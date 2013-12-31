using System;
using System.Linq;
using Gtk;
using Commons.Music.Midi.Player;

namespace Xmdsp
{
	public class MainWindow : Window
	{
		readonly Model model;
		readonly ViewModel vm;
		
		public MainWindow ()
			: base ("XMDSP")
		{
			model = new Model ();
			vm = new ViewModel (model);
			
			Title = "xmdsp";
			WidthRequest = 800;
			HeightRequest = 660;
			//Icon = Image.LoadFromResource (/*GetType ().Assembly,*/ "xmdsp_icon.png");
			
			this.Destroyed += delegate { ShutdownApplication (); };
			
			var menuPane = new VBox ();
			Add (menuPane);
			
			menuPane.PackStart (SetupMenu (), false, false, 0);
			
			var mainPane = new HBox ();
			menuPane.PackStart (mainPane, true, true, 0);
			
			var rightPane = new VBox ();
			//rightPane.ModifyBase (StateType.Normal, vm.Pallette.ApplicationBackgroundColor.ToGdk ());
			rightPane.Add (new ApplicationHeaderPane (vm));
			var rightSecondPane = new HBox ();
			//rightSecondPane.ModifyBase (StateType.Normal, vm.Pallette.ApplicationBackgroundColor.ToGdk ());
			rightSecondPane.Add (new CircularProgressMeter (vm));
			rightSecondPane.Add (new PlayerStatusMonitor (vm));
			rightSecondPane.Add (new PlayTimeStatusMonitor (vm, null, null));
			rightPane.Add (rightSecondPane);
			rightPane.Add (new KeyOnMeterList (vm));
			
			rightPane.ModifyBase (StateType.Normal, vm.Pallette.ApplicationBackgroundColor.ToGdk ());
			rightPane.ModifyBg (StateType.Normal, vm.Pallette.ApplicationBackgroundColor.ToGdk ());
			
			mainPane.PackStart (new KeyboardList (vm), true, true, 0);
			mainPane.PackStart (rightPane);
		}
		
		void ShutdownApplication ()
		{
			model.Dispose ();
			Application.Quit ();			
		}
		
		Widget SetupMenu ()
		{
			var mb = new MenuBar ();
			
			var file = new MenuItem ("_File");
			var fileMenu = new Menu ();
			file.Submenu = fileMenu;
			
			var open = new MenuItem ("_Open");
			open.Activated += delegate {
				var dlg = new FileChooserDialog ("Select a Standard MIDI file to play", this, FileChooserAction.Open,
					"Cancel", ResponseType.Cancel,
					"OK", ResponseType.Ok);
				var mid = new FileFilter () { Name = "Standard MIDI Files" };
				foreach (string pattern in new String [] {"*.mid", "*.MID", "*.Mid"})
					mid.AddPattern (pattern);
				dlg.AddFilter (mid);
				var all = new FileFilter () { Name = "All Files" };
				all.AddPattern ("*");
				dlg.AddFilter (all);
				if (dlg.Run () == (int) ResponseType.Ok) {
					model.LoadSmf (dlg.Filename);
					model.Play ();
				}
				dlg.Destroy ();
			};
			fileMenu.Add (open);
			MenuItem close = new MenuItem ("_Quit");
			close.Activated += delegate { ShutdownApplication (); };
			fileMenu.Add (close);
			
			mb.Append (file);

			var device = new MenuItem ("_Device");
			var deviceMenu = new Menu ();
			device.Submenu = deviceMenu;
			device.Activated += delegate {
				Console.WriteLine ("DeviceMenuClicked");
				foreach (var c in deviceMenu.Children.ToArray ())
					if (c is MenuItem)
						deviceMenu.Remove (c);
				int nDev = 0;
				foreach (var dev in model.Platform.AllMidiDevices) {
					var dmi = new MenuItem (string.Format ("_{0}: {1}", nDev++, dev.Name));
					dmi.Activated += delegate { model.Platform.MidiOutputDeviceIndex = dev.ID; };
					deviceMenu.Add (dmi);
					dmi.ShowAll ();
				}
			};
			
			mb.Append (device);
			
			var player = new MenuItem ("_Player");
			var playerMenu = new Menu ();
			player.Submenu = playerMenu;
			player.Activated += delegate {
				foreach (var c in playerMenu.Children.ToArray ())
					if (c is MenuItem)
						playerMenu.Remove (c);
				var state = model.Player == null ? PlayerState.Stopped : model.Player.State;
				switch (state) {
				case PlayerState.Playing:
				case PlayerState.FastForward:
					MenuItem pause = new MenuItem ("_Pause");
					pause.Activated += delegate { model.Pause (); };
					playerMenu.Add (pause);
					break;
				default:
					MenuItem play = new MenuItem ("_Play");
					play.Activated += delegate { model.Play (); };
					playerMenu.Add (play);
					break;
				}
				MenuItem stop = new MenuItem ("_Stop");
				stop.Activated += delegate { model.Stop (); };
				playerMenu.Add (stop);
				
				foreach (var c in playerMenu.Children.ToArray ())
					if (c is MenuItem)
						((MenuItem) c).Sensitive = model.Player != null;
			};
			
			mb.Append (player);
			
			return mb;
		}
	}
}


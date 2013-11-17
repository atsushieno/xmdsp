using System;
using Xwt;

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
			Width = 800;
			Height = 600;
			Padding = 0;

			SetupMenu ();
			
			this.CloseRequested += delegate { ShutdownApplication (); };
			
			HBox mainPane = new HBox () { BackgroundColor = vm.Pallette.ApplicationBackgroundColor.ToXwt () };
			mainPane.PackStart (new KeyboardList (vm), true);
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
				Console.WriteLine ("DeviceMenuClicked");
				device.SubMenu.Items.Clear ();
				foreach (var dev in model.Platform.AllMidiDevices) {
					var dmi = new MenuItem (dev.Name);
					dmi.Clicked += delegate { model.Platform.MidiOutputDeviceIndex = dev.ID; };
					device.SubMenu.Items.Add (dmi);
				}
			};
			menu.Items.Add (device);
			
			this.MainMenu = menu;
		}
	}
}


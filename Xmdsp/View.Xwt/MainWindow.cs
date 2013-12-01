using System;
using System.Linq;
using Gtk;

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
			
			WidthRequest = 800;
			HeightRequest = 600;

			VBox mainPane = new VBox ()/* { BackgroundColor = vm.Pallette.ApplicationBackgroundColor.ToXwt () }*/;
			Add (mainPane);
			mainPane.PackStart (SetupMenu (), false, false, 0);
			
			this.Destroyed += delegate { ShutdownApplication (); };
			
			mainPane.PackStart (new KeyboardList (vm), true, true, 0);
		}
		
		void ShutdownApplication ()
		{
			model.Dispose ();
			Application.Quit ();			
		}
		
		Widget SetupMenu ()
		{
			MenuBar mb = new MenuBar ();
			var file = new MenuItem ("_File");
			var fileMenu = new Menu ();
			file.Submenu = fileMenu;
			
			MenuItem open = new MenuItem ("_Open");
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

			return mb;
		}
	}
}

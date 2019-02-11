using System;
using System.Text;
using System.Timers;
using Commons.Music.Midi;
using Xwt;
using Xwt.Drawing;

namespace Xmdsp
{
	public class MainWindow : Window
	{
		readonly Model model;
		readonly Presenter pm;

		bool suspend_slider_updates;
		
		public MainWindow ()
		{
			model = new Model (new DesktopPlatformLayer ());
			model.Report += msg => Console.Error.WriteLine (msg); // HACK
			pm = new Presenter (model);
			var pmw = pm.MainWindow;

			double initialWidth, initialHeight;

			Title = "xmdsp";
			// FIXME: calculate sizes
			initialWidth = Width = pmw.Width * pm.Scale;
			initialHeight = Height = pmw.Height * pm.Scale;
			Padding = pmw.Padding * pm.Scale;
			Icon = Image.FromResource (GetType ().Assembly, "xmdsp_icon.png");
			SetupMenu ();
			
			this.CloseRequested += delegate { ShutdownApplication (); };
			
			var mainPane = new HBox () { BackgroundColor = pm.Pallette.ApplicationBackgroundColor.ToXwt () };
			var rightPane = new VBox () { BackgroundColor = pm.Pallette.ApplicationBackgroundColor.ToXwt () };
			app_header_pane = new ApplicationHeaderPane (pm);
			rightPane.PackStart (app_header_pane, false);
			
			/* disable it until the issue that the slider jumps around gets fixed...
			var progressSlider = new HSlider () { MaximumValue = 100.0, ExpandHorizontal = true };
			progressSlider.ButtonPressed += (o, e) => suspend_slider_updates = true;
			progressSlider.ButtonReleased += delegate {
				var pos = progressSlider.Value;
				suspend_slider_updates = false;
				pm.SeekByPercent (pos);
			};
			pm.MidiMessageReceived += delegate {
					Application.Invoke (() => {
						if (!suspend_slider_updates)
							progressSlider.Value = pm.Model.PlayerProgress * 100;
					});
			};
			rightPane.PackStart (progressSlider, false);
			*/
			
			var rightSecondPane = new HBox () { BackgroundColor = pm.Pallette.ApplicationBackgroundColor.ToXwt () };
			circular_progress = new CircularProgressMeter (pm);
			rightSecondPane.PackStart (circular_progress, false);
			player_status = new PlayerStatusMonitor (pm);
			rightSecondPane.PackStart (player_status, false);
			play_time_status_monitor = new PlayTimeStatusMonitor (pm);
			rightSecondPane.PackStart (play_time_status_monitor, false);
			rightPane.PackStart (rightSecondPane, false);
			key_on_meter_list = new KeyOnMeterList (pm);
			rightPane.PackStart (key_on_meter_list, false);

			keyboard_list = new KeyboardList (pm);
			mainPane.PackStart (keyboard_list, true);
			mainPane.PackStart (rightPane, true);

			mainPane.SetDragDropTarget (TransferDataType.Uri);
			mainPane.DragOver += (sender, e) => {
				if (e.Action == DragDropAction.All)
					e.AllowedAction = DragDropAction.Move;
				else
					e.AllowedAction = e.Action;
			};
			mainPane.DragDrop += (sender, e) => {
				if (e.Data.Uris != null) {
					foreach (var uri in e.Data.Uris) {
						if (uri.IsFile) {
							e.Success = true;
							model.LoadSmf (uri.LocalPath);
							model.Play ();
							break;
						}
					}
				}
			};

			Content = mainPane;

			this.BoundsChanged += (sender, e) => pm.Scale = Math.Min (Width / initialWidth, Height / initialHeight);
		}

		ApplicationHeaderPane app_header_pane;
		CircularProgressMeter circular_progress;
		KeyboardList keyboard_list;
		KeyOnMeterList key_on_meter_list;
		PlayerStatusMonitor player_status;
		PlayTimeStatusMonitor play_time_status_monitor;
		
		void ShutdownApplication ()
		{
			model.Stop ();
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
			var watchUnwatch = new CheckBoxMenuItem ("_Watch and restart for file change") { Checked = true };
			watchUnwatch.Clicked += (sender, e) => model.Platform.WatchFileChanges = watchUnwatch.Checked;
			file.SubMenu.Items.Add (watchUnwatch);
			MenuItem close = new MenuItem ("_Close");
			close.Clicked += delegate { ShutdownApplication (); };
			file.SubMenu.Items.Add (close);
			menu.Items.Add (file);

			var view = new MenuItem ("_View");
			view.SubMenu = new Menu ();
			var viewKeyboards = new CheckBoxMenuItem ("_Keyboards") {Checked = true};
			viewKeyboards.Clicked += delegate {
				viewKeyboards.Checked = !pm.KeyboardList.Visible;
				pm.KeyboardList.Visible = keyboard_list.Visible = viewKeyboards.Checked;
			};
			view.SubMenu.Items.Add (viewKeyboards);
			var viewHeader = new CheckBoxMenuItem ("Application _Header Pane") {Checked = true};
			viewHeader.Clicked += delegate {
				viewHeader.Checked = !pm.ApplicationHeaderPane.Visible;
				pm.ApplicationHeaderPane.Visible = app_header_pane.Visible = viewHeader.Checked;
			};
			view.SubMenu.Items.Add (viewHeader);
			var viewCircle = new CheckBoxMenuItem ("_Circular Progress") {Checked = true};
			viewCircle.Clicked += delegate {
				viewCircle.Checked = !pm.CircularProgressMeter.Visible;
				pm.CircularProgressMeter.Visible = circular_progress.Visible = viewCircle.Checked;
			};
			view.SubMenu.Items.Add (viewCircle);
			var viewPlayStat = new CheckBoxMenuItem ("Player _Statuses") {Checked = true};
			viewPlayStat.Clicked += delegate {
				viewPlayStat.Checked = !pm.PlayerStatusMonitor.Visible;
				pm.PlayerStatusMonitor.Visible = player_status.Visible = viewPlayStat.Checked;
			};
			view.SubMenu.Items.Add (viewPlayStat);
			menu.Items.Add (view);
			var viewPlayTime = new CheckBoxMenuItem ("Play _Time Monitor") {Checked = true};
			viewPlayTime.Clicked += delegate {
				viewPlayTime.Checked = !pm.PlayTimeStatusMonitor.Visible;
				pm.PlayTimeStatusMonitor.Visible = play_time_status_monitor.Visible = viewPlayTime.Checked;
			};
			view.SubMenu.Items.Add (viewPlayTime);
			var viewKeyOnMeters = new CheckBoxMenuItem ("Key-On _Meters") {Checked = true};
			viewKeyOnMeters.Clicked += delegate {
				viewKeyOnMeters.Checked = !pm.KeyOnMeterList.Visible;
				pm.KeyOnMeterList.Visible = key_on_meter_list.Visible = viewKeyOnMeters.Checked;
			};
			view.SubMenu.Items.Add (viewKeyOnMeters);
			menu.Items.Add (view);

			var device = new MenuItem ("_Device");
			device.SubMenu = new Menu ();
			device.Clicked += delegate {
				device.SubMenu.Items.Clear ();
				foreach (var dev in model.Platform.AllMidiDevices) {
					var dmi = new CheckBoxMenuItem(dev.Name);
					dmi.Checked = dev.Id == model.MidiOutputDeviceId;
					dmi.Clicked += delegate { model.MidiOutputDeviceId = dev.Id; };
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

				foreach (var ratio in new double [] { 1, 2, 4, 8}) {
					MenuItem ff = new MenuItem ($"FF: {ratio}x");
					ff.Clicked += delegate { model.ProcessChangeTempoRatio (ratio); };
					player.SubMenu.Items.Add (ff);
				}

				MenuItem marker = new MenuItem ("_Markers");
				marker.SubMenu = new Menu ();
				if (model.Music != null) {
					int i = 0;
					foreach (var m in model.Music.GetMetaEventsOfType (MidiMetaType.Marker)) {
						var markerText = "";
						try {
							markerText = Encoding.Default.GetString (m.Event.Data);
						} catch (ArgumentException) {
							markerText = Encoding.UTF8.GetString (m.Event.Data);
						}
						var milliseconds = model.Music.GetTimePositionInMillisecondsForTick (m.DeltaTime);
						var item = new MenuItem (TimeSpan.FromMilliseconds (milliseconds).ToString ("mm':'ss'.'fff") + " : " + markerText);
						// FIXME: add seek operation.
						item.Clicked += delegate { pm.SeekByDeltaTime (m.DeltaTime); };
						marker.SubMenu.Items.Add (item);
					}
				}
				player.SubMenu.Items.Add(marker);
			};
			menu.Items.Add (player);
			
			this.MainMenu = menu;
		}
	}
}


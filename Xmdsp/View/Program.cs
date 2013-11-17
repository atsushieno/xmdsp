using System;
using Xwt;

namespace Xmdsp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			ToolkitType type;
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Unix:
				type = ToolkitType.Gtk;
				break;
			case PlatformID.MacOSX:
				type = ToolkitType.Cocoa;
				break;
			default:
				type = ToolkitType.Wpf;
				break;
			}
			Application.Initialize (type);
			
			MainWindow w = new MainWindow ();
			w.Show ();
			
			Application.Run ();
			
			w.Dispose ();

			Application.Dispose ();
		}
	}
}

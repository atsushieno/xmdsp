using System;
using Gtk;

namespace Xmdsp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			
			MainWindow w = new MainWindow ();
			w.ShowAll ();
			
			Application.Run ();
			
			w.Dispose ();
		}
	}
}

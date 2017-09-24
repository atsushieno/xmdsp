using System;
using Xwt;

namespace Xmdsp
{
	class MainClass
	{
	        [STAThread]
	        public static void Main (string[] args)
		{
			Application.Initialize ();
			
			MainWindow w = new MainWindow ();
			w.Show ();
			
			Application.Run ();
			
			w.Dispose ();

			Application.Dispose ();
		}
	}
}

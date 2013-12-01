using System;
using Gtk;

namespace Xmdsp
{
	public static class XwtViewModelExtensions
	{
		public static Gdk.Color ToGdk (this ViewModel.Color color)
		{
			// no alpha channel?
			return new Gdk.Color (color.R, color.G, color.B);
		}
	}
}


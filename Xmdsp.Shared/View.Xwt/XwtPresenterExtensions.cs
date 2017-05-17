using System;
using Xwt;

namespace Xmdsp
{
	public static class XwtPresenterExtensions
	{
		public static Xwt.Drawing.Color ToXwt (this Presenter.Color color)
		{
			return Xwt.Drawing.Color.FromBytes (color.R, color.G, color.B, color.A);
		}
	}
}


using System;
using SkiaSharp;

namespace Xmdsp
{
	public static class XwtPresenterExtensions
	{
		public static SKColor ToSkia (this Presenter.Color color)
		{
			return new SKColor (color.R, color.G, color.B, color.A);
		}

		public static Xamarin.Forms.Color ToForms (this Presenter.Color color)
		{
			return Xamarin.Forms.Color.FromRgba (color.R, color.G, color.B, color.A);
		}
	}
}


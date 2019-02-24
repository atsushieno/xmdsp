using System;
using System.Linq;
using Commons.Music.Midi;
using Xwt;
using Xwt.Drawing;

namespace Xmdsp
{
	public class QueuedFileList : Canvas
	{
		Presenter pm;

		public QueuedFileList (Presenter pm)
		{
			this.pm = pm;

			pm.ScaleChanged += SetSize;

			font_label = font_label ?? this.Font.WithSize (10);
			font_value = font_value ?? Font.SystemMonospaceFont.WithSize (16);
		}
		
		Font font_label;
		Font font_value;

		internal void SetSize ()
		{
			WidthRequest = pm.QueuedFileList.Width * pm.Scale;
			HeightRequest = pm.QueuedFileList.Height * pm.Scale;
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			if (!pm.QueuedFileList.Visible)
				return;

			base.OnDraw (ctx, dirtyRect);
			ctx.Scale (pm.Scale, pm.Scale);

			var color = pm.Pallette.CommonTextDarkest;
			ctx.SetColor (color.ToXwt ());
			ctx.SetLineWidth (1);

			var size = pm.QueuedFileList.TextSize;
			var height = pm.QueuedFileList.TextSize + pm.QueuedFileList.LineGap;

			if (pm.Model.Queue.QueuedFiles.Any ()) {
				DrawingHelper.DrawText (ctx, font_label, pm.QueuedFileList.LabelTextSize, color,
					"- No. -", pm.QueuedFileList.TrackNumberXOffset, 0);
				DrawingHelper.DrawText (ctx, font_label, pm.QueuedFileList.LabelTextSize, color,
					"- Title -", pm.QueuedFileList.TitleXOffset, 0);
				for (int i = 0; i < 10; i++) {
					if (i >= pm.Model.Queue.QueuedFiles.Count)
						break;
					var file = pm.Model.Queue.QueuedFiles [i];
					DrawingHelper.DrawText (ctx, font_label, size, color, (i + 1).ToString (),
						pm.QueuedFileList.TrackNumberXOffset, (i + 1) * height);
					DrawingHelper.DrawText (ctx, font_label, size, color, file.Title,
						pm.QueuedFileList.TitleXOffset, (i + 1) * height);
				}
			}
		}
	}
}

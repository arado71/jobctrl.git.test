using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	public class HighlightPanel : Panel
	{
		public Color HighlightColor { get; set; }
		public Rectangle HighlightRectangle { get; set; }

		private SolidBrush highlightBrush;

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);
			if (HighlightRectangle == Rectangle.Empty) return;
			if (highlightBrush == null || highlightBrush.Color != HighlightColor)
			{
				using (highlightBrush) { } //Dispose
				highlightBrush = new SolidBrush(HighlightColor);
			}
			e.Graphics.FillRectangle(highlightBrush, HighlightRectangle);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && highlightBrush != null)
			{
				highlightBrush.Dispose();
				highlightBrush = null;
			}
			base.Dispose(disposing);
		}
	}
}

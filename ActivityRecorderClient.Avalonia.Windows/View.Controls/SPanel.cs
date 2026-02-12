using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View.Controls
{
	/// <summary>
	/// Rounded "smooth" panel based on https://stackoverflow.com/questions/38632035/winforms-smooth-the-rounded-edges-for-panel
	/// </summary>
	public class SPanel : Panel
	{
		private const int hIndent = 1;
		private const int vIndent = 1;

		private int radius = 5;
		private int border = 2;
		private Color borderColor = DefaultForeColor;

		[Category("Appearance")]
		[Description("The radius of the panel's corners.")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		public int Radius { get { return radius; } set { if (radius == value) return; radius = value; /*Invalidate();*/ } }

		[Category("Appearance")]
		[Description("The border witdh around panel")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		public int Border { get { return border; } set { if (border == value) return; border = value; /*Invalidate();*/ } }
		[Category("Appearance")]
		[Description("The border witdh around panel")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		public Color BorderColor { get { return borderColor; } set { if (borderColor == value) return; borderColor = value; /*Invalidate();*/ } }

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			if (radius == 0) return;
			using (var parentBrush = new SolidBrush(Parent?.BackColor ?? BackColor))
			using (var brush = new SolidBrush(BackColor))
			using (var pen = new Pen(borderColor, border))
			using (var g = e.Graphics)
			{
				var origClip = e.ClipRectangle;
				g.SetClip(new Rectangle(hIndent + radius, vIndent + radius, this.Width - 2 * hIndent - 2 * radius - 2, this.Height - 2 * vIndent - 2 * radius - 2), CombineMode.Exclude);
				g.FillRectangle(parentBrush, ClientRectangle);
				g.SetClip(origClip, CombineMode.Replace);
				g.SmoothingMode = SmoothingMode.AntiAlias;
				var rect = new Rectangle(border + hIndent + radius + 2, border + vIndent + radius + 2, this.Width - 4 - 2 * border - 2 * hIndent - 2 * radius, this.Height - 4 - 2 * border - 2 * vIndent - 2 * radius);
				g.SetClip(rect, CombineMode.Exclude);
				g.FillRoundedRectangle(brush, hIndent, vIndent, this.Width - 2 - 2 * hIndent, this.Height - 2 - 2 * vIndent, radius);
				g.SetClip(origClip, CombineMode.Replace);
				if (Border > 0)
					g.DrawRoundedRectangle(pen, hIndent, vIndent,
						this.Width - 2 - 2 * hIndent, this.Height - 2 - 2 * vIndent, radius);
				g.SmoothingMode = SmoothingMode.None;
				g.FillRectangle(brush, rect);
			}
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			Invalidate(true);
		}
	}
}

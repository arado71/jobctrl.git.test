using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public enum ProgressStyle
	{
		Fill,
		Dot
	}

	public partial class ProgressBar : UserControl
	{
		private const int DotRadius = 3;
		private static readonly SolidBrush darkBrush = new SolidBrush(StyleUtils.ForegroundDark);
		private static readonly SolidBrush lightBrush = new SolidBrush(StyleUtils.ForegroundLight);
		private ProgressStyle style;
		private float value;

		public ProgressStyle Style
		{
			get { return style; }

			set
			{
				style = value;
				Invalidate();
			}
		}

		public float Value
		{
			get { return value; }

			set
			{
				this.value = Math.Max(Math.Min(value, 1f), 0f);
				Invalidate();
			}
		}

		public ProgressBar()
		{
			InitializeComponent();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.FillRectangle(lightBrush, 0, DotRadius, Width, 2);
			if (style == ProgressStyle.Fill)
			{
				e.Graphics.FillRectangle(darkBrush, 0, DotRadius, Width * value, 2);
			}
			else
			{
				e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
				e.Graphics.FillEllipse(darkBrush, (Width - DotRadius * 2) * value, 0, DotRadius * 2, DotRadius * 2);
			}
		}
	}
}
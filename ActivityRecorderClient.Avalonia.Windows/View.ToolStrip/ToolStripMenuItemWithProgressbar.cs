using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View.ToolStrip
{
	public class ToolStripMenuItemWithProgressbar : ToolStripMenuItemWithButton
	{
		public ToolStripMenuItemWithProgressbar()
			: this("", 0.345f)
		{
		}

		public ToolStripMenuItemWithProgressbar(string text, float? value)
			: base(text)
		{
			//ToolTipText = Value.ToString(FormatString);
			AutoToolTip = false;
			ImageAlign = ContentAlignment.MiddleCenter;
			ImageScaling = ToolStripItemImageScaling.None;

			showProgressbar = true;
			barSize = new Size(60, 15);
			formatString = "0.0%";
			barForeColor = Color.Blue;
			barForeOverflowColor = Color.Red;
			barBackColor = Color.White;
			fontForeColor = Color.White;
			fontBackColor = Color.Black;
			this.value = value;
			RefreshBarImage();
		}

		private bool showProgressbar;
		public bool ShowProgressbar
		{
			get { return showProgressbar; }
			set
			{
				if (showProgressbar == value) return;
				showProgressbar = value;
				RefreshBarImage();
			}
		}

		private float? value;
		public float? Value
		{
			get { return value; }
			set
			{
				if (this.value == value) return;
				this.value = value;
				RefreshBarImage();
			}
		}

		private string formatString;
		public string FormatString
		{
			get { return formatString; }
			set
			{
				if (formatString == value) return;
				formatString = value;
				RefreshBarImage();
			}
		}

		private string barText;
		public string BarText
		{
			get { return barText; }
			set
			{
				if (barText == value) return;
				barText = value;
				RefreshBarImage();
			}
		}

		private Size barSize;
		public Size BarSize
		{
			get { return barSize; }
			set
			{
				if (barSize == value) return;
				barSize = value;
				RefreshBarImage();
			}
		}

		private Color barForeColor;
		public Color BarForeColor
		{
			get { return barForeColor; }
			set
			{
				if (barForeColor == value) return;
				barForeColor = value;
				RefreshBarImage();
			}
		}

		private Color barForeOverflowColor;
		public Color BarForeOverflowColor
		{
			get { return barForeOverflowColor; }
			set
			{
				if (barForeOverflowColor == value) return;
				barForeOverflowColor = value;
				RefreshBarImage();
			}
		}

		private Color barBackColor;
		public Color BarBackColor
		{
			get { return barBackColor; }
			set
			{
				if (barBackColor == value) return;
				barBackColor = value;
				RefreshBarImage();
			}
		}

		private Color fontForeColor;
		public Color FontForeColor
		{
			get { return fontForeColor; }
			set
			{
				if (fontForeColor == value) return;
				fontForeColor = value;
				RefreshBarImage();
			}
		}

		private Color fontBackColor;
		public Color FontBackColor
		{
			get { return fontBackColor; }
			set
			{
				if (fontBackColor == value) return;
				fontBackColor = value;
				RefreshBarImage();
			}
		}

		public Color BarForeFinalColor
		{
			get { return Value.HasValue && Value.Value > 1 ? BarForeOverflowColor : BarForeColor; }
		}

		private void RefreshBarImage()
		{
			using (Image) { } //Dispose
			Image = ShowProgressbar && Value.HasValue
				? GetProgressBarImage(BarForeFinalColor, BarBackColor, FontForeColor, FontBackColor, BarSize, Value.Value, BarText ?? Value.Value.ToString(FormatString), Font)
				: null;
		}

		private const TextFormatFlags format = TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;

		private static Image GetProgressBarImage(Color barForeColor, Color barBackColor, Color fontForeColor, Color fontBackColor, Size barSize, float barValue, string barText, Font barFont)
		{
			//var mod = Math.Min(1, Math.Max(0, barValue));
			//barForeColor = Color.FromArgb((int)(255 * mod), (int)(255 * (1 - mod)), 0);

			var rect = new Rectangle(Point.Empty, barSize);
			if (rect.Width == 0 || rect.Height == 0 || rect.X != 0 || rect.Y != 0) return null;
			var rectLeft = new Rectangle(rect.X, rect.Y, (int)(rect.Width * Math.Min(1, Math.Max(0, barValue))), rect.Height);
			var rectRight = new Rectangle(rectLeft.Width, rect.Y, rect.Width - rectLeft.Width, rect.Height);

			Image result = new Bitmap(rect.Width, rect.Height);
			using (var gResult = Graphics.FromImage(result))
			{
				if (rectLeft.Width > 0)
				{
					gResult.Clear(barForeColor);
					TextRenderer.DrawText(gResult, barText, barFont, rect, fontForeColor, barForeColor, format);
				}

				if (rectRight.Width > 0)
				{
					using (var bitmapRight = new Bitmap(rect.Width, rect.Height))
					using (var gRight = Graphics.FromImage(bitmapRight))
					{
						gRight.Clear(barBackColor);
						TextRenderer.DrawText(gRight, barText, barFont, rect, fontBackColor, barBackColor, format);

						using (var bmpR = bitmapRight.Clone(rectRight, bitmapRight.PixelFormat))
						{
							gResult.DrawImage(bmpR, rectRight);
						}
					}
				}

				if (rect.Width > 1 && rect.Height > 1)
				{
					gResult.DrawRectangle(Pens.Black, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
				}
			}
			return result;
		}
	}

}

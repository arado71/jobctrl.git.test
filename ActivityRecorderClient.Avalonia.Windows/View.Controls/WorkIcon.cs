using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public sealed class WorkIcon : Panel
	{
		private readonly SolidBrush brush = new SolidBrush(StyleUtils.ColorPalette.First());
		private readonly SolidBrush foreBrush = new SolidBrush(StyleUtils.Background);
		private static readonly Font iconFont = StyleUtils.GetFont(FontStyle.Bold, 12f);
		private bool alternativeStyle;
		private string initials;

		public string Initials
		{
			get { return initials; }

			set
			{
				initials = value;
				Invalidate();
			}
		}

		public override Color ForeColor
		{
			get { return foreBrush.Color; }

			set { foreBrush.Color = value; }
		}

		public Color Color
		{
			get { return brush.Color; }
			set { brush.Color = value; }
		}

		public bool AlternativeStyle
		{
			get { return alternativeStyle; }

			set
			{
				alternativeStyle = value;
				Invalidate();
			}
		}

		public WorkIcon()
		{
			DoubleBuffered = true;
			Invalidate();
		}

		public static Color GetColor(string name)
		{
			int hash = name.Aggregate(0, (current, c) => current + c);
			return StyleUtils.GetColor(hash);
		}

		public static Color GetColor(WorkData workData)
		{
			return workData.Id.HasValue ? StyleUtils.GetColor(workData.Id.Value) : Color.Black;
		}

		public static string GetInitials(WorkDataWithParentNames work)
		{
			return GetInitials(work.WorkData.Name);
		}

		public static string GetInitials(string name)
		{
			if (name == null) return "";
			var trimmed = name.TrimStart(' ');
			return trimmed.Length > 2 ? trimmed.Substring(0, 2) : trimmed;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			if (alternativeStyle)
			{
				e.Graphics.FillRectangle(brush, 0, 0, Width - 2, Height - 2);
			}
			else
			{
				e.Graphics.FillEllipse(brush, 0, 0, Width - 2, Height - 2);
			}

			using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
			{
				e.Graphics.DrawString(initials, iconFont, foreBrush, new RectangleF(0, 0, Width, Height), format);
			}
		}
	}
}
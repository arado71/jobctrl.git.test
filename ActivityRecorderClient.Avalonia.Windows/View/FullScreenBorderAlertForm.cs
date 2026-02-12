using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	public partial class FullScreenBorderAlertForm : Form
	{
		private const int GWL_EXSTYLE = -20;
		//private const int WS_EX_LAYERED = 0x80000;
		private const int WS_EX_Transparent = 0x20;
		private const int WS_EX_TOOLWINDOW = 0x00000080;

		private int timerInterval, timerValue;

		public int BorderSize { get; set; } = 20;

		public Color BorderColor { get; set; } = Color.Red;

		public FullScreenBorderAlertForm()
		{
			InitializeComponent();
#if DEBUG
			TopMost = false;
#endif
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DoubleBuffered = true;
			WinApi.SetWindowLong(Handle, GWL_EXSTYLE, WinApi.GetWindowLong(Handle, GWL_EXSTYLE) /*| WS_EX_LAYERED */| WS_EX_Transparent | WS_EX_TOOLWINDOW);
		}

		public void SetInterval(int ms)
		{
			if (ms == 0)
			{
				if (flashTimer.Enabled)
					flashTimer.Stop();
				return;
			}

			timerInterval = ms;
			if (!flashTimer.Enabled)
				flashTimer.Start();
		}

		public void ShowOnce(int ms)
		{
			Show();
			hideTimer.Interval = ms;
			hideTimer.Start();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			using (var brush = new SolidBrush(BorderColor))
			{
				e.Graphics.FillRectangle(brush, 0, 0, Width, BorderSize);
				e.Graphics.FillRectangle(brush, 0, 0, BorderSize, Height);
				e.Graphics.FillRectangle(brush, 0, Height - BorderSize, Width, BorderSize);
				e.Graphics.FillRectangle(brush, Width - BorderSize, 0, BorderSize, Height);
			}
		}

		private void flashTimer_Tick(object sender, EventArgs e)
		{
			timerValue += flashTimer.Interval;
			if (timerValue < timerInterval) return;
			timerValue = 0;
			if (ConfigManager.LocalSettingsForUser.IdleAlertVisual)
			{
				Show();
				hideTimer.Start();
			}
			if (ConfigManager.LocalSettingsForUser.IdleAlertBeep)
				Beep(3000, 50); //high frequency, short sound
		}

		private void hideTimer_Tick(object sender, EventArgs e)
		{
			Hide();
			hideTimer.Stop();
		}

		[DllImport("kernel32.dll")]
		private static extern bool Beep(int freq, int duration);
	}
}

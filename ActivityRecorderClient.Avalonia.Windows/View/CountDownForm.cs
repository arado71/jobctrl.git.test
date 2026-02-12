using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MetroFramework.Forms;
using Tct.ActivityRecorderClient.Taskbar;
using log4net;
using Microsoft.VisualBasic.CompilerServices;
using Tct.ActivityRecorderClient.Stability;

namespace Tct.ActivityRecorderClient.View
{
	public partial class CountDownForm : FixedMetroForm
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly TaskbarWin7Service taskbarService;

		public string Description
		{
			get { return lblDesc.Text; }
			set { lblDesc.Text = value; }
		}

		public string Title { get; set; }
		public bool IsUserCloseNotForced { get; private set; } //indicates whether the user closed the form but not because starting new work
		public Func<TimeSpan> GetElapsedTime { get; set; }
		public TimeSpan TotalTime { get; set; }
		public Action<TimeSpan> OnUpdating { get; set; }
		public Action OnFinishing { get; set; }
		public bool IsCountUp
		{
			get { return isCountUp; }
			set
			{
				isCountUp = value;
				lblTimeLeft.Text = isCountUp ? Labels.CountDown_TimeElapsed : Labels.CountDown_TimeLeft;
			}
		}

		public CountDownForm()
		{
			InitializeComponent();

			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe

			taskbarService = new TaskbarWin7Service(this.Handle);

			lblTimeLeft.Text = Labels.CountDown_TimeLeft;
			GetElapsedTime = () => TimeSpan.Zero;
		}

		private void CountDownFormShown(object sender, EventArgs e)
		{
			countDownTimer.Enabled = true;
			taskbarService.SetProgressState(TaskbarWin7Service.TaskbarProgressBarStatus.Normal);
			UpdateDisplay(new TimeSpan());
		}
		private int prevMin = -1, prevSec = -1;
		private bool firstCall = true;
		private void CountDownTimerTick(object sender, EventArgs e)
		{
			var elapsedTime = GetElapsedTime();
			if (UpdateDisplay(elapsedTime)) return;
			DateTime d = DateTime.UtcNow;
			if (d.Minute != prevMin && d.Second == prevSec && !firstCall)
			{
				OnUpdating(elapsedTime + TimeSpan.FromMinutes(2));
				prevMin = d.Minute;
			}
			if (firstCall)
			{
				OnUpdating(elapsedTime + TimeSpan.FromMinutes(2));
				prevSec = d.Second;
				firstCall = false;
			}
		}
		private bool UpdateDisplay(TimeSpan elapsedTime)
		{
			var displayedTime = IsCountUp ? elapsedTime : TotalTime - elapsedTime;
			if (elapsedTime.Ticks < 0) elapsedTime = new TimeSpanEx(elapsedTime) * -1;
			lblTime.Text = displayedTime.ToHourMinuteSecondString();
			this.Text = "[" + displayedTime.ToHourMinuteSecondString() + "] " + Title;
			var totalMs = (uint)(TotalTime.TotalMilliseconds);
			taskbarService.SetProgressValue((uint)elapsedTime.TotalMilliseconds, totalMs);
			if (elapsedTime < TotalTime) return false;
			//time is up
			OnUpdating(elapsedTime);
			OnFinishing();
			countDownTimer.Enabled = false;
			this.WindowState = FormWindowState.Normal;
			this.TopMost = false;
			this.Focus();
			this.BringToFront();
			this.TopMost = true;
			log.Info("Manual work time ended");
			MessageBox.Show(this, Labels.NotificationManualWorkOverBody, Labels.NotificationManualWorkOverTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
			IsUserCloseNotForced = true; //start working after OK pressed (if working before)
			taskbarService.SetProgressState(TaskbarWin7Service.TaskbarProgressBarStatus.NoProgress);
			isForced = true;
			this.Close();
			return true;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (GetElapsedTime() < TotalTime) //we need to confirm if we want to stop manual worktime before enddate
			{
				this.WindowState = FormWindowState.Normal;
				this.TopMost = false;
				this.Focus();
				this.BringToFront();
				this.TopMost = true;
				var res =
					isForced
					? DialogResult.Yes
					: MessageBox.Show(this,
						externalClose ? Labels.NotificationManualWorkConfirmStopStartingNewBody : Labels.NotificationManualWorkConfirmStopBody,
						externalClose ? Labels.NotificationManualWorkConfirmStopStartingNewTitle : Labels.NotificationManualWorkConfirmStopTitle,
						MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				log.Info("Manual work time stoppage " + (externalClose ? "(starting new work) " : "") + "before enddate confirm: " + res + (isForced ? " Forced" : ""));
				if (res != DialogResult.Yes)
				{
					e.Cancel = true;
					this.TopMost = false;
				}
				else
				{
					if (!externalClose) //DialogResult.Yes && !isStartingNew
					{
						IsUserCloseNotForced = true;
					}
					OnUpdating(GetElapsedTime());
					OnFinishing();
				}
			}
			base.OnClosing(e);
		}

		private bool externalClose;
		private bool isForced;
		private bool isCountUp;

		public void CloseFrom(bool isForcedParam)
		{
			isForced = isForcedParam;
			externalClose = true;
			this.Close();
			externalClose = false;
			isForced = false;
		}
	}
	internal class TimeSpanEx
	{
		private TimeSpan ts { set; get; }

		public TimeSpanEx(TimeSpan _ts)
		{
			ts = _ts;
		}
		public static TimeSpan operator *(TimeSpanEx t1, int t)
		{
			return TimeSpan.FromTicks(t1.ts.Ticks * t);
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.View.ToolStrip
{
	public partial class WorkTimeStatsControl : UserControl
	{
		private const string deltaPrefix = "\u0394: ";
		private const string summaPrefix = "\u01a9: ";
		private const string unknowShortTime = "--:--";
		private const string unknowLongTime = "---:--";

		private bool weekShown = true;

		public WorkTimeStatsControl()
		{
			InitializeComponent();
			lblMonthsDeltaTime.Text = deltaPrefix + unknowShortTime;
			lblMonthsSummaTime.Text = summaPrefix + unknowLongTime;
			lblWeeksDeltaTime.Text = deltaPrefix + unknowShortTime;
			lblWeeksSummaTime.Text = summaPrefix + unknowShortTime;
			lblTodaysWorkTime.Text = unknowShortTime;
			lblTodaysSpecialTime.Text = "";
			lblTodaysSpecialTime.ForeColor = Color.DimGray;

			lblTodaysWorkTimeText.Text = Labels.TodaysWorkTime + ":";
			lblWeeksWorkTimeText.Text = Labels.ThisWeeksWorkTime + ":";
			lblMonthsWorkTimeText.Text = Labels.ThisMonthsWorkTime + ":";

			toolTip.SetToolTip(lblMonthsDeltaTime, Labels.WorkTimeStats_DeltaTime);
			toolTip.SetToolTip(lblWeeksDeltaTime, Labels.WorkTimeStats_DeltaTime);
			toolTip.SetToolTip(lblMonthsSummaTime, Labels.WorkTimeStats_SummaTime);
			toolTip.SetToolTip(lblWeeksSummaTime, Labels.WorkTimeStats_SummaTime);

			if (DesignMode) return;
			//resize so the text will fit into the control
			using (var g = this.CreateGraphics())
			{
				var size = GetMaxSize(g, new[] { lblTodaysWorkTimeText, lblWeeksWorkTimeText, lblMonthsWorkTimeText });
				var newWidth = (int)Math.Ceiling(size.Width) + 12;
				var sizeChange = newWidth - (int)tlPanel.ColumnStyles[0].Width;
				Width += sizeChange;
				tlPanel.ColumnStyles[0].Width = newWidth;
			}

		}

		private static SizeF GetMaxSize(Graphics g, IEnumerable<Control> controls)
		{
			return controls.Aggregate(
				SizeF.Empty, (res, currCtrl) =>
					{
						var size = g.MeasureString(currCtrl.Text, currCtrl.Font);
						return new SizeF(Math.Max(size.Width, res.Width), Math.Max(size.Height, res.Height));
					});
		}

		private bool supressTodaysWorkTimeTextRefresh;
		private void RefreshTodaysWorkTimeText()
		{
			if (supressTodaysWorkTimeTextRefresh) return;
			bool localIndicator;
			var workTime = GetTodaysWorkTime(todaysLocalWorkTime ?? TimeSpan.Zero, todaysWorkTime, unsentItemsCount, out localIndicator);
			var ind = localIndicator ? "." : "";
			lblTodaysWorkTime.Text = workTime.ToHourMinuteString() + ind;
			lblTodaysWorkTime.Tag = workTime.ToHourMinuteSecondString() + ind;
		}

		public TimeSpan GetTodaysWorkTime(out bool localIndicator)
		{
			return GetTodaysWorkTime(TodaysLocalWorkTime ?? TimeSpan.Zero, TodaysWorkTime, unsentItemsCount, out localIndicator);
		}

		private static TimeSpan GetTodaysWorkTime(TimeSpan localWorkTime, TimeSpan? srvWorkTime, int unsentItems, out bool localIndicator)
		{
			if (srvWorkTime == null) //if we don't have the server time use the local
			{
				localIndicator = true;
				return localWorkTime;
			}
			if (unsentItems > 5) //if we have significant offline data then use the bigger one (it is rare and won't be accurate but still possible that netWorkTime is bigger)
			{
				if (localWorkTime > srvWorkTime.Value)
				{
					localIndicator = true;
					return localWorkTime;
				}
				else
				{
					localIndicator = false;
					return srvWorkTime.Value;
				}
			}
			var localDiff = localWorkTime - srvWorkTime.Value;
			if (localDiff > TimeSpan.Zero && localDiff < TimeSpan.FromMinutes(2.5)) //if local is only a bit bigger than the server time then use it (without the '.')
			{
				localIndicator = false;
				return localWorkTime;
			}
			//else use the server time
			localIndicator = false;
			return srvWorkTime.Value;
		}

		private TimeSpan? todaysWorkTime;
		public TimeSpan? TodaysWorkTime
		{
			get { return todaysWorkTime; }
			set
			{
				if (todaysWorkTime == value) return;
				todaysWorkTime = value;
				RefreshTodaysWorkTimeText();
			}
		}

		private TimeSpan? todaysLocalWorkTime;
		public TimeSpan? TodaysLocalWorkTime
		{
			get { return todaysLocalWorkTime; }
			set
			{
				if (todaysLocalWorkTime == value) return;
				todaysLocalWorkTime = value;
				RefreshTodaysWorkTimeText();
			}
		}

		private int unsentItemsCount;
		public int UnsentItemsCount
		{
			get { return unsentItemsCount; }
			set
			{
				if (unsentItemsCount == value) return;
				unsentItemsCount = value;
				RefreshTodaysWorkTimeText();
				lblItemsToUpload.Text = unsentItemsCount == 0 ? "" : "(" + unsentItemsCount.ToString(CultureInfo.CurrentUICulture) + ")";
				toolTip.SetToolTip(lblItemsToUpload, unsentItemsCount == 0 ? "" : string.Format(Labels.NumberOfItemsToUpload, unsentItemsCount));
			}
		}

		private TimeSpan? todaysSpecialTime;
		public TimeSpan? TodaysSpecialTime
		{
			get { return todaysSpecialTime; }
			set
			{
				if (todaysSpecialTime == value) return;
				todaysSpecialTime = value;
				lblTodaysSpecialTime.Text = todaysSpecialTime == null ? "" : todaysSpecialTime.ToHourMinuteString();
			}
		}

		private TimeSpan? thisMonthsWorkTime;
		public TimeSpan? ThisMonthsWorkTime
		{
			get { return thisMonthsWorkTime; }
			set
			{
				if (thisMonthsWorkTime == value) return;
				thisMonthsWorkTime = value;
				lblMonthsWorkTime.Text = thisMonthsWorkTime == null ? unknowShortTime : thisMonthsWorkTime.ToHourMinuteString();
			}
		}

		private TimeSpan? thisMonthsDeltaTime;
		public TimeSpan? ThisMonthsDeltaTime
		{
			get { return thisMonthsDeltaTime; }
			set
			{
				if (thisMonthsDeltaTime == value) return;
				thisMonthsDeltaTime = value;
				lblMonthsDeltaTime.Text = deltaPrefix + (thisMonthsDeltaTime == null ? unknowShortTime : thisMonthsDeltaTime.ToHourMinuteString());
				SetDeltaColor(lblMonthsDeltaTime, thisMonthsDeltaTime);
			}
		}

		private TimeSpan? thisMonthsSummaTime;
		public TimeSpan? ThisMonthsSummaTime
		{
			get { return thisMonthsSummaTime; }
			set
			{
				if (thisMonthsSummaTime == value) return;
				thisMonthsSummaTime = value;
				lblMonthsSummaTime.Text = summaPrefix + (thisMonthsSummaTime == null ? unknowLongTime : thisMonthsSummaTime.ToHourMinuteString());
			}
		}

		private TimeSpan? thisWeeksWorkTime;
		public TimeSpan? ThisWeeksWorkTime
		{
			get { return thisWeeksWorkTime; }
			set
			{
				if (thisWeeksWorkTime == value) return;
				thisWeeksWorkTime = value;
				lblWeeksWorkTime.Text = thisWeeksWorkTime == null ? unknowShortTime : thisWeeksWorkTime.ToHourMinuteString();
			}
		}

		private TimeSpan? thisWeeksDeltaTime;
		public TimeSpan? ThisWeeksDeltaTime
		{
			get { return thisWeeksDeltaTime; }
			set
			{
				if (thisWeeksDeltaTime == value) return;
				thisWeeksDeltaTime = value;
				lblWeeksDeltaTime.Text = deltaPrefix + (thisWeeksDeltaTime == null ? unknowShortTime : thisWeeksDeltaTime.ToHourMinuteString());
				SetDeltaColor(lblWeeksDeltaTime, thisWeeksDeltaTime);
			}
		}

		private TimeSpan? thisWeeksSummaTime;
		public TimeSpan? ThisWeeksSummaTime
		{
			get { return thisWeeksSummaTime; }
			set
			{
				if (thisWeeksSummaTime == value) return;
				thisWeeksSummaTime = value;
				lblWeeksSummaTime.Text = summaPrefix + (thisWeeksSummaTime == null ? unknowLongTime : thisWeeksSummaTime.ToHourMinuteString());
			}
		}


		public void SetWeeksVisible(bool visible)
		{
			if (weekShown == visible) return;
			if (weekShown)
			{
				tlPanel.RemoveRow(1);
				Height = Height / 3 * 2;
			}
			else
			{
				tlPanel.InsertRow(1, new RowStyle(SizeType.Percent, 33.33f), new[] { lblWeeksWorkTimeText, lblWeeksWorkTime, lblWeeksDeltaTime, lblWeeksSummaTime });
				Height = Height / 2 * 3;
			}
			weekShown = visible;
		}

		public void SetSummaVisible(bool visible)
		{
			lblWeeksSummaTime.Visible = visible;
			lblMonthsSummaTime.Visible = visible;
		}

		public void SetDeltaVisible(bool visible)
		{
			lblWeeksDeltaTime.Visible = visible;
			lblMonthsDeltaTime.Visible = visible;
		}

		public string SetWorkTimeStats(TimeSpan todaysLocalTime, int unsentItems, ClientWorkTimeStats stats)
		{
			supressTodaysWorkTimeTextRefresh = true; //don't refresh it multiple times
			TodaysLocalWorkTime = todaysLocalTime;
			TodaysWorkTime = stats == null || stats.TodaysWorkTime == null ? (TimeSpan?)null : stats.TodaysWorkTime.NetWorkTime;
			UnsentItemsCount = unsentItems;
			supressTodaysWorkTimeTextRefresh = false;
			RefreshTodaysWorkTimeText(); //this will refresh TodaysWorkTimeText

			if (stats != null
			&& stats.TodaysWorkTime != null
			&& stats.TodaysWorkTime.ManuallyAddedWorkTime != TimeSpan.Zero)
			{
				TodaysSpecialTime = stats.TodaysWorkTime.ManuallyAddedWorkTime;
				toolTip.SetToolTip(lblTodaysSpecialTime, Labels.TodaysManuallyAddedWorkTime + ": " + stats.TodaysWorkTime.ManuallyAddedWorkTime.ToHourMinuteString());
			}
			else
			{
				TodaysSpecialTime = null;
				toolTip.SetToolTip(lblTodaysSpecialTime, "");
			}

			ThisMonthsWorkTime = stats == null ? (TimeSpan?)null : stats.ThisMonthsWorkTime.NetWorkTime;
			ThisMonthsSummaTime = stats == null ? (TimeSpan?)null : stats.ThisMonthsTargetNetWorkTime;
			ThisMonthsDeltaTime = stats == null ? (TimeSpan?)null : stats.ThisMonthsTargetUntilTodayNetWorkTime - stats.ThisMonthsWorkTime.NetWorkTime;
			ThisWeeksWorkTime = stats == null ? (TimeSpan?)null : stats.ThisWeeksWorkTime.NetWorkTime;
			ThisWeeksSummaTime = stats == null ? (TimeSpan?)null : stats.ThisWeeksTargetNetWorkTime;
			ThisWeeksDeltaTime = stats == null ? (TimeSpan?)null : stats.ThisWeeksTargetUntilTodayNetWorkTime - stats.ThisWeeksWorkTime.NetWorkTime;
			var todaysTooltip = stats == null || stats.TodaysWorkTime == null || stats.TodaysWorkTime.NetWorkTime == TimeSpan.Zero
				? ""
				: ((stats.TodaysWorkTime.ComputerWorkTime == TimeSpan.Zero ? "" : Labels.TodaysComputerWorkTime + ": " + stats.TodaysWorkTime.ComputerWorkTime.ToHourMinuteString() + " ")
				+ (stats.TodaysWorkTime.MobileWorkTime == TimeSpan.Zero ? "" : Labels.TodaysMobileWorkTime + ": " + stats.TodaysWorkTime.MobileWorkTime.ToHourMinuteString() + " ")
				+ (stats.TodaysWorkTime.ManuallyAddedWorkTime == TimeSpan.Zero ? "" : Labels.TodaysManuallyAddedWorkTime + ": " + stats.TodaysWorkTime.ManuallyAddedWorkTime.ToHourMinuteString() + " "));
			if (stats == null)
			{
				todaysTooltip = Labels.TodaysLocalWorkTime + ": " + todaysLocalTime.ToHourMinuteSecondString();
			}
			toolTip.SetToolTip(lblTodaysWorkTime, todaysTooltip);
			toolTip.SetToolTip(lblTodaysWorkTimeText, todaysTooltip);

			//we have to pay the price of localization.. that is why this is kinda copy/paste :(
			var thisWeeksTooltip = stats == null || stats.ThisWeeksWorkTime == null || stats.ThisWeeksWorkTime.NetWorkTime == TimeSpan.Zero
				? ""
				: ((stats.ThisWeeksWorkTime.ComputerWorkTime == TimeSpan.Zero ? "" : Labels.ThisWeeksComputerWorkTime + ": " + stats.ThisWeeksWorkTime.ComputerWorkTime.ToHourMinuteString() + " ")
				+ (stats.ThisWeeksWorkTime.MobileWorkTime == TimeSpan.Zero ? "" : Labels.ThisWeeksMobileWorkTime + ": " + stats.ThisWeeksWorkTime.MobileWorkTime.ToHourMinuteString() + " ")
				+ (stats.ThisWeeksWorkTime.ManuallyAddedWorkTime == TimeSpan.Zero ? "" : Labels.ThisWeeksManuallyAddedWorkTime + ": " + stats.ThisWeeksWorkTime.ManuallyAddedWorkTime.ToHourMinuteString() + " "));
			toolTip.SetToolTip(lblWeeksWorkTime, thisWeeksTooltip);
			toolTip.SetToolTip(lblWeeksWorkTimeText, thisWeeksTooltip);

			var thisMonthsTooltip = stats == null || stats.ThisMonthsWorkTime == null || stats.ThisMonthsWorkTime.NetWorkTime == TimeSpan.Zero
				? ""
				: ((stats.ThisMonthsWorkTime.ComputerWorkTime == TimeSpan.Zero ? "" : Labels.ThisMonthsComputerWorkTime + ": " + stats.ThisMonthsWorkTime.ComputerWorkTime.ToHourMinuteString() + " ")
				+ (stats.ThisMonthsWorkTime.MobileWorkTime == TimeSpan.Zero ? "" : Labels.ThisMonthsMobileWorkTime + ": " + stats.ThisMonthsWorkTime.MobileWorkTime.ToHourMinuteString() + " ")
				+ (stats.ThisMonthsWorkTime.ManuallyAddedWorkTime == TimeSpan.Zero ? "" : Labels.ThisMonthsManuallyAddedWorkTime + ": " + stats.ThisMonthsWorkTime.ManuallyAddedWorkTime.ToHourMinuteString() + " "));
			toolTip.SetToolTip(lblMonthsWorkTime, thisMonthsTooltip);
			toolTip.SetToolTip(lblMonthsWorkTimeText, thisMonthsTooltip);

			return (string)lblTodaysWorkTime.Tag;
		}

		private static void SetDeltaColor(Label lblDeltaTime, TimeSpan? deltaTime)
		{
			lblDeltaTime.ForeColor =
				deltaTime == null
					? Color.Black
					: (deltaTime > TimeSpan.Zero ? Color.Red : Color.Green);
		}

	}
}

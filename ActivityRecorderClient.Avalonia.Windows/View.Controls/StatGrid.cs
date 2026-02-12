using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public partial class StatGrid : UserControl, ILocalizableControl
	{
		private string extraRowDelta, baseRowDelta;
		private bool deltaVisible = false;
		private bool sumVisible = false;
		private bool extraRowVisible = false;

		public event EventHandler StatsClicked;

		public TimeSpan DayUsed
		{
			set
			{
				lblDayUsed.Text = value.ToHourMinuteString();
			}
		}

		public TimeSpan? DaySum
		{
			set
			{
				lblDayTotal.Text = value != null ? value.Value.ToHourMinuteString() : null;
				lblDayTotal.SetVisibleAndTag(value != null && sumVisible);
			}
		}

		public TimeSpan? ExtraRowDelta
		{
			set
			{
				extraRowDelta = value != null ? value.Value.Duration().ToHourMinuteString() : null;
				lblExtraRowDelta.SetVisibleAndTag(!string.IsNullOrEmpty(extraRowDelta) && deltaVisible);
				if (value == null) return;
				lblExtraRowDelta.ForeColor = value.Value.Ticks < 0 ? StyleUtils.Negative : StyleUtils.Positive;
				lblExtraRowDelta.Text = string.Format("∆ {0}", extraRowDelta);
			}
		}

		public TimeSpan? ExtraRowUsed
		{
			set
			{
				lblExtraRowUsed.Text = value.ToHourMinuteString();
			}
		}

		public TimeSpan? ExtraRowSum
		{
			set
			{
				lblExtraTotal.Text = value.ToHourMinuteString();
				lblExtraTotal.SetVisibleAndTag(value != null && sumVisible);
			}
		}

		public string ExtraRowTitle
		{
			set => lblExtraRowTitle.Text = value + @":";
		}

		public TimeSpan? BaseRowDelta
		{
			set
			{
				baseRowDelta = value != null ? value.Value.Duration().ToHourMinuteString() : null;
				lblBaseRowDelta.SetVisibleAndTag(!string.IsNullOrEmpty(baseRowDelta) && deltaVisible);
				if (value == null) return;
				lblBaseRowDelta.ForeColor = value.Value.Ticks < 0 ? StyleUtils.Negative : StyleUtils.Positive;
				lblBaseRowDelta.Text = string.Format("∆ {0}", baseRowDelta);
			}
		}

		public TimeSpan? BaseRowUsed
		{
			set
			{
				lblBaseRowUsed.Text = value.ToHourMinuteString();
			}
		}

		public TimeSpan? BaseRowSum
		{
			set
			{
				lblBaseRowTotal.Text = value != null ? value.Value.ToHourMinuteString() : null;
				lblBaseRowTotal.SetVisibleAndTag(value != null && sumVisible);
			}
		}

		public string BaseRowTitle
		{
			set => lblBaseRowTitle.Text = value + @":";
		}

		public bool DeltaVisible
		{
			get
			{
				return deltaVisible;
			}

			set
			{
				deltaVisible = value;
				lblExtraRowDelta.SetVisibleAndTag(!string.IsNullOrEmpty(extraRowDelta) && deltaVisible);
				lblBaseRowDelta.SetVisibleAndTag(!string.IsNullOrEmpty(baseRowDelta) && deltaVisible);
			}
		}

		public bool SumVisible
		{
			get
			{
				return sumVisible;
			}

			set
			{
				sumVisible = value;
				lblDayTotal.SetVisibleAndTag(!string.IsNullOrEmpty(lblDayTotal.Text) && sumVisible);
				lblExtraTotal.SetVisibleAndTag(!string.IsNullOrEmpty(lblExtraTotal.Text) && sumVisible);
				lblBaseRowTotal.SetVisibleAndTag(!string.IsNullOrEmpty(lblBaseRowTotal.Text) && sumVisible);

			}
		}

		public bool ExtraRowVisible
		{
			get
			{
				return extraRowVisible;
			}

			set
			{
				extraRowVisible = value;
				tableLayoutPanel1.RowStyles[1].Height = 0;
				tableLayoutPanel1.RowStyles[1].SizeType = value ? SizeType.AutoSize : SizeType.Absolute;
				tableLayoutPanel1.RowStyles[2].Height = 0;
				tableLayoutPanel1.RowStyles[2].SizeType = value ? SizeType.AutoSize : SizeType.Absolute;
			}
		}

		public StatGrid()
		{
			DoubleBuffered = true;
			InitializeComponent();
			SetColorScheme();
			ExtraRowDelta = null;
			BaseRowDelta = null;
			lblDayUsed.Font = StyleUtils.GetFont(FontStyle.Bold, 9f);
			lblDayTotal.Font = StyleUtils.GetFont(FontStyle.Bold, 9f);
			lblDayTitle.Font = StyleUtils.GetFont(FontStyle.Regular, 8f);
			lblExtraRowDelta.Font = StyleUtils.GetFont(FontStyle.Bold, 9f);
			lblExtraRowUsed.Font = StyleUtils.GetFont(FontStyle.Bold, 9f);
			lblExtraTotal.Font = StyleUtils.GetFont(FontStyle.Bold, 9f);
			lblExtraRowTitle.Font = StyleUtils.GetFont(FontStyle.Regular, 8f);
			lblBaseRowDelta.Font = StyleUtils.GetFont(FontStyle.Bold, 9f);
			lblBaseRowUsed.Font = StyleUtils.GetFont(FontStyle.Bold, 9f);
			lblBaseRowTotal.Font = StyleUtils.GetFont(FontStyle.Bold, 9f);
			lblBaseRowTitle.Font = StyleUtils.GetFont(FontStyle.Regular, 8f);
			Localize();

			lblDayUsed.UseCompatibleTextRendering = true;
			lblDayTotal.UseCompatibleTextRendering = true;
			lblDayTitle.UseCompatibleTextRendering = true;
			lblExtraRowDelta.UseCompatibleTextRendering = true;
			lblExtraRowUsed.UseCompatibleTextRendering = true;
			lblExtraTotal.UseCompatibleTextRendering = true;
			lblExtraRowTitle.UseCompatibleTextRendering = true;
			lblBaseRowDelta.UseCompatibleTextRendering = true;
			lblBaseRowUsed.UseCompatibleTextRendering = true;
			lblBaseRowTotal.UseCompatibleTextRendering = true;
			lblBaseRowTitle.UseCompatibleTextRendering = true;
#if !NET4
			pStatsBtn.Visible = false;
#endif
		}

		public void SetColorScheme()
		{
			if (SystemInformation.HighContrast)
			{
				lblDayUsed.ForeColor = SystemColors.WindowText;
				lblDayTotal.ForeColor = SystemColors.HighlightText;
				lblDayTitle.ForeColor = SystemColors.WindowText;
				lblExtraRowUsed.ForeColor = SystemColors.WindowText;
				lblExtraTotal.ForeColor = SystemColors.HighlightText;
				lblExtraRowTitle.ForeColor = SystemColors.WindowText;
				lblBaseRowUsed.ForeColor = SystemColors.WindowText;
				lblBaseRowTotal.ForeColor = SystemColors.HighlightText;
				lblBaseRowTitle.ForeColor = SystemColors.WindowText;
				BackColor = SystemColors.Window;
			}
			else
			{
				lblDayUsed.ForeColor = StyleUtils.Foreground;
				lblDayTotal.ForeColor = StyleUtils.ForegroundLight;
				lblDayTitle.ForeColor = StyleUtils.Foreground;
				lblExtraRowUsed.ForeColor = StyleUtils.Foreground;
				lblExtraTotal.ForeColor = StyleUtils.ForegroundLight;
				lblExtraRowTitle.ForeColor = StyleUtils.Foreground;
				lblBaseRowUsed.ForeColor = StyleUtils.Foreground;
				lblBaseRowTotal.ForeColor = StyleUtils.ForegroundLight;
				lblBaseRowTitle.ForeColor = StyleUtils.Foreground;
				BackColor = StyleUtils.BackgroundInactive;
			}
		}

		public void Localize()
		{
			toolTip1.SetToolTip(pStatsBtn, Labels.Worktime_Tooltip);
			lblDayTitle.Text = Labels.TodaysWorkTime + @":";
		}

		private void HandleStatsClicked(object sender, MouseEventArgs e)
		{
			var evt = StatsClicked;
			if (evt != null) evt(this, EventArgs.Empty);
		}
	}
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.View.Controls;
using Tct.ActivityRecorderClient.WorktimeHistory;

namespace Tct.ActivityRecorderClient.View
{
	public partial class WorkTimeHistoryForm : FixedMetroForm
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private DeviceWorkIntervalLookup currentLookup;
		private readonly IWorkTimeService service;
		private bool pickerDropdown = false;

		public WorkTimeHistoryForm()
		{
			InitializeComponent();
			Icon = Resources.JobCtrl;
			Text = Labels.Worktime_Title;
			lblStart.Text = Labels.Worktime_Start;
			lblEnd.Text = Labels.Worktime_End;
			lblGrouping.Text = Labels.Worktime_Grouping;
			cbGroupBy.Items.Add(Labels.Worktime_Work);
			cbGroupBy.Items.Add(Labels.WorkData_Category);
			cbGroupBy.SelectedIndex = 0;
			lblTotal.Text = Labels.Worktime_Total;
			lblDuration.Text = Labels.Worktime_Duration;
			cbShowDeleted.Text = Labels.Worktime_ShowDeleted;
			InitializeChart();
		}

		public WorkTimeHistoryForm(IWorkTimeService service)
			: this()
		{
			this.service = service;
		}

		public void SetSelection(DateTime start, DateTime end)
		{
			chart.SetSelection(start, end);
		}

		private void InitializeChart()
		{
			chart.AntiAliasing = AntiAliasingStyles.All;
			chart.TextAntiAliasingQuality = TextAntiAliasingQuality.High;
			chart.Palette = ChartColorPalette.SemiTransparent;
			chart.BackColor = StyleUtils.Background;
		}

		private void GenerateChart(DeviceWorkIntervalLookup lookup, IEnumerable<WorkOrProjectWithParentNames> works, Interval dayInterval)
		{
			currentLookup = lookup;
			Debug.Assert(currentLookup != null);

			chart.Visible = true;
			chart.LoadSeries(currentLookup, works, dayInterval);
			if (currentLookup.VisibleBounds != null)
			{
				var localBoundStart = currentLookup.VisibleBounds.StartDate;
				var localBoundEnd = currentLookup.VisibleBounds.EndDate;
				lblStartValue.Text = localBoundStart.ToShortTimeString();
				lblEndValue.Text = localBoundEnd.ToShortTimeString();
				lblDurationValue.Text = currentLookup.StartEndDiff.ToHourMinuteString();
			}
			else
			{
				lblStartValue.Text = "";
				lblEndValue.Text = "";
				lblDurationValue.Text = "";
			}

			lblTotalValue.Text = currentLookup.WorkTime.ToHourMinuteSecondString();
		}

		protected override void SetBusyImpl(bool isBusy)
		{
			dtpDay.Enabled = !isBusy;
			btnNext.Enabled = !isBusy;
			btnPrev.Enabled = !isBusy;
		}

		private void ProcessIntervalResult(Interval i)
		{
			BackgroundQuery(() => service.GetStats(i), x => ProcessIntervalLookup(x, i), Labels.Worktime_NoResponse);
		}

		private void ProcessIntervalLookup(DeviceWorkIntervalLookup lookup, Interval i)
		{
			var workIds = lookup.WorkIds.ToList();
			var menuWorkNames = GetWorkNamesFromClientMenu(workIds);
			if (workIds.Count > 0)
			{
				BackgroundQuery(() => service.GetWorkOrProjectWithParentNames(workIds), x => GenerateChart(lookup, x.Union(menuWorkNames), i),
					Labels.Worktime_NoResponse);
			}
			else
			{
				GenerateChart(lookup, menuWorkNames, i);
			}
		}

		private IEnumerable<WorkOrProjectWithParentNames> GetWorkNamesFromClientMenu(List<int> workOrProjectIds)
		{
			var clientMenuLookup = MenuQuery.Instance.ClientMenuLookup.Value;
			var res = new List<WorkOrProjectWithParentNames>();
			for (int i = 0; i < workOrProjectIds.Count; ++i)
			{
				WorkDataWithParentNames workData;
				if (!clientMenuLookup.WorkDataById.TryGetValue(workOrProjectIds[i], out workData))
				{
					if (!clientMenuLookup.ProjectDataById.TryGetValue(workOrProjectIds[i], out workData))
						continue;
				}

				Debug.Assert(workData.WorkData.Id != null || workData.WorkData.ProjectId != null);
				var id = workData.WorkData.Id ?? workData.WorkData.ProjectId.Value;
				Debug.Assert(workOrProjectIds.Contains(id));
				workOrProjectIds.RemoveAt(i--);
				res.Add(new WorkOrProjectWithParentNames { FullName = workData.FullName, WorkOrProjectName = new WorkOrProjectName(workData) });
			}

			return res;
		}

		public void GenerateChart(DateTime localDay)
		{
			log.InfoFormat("Generating statistics for {0}", localDay);
			BackgroundQuery(() => service.GetLocalDayInterval(localDay), x => ProcessIntervalResult(x), Labels.Worktime_NoResponse);
		}

		private void HandleDateChanged(object sender, EventArgs e)
		{
			if (!pickerDropdown) GenerateChart(dtpDay.Value.Date);
		}

		private double posStart, posEnd;
		private int currentPointIndex;
		private Series currentSeries = null;
		private bool isDraggingSelection = false;
		private void HandleSelectionRangeChanged(object sender, CursorEventArgs e)
		{
			isDraggingSelection = false;
			var start = DateTime.FromOADate(Math.Min(posStart, posEnd)).FromLocalToUtc();
			var end = DateTime.FromOADate(Math.Max(posStart, posEnd)).FromLocalToUtc();
			if (start == end)
			{
				var freeInterval = Interval.FindSpace(chart.LastSelectionStart.FromLocalToUtc(), currentLookup.Intervals);
				if (freeInterval != null)
				{
					chart.SetSelection(freeInterval.StartDate.ToLocalTime(), freeInterval.EndDate.ToLocalTime());
					service.ShowModifyInterval(freeInterval);
					return;
				}
			}

			TelemetryHelper.RecordFeature("WorktimeModification", "RangeSelect");
			service.ShowModifyInterval(new Interval { StartDate = start, EndDate = end });
		}

		public void Regenerate()
		{
			GenerateChart(dtpDay.Value.Date);
		}

		public void ResetSelection()
		{
			chart.ChartAreas[0].CursorY.SetSelectionPosition(0.0, 0.0);
			chart.ChartAreas[0].CursorY.SetCursorPosition(0.0);
		}

		private void HandleSelectionRangeChanging(object sender, CursorEventArgs e)
		{
			posStart = e.NewSelectionStart;
			posEnd = e.NewSelectionEnd;
			isDraggingSelection = true;
		}

		private void HandleChartMouseMove(object sender, MouseEventArgs e)
		{
			if (isDraggingSelection)
			{
				chart.Cursor = Cursors.SizeWE;
				return;
			}

			var hitResult = chart.HitTest(e.X, e.Y);
			switch (hitResult.ChartElementType)
			{
				case ChartElementType.DataPoint:
					currentPointIndex = hitResult.PointIndex;
					currentSeries = hitResult.Series;
					chart.Cursor = Cursors.Hand;
					chart.ChartAreas[0].CursorY.IsUserEnabled = false;
					chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = false;
					break;
				case ChartElementType.Gridlines:
				case ChartElementType.PlottingArea:
					currentSeries = null;
					chart.Cursor = Cursors.Cross;
					chart.ChartAreas[0].CursorY.IsUserEnabled = true;
					chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
					break;
				default:
					chart.Cursor = Cursors.Arrow;
					break;
			}
		}

		private void HandleChartMouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (currentSeries != null)
				{
					var original = chart.GetDeviceWorkInterval(currentSeries, currentPointIndex);
					TelemetryHelper.RecordFeature("WorktimeModification", "WorkSelect");
					service.ShowModifyWork(original);
				}
			}
		}

		private void btnPrev_Click(object sender, EventArgs e)
		{
			dtpDay.Value = dtpDay.Value.AddDays(-1);
		}

		private void btnNext_Click(object sender, EventArgs e)
		{
			dtpDay.Value = dtpDay.Value.AddDays(1);
		}

		private void HandleShowDeletedChanged(object sender, EventArgs e)
		{
			chart.IsDeletedVisible = cbShowDeleted.Checked;
		}

		private void HandleGroupByChanged(object sender, EventArgs e)
		{
			chart.GroupByWork = cbGroupBy.SelectedIndex == 0;
		}

		private void HandlePickerClosedUp(object sender, EventArgs e)
		{
			pickerDropdown = false;
			GenerateChart(dtpDay.Value.Date);
		}

		private void HandlePickerDropDown(object sender, EventArgs e)
		{
			pickerDropdown = true;
		}
	}
}

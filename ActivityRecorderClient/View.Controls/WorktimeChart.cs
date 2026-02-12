using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.WorktimeHistory;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public class WorkTimeChart : Chart
	{
		private const int SelectionMinutePrecision = 5;
		private const string IntervalAreaName = "Area";
		private const string IntervalLegendName = "WorkIntervals";
		private readonly Dictionary<int, Series> seriesByWorkId = new Dictionary<int, Series>();
		private readonly Dictionary<int, string> workNameById = new Dictionary<int, string>(); 
		private readonly Dictionary<int, DeviceType> deviceByXCoord = new Dictionary<int, DeviceType>();
		private DeviceWorkIntervalLookup currentLookup;
		private WorkOrProjectWithParentNames[] currentWorks;
		private Legend legendIntervals;
		private Interval currentBounds;

		private bool groupByWork;
		private bool isDeletedVisible;
		private bool isEmpty;

		private readonly ChartArea areaIntervals = null;
		private int xValue = 0;

		public bool GroupByWork
		{
			set
			{
				if (groupByWork == value) return;
				groupByWork = value;
				RedrawChart();
			}

			get
			{
				return groupByWork;
			}
		}

		public bool IsDeletedVisible
		{
			set
			{
				if (isDeletedVisible == value) return;
				isDeletedVisible = value;
				RedrawChart();
			}

			get
			{
				return isDeletedVisible;
			}
		}

		public DateTime LastSelectionStart { get; private set; }

		public WorkTimeChart()
		{
			areaIntervals = new ChartArea(IntervalAreaName)
			{
				AxisY =
				{
					MajorTickMark = {Enabled = true, Interval = 2.0, IntervalType = DateTimeIntervalType.Hours },
					MajorGrid = { LineColor = Color.FromArgb(50, 0, 0, 0), Enabled = true, Interval = 1.0, IntervalType = DateTimeIntervalType.Hours },
					MinorGrid = { LineColor = Color.FromArgb(20, 0, 0, 0), Enabled = true, Interval = 15.0, IntervalType = DateTimeIntervalType.Minutes },
					LabelStyle = { Format = "HH:mm", Interval = 1.0, IntervalType = DateTimeIntervalType.Hours },
					Enabled = AxisEnabled.True,
				},
				AxisX =
				{
					MajorGrid = { Enabled = false, },
					MinorGrid = { Enabled = false, },
					Interval = 1, //don't skip any labels
					Enabled = AxisEnabled.True
				},
				AlignmentOrientation = AreaAlignmentOrientations.Horizontal
			};
			ChartAreas.Add(areaIntervals);
			areaIntervals.AxisY.ScaleView.Zoomable = false;
			var ca = areaIntervals.CursorY;
			ca.IsUserEnabled = true;
			ca.Interval = SelectionMinutePrecision / (24.0 * 60.0);
			ca.IsUserSelectionEnabled = true;
			ca.IntervalOffsetType = DateTimeIntervalType.Number;
			ca.SelectionColor = StyleUtils.ForegroundHighlight;

			legendIntervals = new Legend(IntervalLegendName)
			{
				Alignment = StringAlignment.Near,
				BackColor = Color.Transparent,
				IsDockedInsideChartArea = false,
				Docking = Docking.Right,
				LegendStyle = LegendStyle.Column,
				IsTextAutoFit = true,
				BorderColor = StyleUtils.Foreground,
				BorderWidth = 1,
				AutoFitMinFontSize = 5,
				MaximumAutoSize = 25f,
			};
		}

		public void LoadSeries(DeviceWorkIntervalLookup lookup, IEnumerable<WorkOrProjectWithParentNames> works, Interval maxBounds)
		{
			currentLookup = lookup;
			currentWorks = works.ToArray();
			currentBounds = maxBounds;
			RedrawChart();
		}

		public void SetSelection(DateTime start, DateTime end)
		{
			SetSelectionPrecision(1);
			areaIntervals.CursorY.SetCursorPosition(end.ToOADate());
			areaIntervals.CursorY.SetSelectionPosition(start.ToOADate(), end.ToOADate());
		}

		public DeviceWorkInterval GetDeviceWorkInterval(Series series, int pointIndex)
		{
			if (pointIndex < 0 || pointIndex >= series.Points.Count) return null;
			return series.Points[pointIndex].Tag as DeviceWorkInterval;
		}

		public bool AddRow(string label, IEnumerable<DeviceWorkInterval> works)
		{
			var hasLabel = false;
			foreach (var work in works)
			{
				if (!seriesByWorkId.ContainsKey(work.WorkId)) continue;
				if (!work.IsVisible) continue;
				if (!isDeletedVisible && work.IsDeleted) continue;
				AddWorkToChart(label, work, ref hasLabel);
			}

			return hasLabel;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				SetSelectionPrecision(SelectionMinutePrecision * 60);
				LastSelectionStart = DateTime.FromOADate(areaIntervals.AxisY.PixelPositionToValue(e.X));
			}

			base.OnMouseDown(e);
		}

		private void ClearChart()
		{
			Series.Clear();
			areaIntervals.AxisX.CustomLabels.Clear();
			seriesByWorkId.Clear();
			deviceByXCoord.Clear();
			workNameById.Clear();
			legendIntervals.CustomItems.Clear();
			Legends.Clear();
			Legends.Add(legendIntervals);
			isEmpty = true;
			areaIntervals.AxisX.Enabled = AxisEnabled.False;
			xValue = 0;
		}

		private class TimeStats
		{
			public TimeStats()
			{
				Total = new TimeSpan();
				Computer= new TimeSpan();
				Ivr = new TimeSpan();
				Mobile = new TimeSpan();
				Manual = new TimeSpan();
				Meeting = new TimeSpan();
				Other = new TimeSpan();
			}

			public TimeSpan Total { get; set; }
			public TimeSpan Computer { get; set; }
			public TimeSpan Ivr { get; set; }
			public TimeSpan Mobile { get; set; }
			public TimeSpan Manual { get; set; }
			public TimeSpan Meeting { get; set; }
			public TimeSpan Other { get; set; }
		}

		private void RedrawChart()
		{
			if (currentWorks == null) return;
			ClearChart();
			var w = new Dictionary<int, Series>();
			var t = new Dictionary<Series, TimeStats>();
			Series def = null;
			var usedColors = new HashSet<int>();
			var spareIdx = 0;
			foreach (var work in currentWorks)
			{
				Debug.Assert(work.WorkOrProjectName.Id != null || work.WorkOrProjectName.ProjectId != null);
				var workId = work.WorkOrProjectName.Id ?? work.WorkOrProjectName.ProjectId.Value;
				var workName = work.FullName;
				workNameById.Add(workId, workName);
				Series series = null;
				if (groupByWork)
				{
					var colIdx = workId % StyleUtils.ColorPalette.Length;
					Color color;
					if (usedColors.Contains(colIdx))
					{
						color = StyleUtils.SpareColors[spareIdx++];
						if (spareIdx >= StyleUtils.SpareColors.Length) spareIdx = 0;
					}
					else
					{
						color = StyleUtils.ColorPalette[colIdx];
						usedColors.Add(colIdx);
					}
					series = new Series()
					{
						LegendText = workName,
						ChartType = SeriesChartType.RangeBar,
						Color = color,
						ChartArea = IntervalAreaName,
						Legend = IntervalLegendName,
						IsVisibleInLegend = false,
						YValueType = ChartValueType.DateTime,
						YValuesPerPoint = 2,
						ToolTip = workName,
						LegendToolTip = workName,
						LabelToolTip = "#PERCENT",
					};

					series["PointWidth"] = "0.9";
					series["DrawSideBySide"] = "false";
					Series.Add(series);
					t.Add(series, new TimeStats());
				}
				else
				{
					if (work.WorkOrProjectName.CategoryId != null)
					{
						var cat = work.WorkOrProjectName.CategoryId.Value;
						if (!w.ContainsKey(cat))
						{
							DebugEx.EnsureGuiThread();
							var catName = "??? (" + cat + ")";
							CategoryData dat;
							if (MenuQuery.Instance.ClientMenuLookup.Value.AllCategoriesById.TryGetValue(cat, out dat))
							{
								catName = dat.Name;
							}

							w.Add(cat, new Series()
							{
								LegendText = catName,
								ChartType = SeriesChartType.RangeBar,
								Color = StyleUtils.GetColor(cat),
								ChartArea = IntervalAreaName,
								Legend = IntervalLegendName,
								IsVisibleInLegend = false,
								YValueType = ChartValueType.DateTime,
								YValuesPerPoint = 2,
								ToolTip = workName,
								LegendToolTip = catName,
								LabelToolTip = "#PERCENT",
							});
							w[cat]["PointWidth"] = "0.9";
							w[cat]["DrawSideBySide"] = "false";
							Series.Add(w[cat]);
							t.Add(w[cat], new TimeStats());
						}

						series = w[cat];
					}
					else
					{
						if (def == null)
						{
							def = new Series()
							{
								LegendText = Labels.Worktime_NoCategory,
								ChartType = SeriesChartType.RangeBar,
								Color = StyleUtils.GetColor(0),
								ChartArea = IntervalAreaName,
								Legend = IntervalLegendName,
								IsVisibleInLegend = false,
								YValueType = ChartValueType.DateTime,
								YValuesPerPoint = 2,
								ToolTip = Labels.Worktime_NoCategory,
								LegendToolTip = GetStats(currentLookup, workId),
								LabelToolTip = "#PERCENT",
							};
							def["PointWidth"] = "0.9";
							def["DrawSideBySide"] = "false";
							Series.Add(def);
							t.Add(def, new TimeStats());
						}

						series = def;
					}
				}

				var compTime = currentLookup.GetDeviceLength(workId, DeviceType.Computer);
				var ivrTime = currentLookup.GetDeviceLength(workId, DeviceType.Ivr);
				var mobTime = currentLookup.GetDeviceLength(workId, DeviceType.Mobile);
				var meetTime = currentLookup.GetDeviceLength(workId, DeviceType.Meeting);
				var manTime = currentLookup.GetDeviceLength(workId, DeviceType.Manual);
				var otherTime = currentLookup.GetDeviceLength(workId, DeviceType.Holiday) + currentLookup.GetDeviceLength(workId, DeviceType.SickLeave);
				var totalTime = compTime + ivrTime + mobTime + meetTime + manTime + otherTime;
				t[series].Computer += compTime;
				t[series].Ivr += ivrTime;
				t[series].Mobile += mobTime;
				t[series].Meeting += meetTime;
				t[series].Manual += manTime;
				t[series].Other += otherTime;
				t[series].Total += totalTime;
				seriesByWorkId.Add(workId, series);
			}

			foreach (var deviceTypeContainer in currentLookup.WorksByDevice)
			{
				var deviceType = deviceTypeContainer.Key;
				var deviceNum = 1;
				foreach (var deviceIdContainer in deviceTypeContainer.Value)
				{
					if (deviceIdContainer.Value.All(x => !x.IsVisible && (!isDeletedVisible || x.IsDeleted))) continue;
					AddRow(string.Format(GetLabelForDeviceType(deviceType), deviceNum++), deviceIdContainer.Value);
				}
			}

			foreach (var serie in t.Keys.OrderByDescending(x => t[x].Total))
			{
				Legends[0].CustomItems.Add(GetLegend(serie.Color, serie.LegendText, t[serie].Total, serie.LegendText + "    " + t[serie].Total.ToHourMinuteSecondString()));
			}

			/////////////////

			var localInterval = currentBounds.FromUtcToLocal();
			if (currentLookup.Bounds != null)
			{
				var localBoundStart = currentLookup.Bounds.StartDate.FromUtcToLocal();
				var localBoundEnd = currentLookup.Bounds.EndDate.FromUtcToLocal();
				var startTicks = Math.Max(localInterval.StartDate.Ticks, localBoundStart.AddHours(-1).Ticks);
				var endTicks = Math.Min(localInterval.EndDate.Ticks, localBoundEnd.AddHours(1).Ticks);
				ResizeArea(new DateTime(startTicks), new DateTime(endTicks));
			}
			else
			{
				ResizeArea(localInterval.StartDate, localInterval.EndDate);
			}
		}

		private string GetStats(DeviceWorkIntervalLookup lookup, int workId)
		{
			var totalComp = lookup.GetDeviceLength(workId, DeviceType.Computer);
			var totalMobile = lookup.GetDeviceLength(workId, DeviceType.Mobile);
			var totalIvr = lookup.GetDeviceLength(workId, DeviceType.Ivr);
			var totalManual = lookup.GetDeviceLength(workId, DeviceType.Manual);
			var totalMeeting = lookup.GetDeviceLength(workId, DeviceType.Meeting);
			var totalWork = totalComp + totalMobile + totalIvr + totalManual + totalMeeting;
			var workStatsBuilder = new StringBuilder();
			workStatsBuilder.Append(Labels.Worktime_Total).Append(": ").AppendLine(totalWork.ToHourMinuteSecondString());
			workStatsBuilder.AppendLine();
			if (totalComp.Ticks > 0) workStatsBuilder.Append(Labels.Worktime_ComputerTotal).Append(": ").AppendLine(totalComp.ToHourMinuteSecondString());
			if (totalMobile.Ticks > 0) workStatsBuilder.Append(Labels.Worktime_MobileTotal).Append(": ").AppendLine(totalMobile.ToHourMinuteSecondString());
			if (totalIvr.Ticks > 0) workStatsBuilder.Append(Labels.Worktime_Ivr).Append(": ").AppendLine(totalIvr.ToHourMinuteSecondString());
			if (totalManual.Ticks > 0) workStatsBuilder.Append(Labels.Worktime_Manual).Append(": ").AppendLine(totalManual.ToHourMinuteSecondString());
			if (totalMeeting.Ticks > 0) workStatsBuilder.Append(Labels.Worktime_Meeting).Append(": ").AppendLine(totalMeeting.ToHourMinuteSecondString());
			return workStatsBuilder.ToString();
		}

		private void SetSelectionPrecision(int seconds)
		{
			areaIntervals.CursorY.Interval = seconds / (24.0 * 3600.0);
		}

		private string GetLabelForDeviceType(DeviceType type)
		{
			switch (type)
			{
				case DeviceType.Computer:
					return Labels.Worktime_Computer;
				case DeviceType.Ivr:
					return Labels.Worktime_Ivr;
				case DeviceType.Manual:
					return Labels.Worktime_Manual;
				case DeviceType.Meeting:
					return Labels.Worktime_Meeting;
				case DeviceType.Mobile:
					return Labels.Worktime_Mobile;
				default:
					return Labels.Worktime_Manual;
			}
		}

		private void AddXLabel(string xAxisName, DeviceType device)
		{
			xValue++;
			var customLabel = areaIntervals.AxisX.CustomLabels.Add(xValue - 0.5, xValue + 0.5, xAxisName);
			customLabel.ToolTip = Labels.Worktime_Total + ": " + currentLookup.GetDeviceLength(device).ToHourMinuteSecondString();
			deviceByXCoord.Add(xValue, device);
		}

		private string GetLabel(DeviceWorkInterval workInterval)
		{
			switch (workInterval.DeviceType)
			{
				case DeviceType.Meeting:
					return string.Format("{0}: {1}\n{2}: {3}", Labels.AddMeeting_Subject, workInterval.Subject, Labels.AddMeeting_Description, workInterval.Description);
				case DeviceType.Manual:
					return string.Format("{0}: {1}", Labels.Worktime_ManualComment, workInterval.Description);
				default:
					return "";
			}
		}

		private LegendItem GetLegend(Color color, string name, TimeSpan length, string tooltip)
		{
			var legendItem = new LegendItem() { ImageStyle = LegendImageStyle.Rectangle, ToolTip = tooltip, Color = color, BorderWidth = 0};
			legendItem.Cells.Add(LegendCellType.SeriesSymbol, "", ContentAlignment.MiddleCenter);
			legendItem.Cells.Add(LegendCellType.Text, name, ContentAlignment.MiddleLeft);
			legendItem.Cells.Add(LegendCellType.Text, length.ToHourMinuteString(), ContentAlignment.MiddleRight);
			return legendItem;
		}

		private IEnumerable<string> GetStatuses(DeviceWorkInterval workInterval)
		{
			if (workInterval.IsPending) yield return Labels.Worktime_WaitApprove;
			if (workInterval.IsDeleted) yield return Labels.Worktime_Deleted;
			if (!workInterval.IsEditable) yield return Labels.Worktime_CantEdit;
		}

		private string GetStatusString(DeviceWorkInterval workInterval)
		{
			var stats = GetStatuses(workInterval).ToArray();
			return stats.Length > 0 ? "(" + string.Join(", ", stats) + ")\n" : "";
		}

		private void AddWorkToChart(string label, DeviceWorkInterval work, ref bool hasLabel)
		{
			Debug.Assert(seriesByWorkId.ContainsKey(work.WorkId)); // Guaranteed by caller
			var localStart = work.StartDate.FromUtcToLocal();
			var localEnd = work.EndDate.FromUtcToLocal();

			if (!hasLabel)
			{
				AddXLabel(label, work.DeviceType);
				hasLabel = true;
			}

			if (isEmpty)
			{
				areaIntervals.AxisX.Enabled = AxisEnabled.True;
				areaIntervals.AxisX.Maximum = float.NaN;
				areaIntervals.AxisX.Minimum = float.NaN;
				isEmpty = false;
			}

			Debug.Assert(workNameById.ContainsKey(work.WorkId));
			seriesByWorkId[work.WorkId].Points.Add(new DataPoint()
			{
				ToolTip = string.Format("{4}{2}\n{0} - {1}\n{3}", localStart.ToShortTimeString(), localEnd.ToShortTimeString(), workNameById[work.WorkId], GetLabel(work), GetStatusString(work)),
				YValues = new[] { localStart.ToOADate(), localEnd.ToOADate() },
				BackSecondaryColor = work.IsDeleted ? Color.White : Color.Black,
				BackHatchStyle = work.IsPending ? ChartHatchStyle.LightDownwardDiagonal : (work.IsDeleted ? ChartHatchStyle.WideDownwardDiagonal : ChartHatchStyle.None),
				BorderColor = seriesByWorkId[work.WorkId].Color,
				BorderDashStyle = ChartDashStyle.Dot,
				BorderWidth = work.IsEditable ? 0 : 2,
				Tag = work,
				XValue = xValue
			});
		}

		protected void ResizeArea(DateTime startDate, DateTime endDate)
		{
			if (startDate >= endDate) return;
			areaIntervals.AxisY.Minimum = startDate.ToOADate();
			areaIntervals.AxisY.Maximum = endDate.ToOADate();
			// hax to show chart properly when empty
			if (isEmpty)
			{
				var series = new Series()
				{
					ChartType = SeriesChartType.RangeBar,
					YValueType = ChartValueType.DateTime,
					IsVisibleInLegend = false
				};
				Series.Add(series);
				series.Points.AddXY(1.0, areaIntervals.AxisY.Minimum, areaIntervals.AxisY.Maximum);
				areaIntervals.AxisX.Maximum = 0.1f;
				areaIntervals.AxisX.Minimum = 0f;
			}
		}

	}
}

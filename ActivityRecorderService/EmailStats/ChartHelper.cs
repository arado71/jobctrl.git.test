using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.DataVisualization.Charting;
using Tct.ActivityRecorderService.Caching.Works;

namespace Tct.ActivityRecorderService.EmailStats
{
	public static class ChartHelper
	{
		public static byte[] ToPng(this Chart chart)
		{
			if (chart == null) return null;
			using (var stream = new MemoryStream())
			{
				chart.SaveImage(stream, ChartImageFormat.Png);
				return stream.ToArray();
			}
		}

		private class BarData
		{
			public double YValue { get; set; }
			public string Label { get; set; }
		}

		private static Chart CreateBarChart(List<BarData> barData, string labelFormat)
		{
			#region Chart
			var chart = new Chart
			{
				AntiAliasing = AntiAliasingStyles.All,
				TextAntiAliasingQuality = TextAntiAliasingQuality.High,
				Width = 1000,
				Height = barData.Count * 20 + 85,
				Palette = ChartColorPalette.BrightPastel,
				BackGradientStyle = GradientStyle.TopBottom,
				BackColor = Color.FromArgb(216, 212, 189),
				BackSecondaryColor = Color.FromArgb(244, 244, 235),
				BorderWidth = 2,
				BorderColor = Color.FromArgb(181, 64, 1),
				BorderlineDashStyle = ChartDashStyle.Solid,
				BorderSkin = { SkinStyle = BorderSkinStyle.Emboss, }, //nice rounded border
			};
			#endregion

			#region ChartAreas
			var areaData = new ChartArea("Data")
			{
				BackColor = Color.FromArgb(40, Color.Gray),
				AxisY =
				{
					LabelStyle = { Format = labelFormat, },
					MajorGrid = { LineColor = Color.FromArgb(30, 0, 0, 0), Enabled = true, },
				},
				AxisX =
				{
					MajorGrid = { Enabled = false, },
					MinorGrid = { Enabled = false, },
					Interval = 1, //don't skip any labels
				},
				Area3DStyle = { Enable3D = true, Inclination = 15, Rotation = 10, WallWidth = 0, LightStyle = LightStyle.Simplistic, PointDepth = 15 },
			};
			chart.ChartAreas.Add(areaData);

			#endregion

			#region Series - Data
			var seriesData = new Series("Data")
			{
				ChartType = SeriesChartType.Bar,
				ChartArea = areaData.Name,
				Color = Color.Red,
			};
			seriesData["DrawingStyle"] = "Cylinder";
			chart.Series.Add(seriesData);

			{
				foreach (var data in barData)
				{
					int idx = seriesData.Points.AddY(data.YValue);
					seriesData.Points[idx].AxisLabel = data.Label;
				}
			}
			#endregion

			return chart;
		}

		public static Chart CreateWorkWarnHeuristic(List<EmailStatsHelper.WorkWithProgress> workProgresses)
		{
			var works = workProgresses
				.Where(n => n.TimeAfterTargetWorkTime <= TimeSpan.Zero) //should be in CreateWorkWarnTargetTimeChart
				.Where(n => n.TimeAfterEndDate <= TimeSpan.Zero) //should be in CreateWorkWarnEndDateChart
				.Where(n =>
				{
					var pctShouldBeDone = 1 - (n.WorkingDaysLeft / (double)n.WorkingDaysCount);
					var done = n.TagetPctWorkTime;
					var effortNeeded = n.AvgWorkTimePerDayToCompletion;

					return n.AvgWorkTimePerDayToCompletion >= TimeSpan.FromHours(4)
						|| (n.WorkingDaysLeft <= 5 && n.TagetPctWorkTime < 0.1f)
						|| (pctShouldBeDone > 0.7 && n.TagetPctWorkTime < 0.1f);
				})
				.ToList();
			if (works.Count == 0) return null;

			#region Chart
			var chart = new Chart
			{
				AntiAliasing = AntiAliasingStyles.All,
				TextAntiAliasingQuality = TextAntiAliasingQuality.High,
				Width = 1000,
				Height = works.Count * 60 + 85,
				Palette = ChartColorPalette.BrightPastel,
				BackGradientStyle = GradientStyle.TopBottom,
				BackColor = Color.FromArgb(216, 212, 189),
				BackSecondaryColor = Color.FromArgb(244, 244, 235),
				BorderWidth = 2,
				BorderColor = Color.FromArgb(181, 64, 1),
				BorderlineDashStyle = ChartDashStyle.Solid,
				BorderSkin = { SkinStyle = BorderSkinStyle.Emboss, }, //nice rounded border
			};
			#endregion

			#region ChartAreas
			var areaWorks = new ChartArea("Works")
			{
				BackColor = Color.FromArgb(40, Color.Gray),
				AxisY2 =
				{
					LabelStyle = { Format = "{0} %", },
					Minimum = 0,
					Maximum = 100,
					MajorGrid = { LineColor = Color.FromArgb(30, 0, 0, 0), Enabled = true, },
				},
				AxisY =
				{
					LabelStyle = { Format = "{0.#} " + EmailStats.ChartHourPerDay, },
					MajorGrid = { LineColor = Color.FromArgb(30, 0, 0, 0), Enabled = true, },
				},
				AxisX =
				{
					MajorGrid = { Enabled = false, },
					MinorGrid = { Enabled = false, },
					Interval = 1, //don't skip any labels
				},
				Area3DStyle = { Enable3D = true, Inclination = 15, Rotation = 15, WallWidth = 0, LightStyle = LightStyle.Simplistic, PointDepth = 15, IsClustered = true, },
			};
			chart.ChartAreas.Add(areaWorks);
			#endregion

			#region Legends
			var legendWorks = new Legend("Works")
			{
				Alignment = StringAlignment.Near,
				BackColor = Color.Transparent,
				DockedToChartArea = areaWorks.Name,
				IsDockedInsideChartArea = false,
				Docking = Docking.Right,
				LegendStyle = LegendStyle.Column,
				IsTextAutoFit = true,
				ShadowColor = Color.FromArgb(128, 64, 64, 64),
				ShadowOffset = 2,
				BorderColor = Color.FromArgb(128, 64, 64, 64),
				BorderWidth = 1,
				AutoFitMinFontSize = 5,
				MaximumAutoSize = 25f,
			};
			chart.Legends.Add(legendWorks);
			#endregion

			#region Series - Data
			var seriesDone = new Series("WorksDone")
			{
				ChartType = SeriesChartType.StackedBar,
				ChartArea = areaWorks.Name,
				Legend = legendWorks.Name,
				Color = Color.FromArgb(128, Color.Green),
				YAxisType = AxisType.Secondary,
			};
			seriesDone["DrawingStyle"] = "Cylinder";
			seriesDone["StackedGroupName"] = "WorkPct";
			chart.Series.Add(seriesDone);

			var seriesShouldBeDone = new Series("WorksShouldBeDone")
			{
				ChartType = SeriesChartType.StackedBar,
				ChartArea = areaWorks.Name,
				Legend = legendWorks.Name,
				Color = Color.FromArgb(128, Color.Red),
				YAxisType = AxisType.Secondary,
			};
			seriesShouldBeDone["DrawingStyle"] = "Cylinder";
			seriesShouldBeDone["StackedGroupName"] = "WorkPct";
			chart.Series.Add(seriesShouldBeDone);

			var seriesRemaining = new Series("WorksRemaining")
			{
				ChartType = SeriesChartType.StackedBar,
				ChartArea = areaWorks.Name,
				Legend = legendWorks.Name,
				Color = Color.FromArgb(40, Color.Gray),
				YAxisType = AxisType.Secondary,
			};
			seriesRemaining["DrawingStyle"] = "Cylinder";
			seriesRemaining["StackedGroupName"] = "WorkPct";
			chart.Series.Add(seriesRemaining);


			var seriesEffortNeeded = new Series("WorksEffortNeeded")
			{
				ChartType = SeriesChartType.StackedBar,
				ChartArea = areaWorks.Name,
				Legend = legendWorks.Name,
				Color = Color.FromArgb(128, Color.Goldenrod),
				YAxisType = AxisType.Primary,
			};
			seriesEffortNeeded["DrawingStyle"] = "Cylinder";
			seriesEffortNeeded["StackedGroupName"] = "Effort";
			chart.Series.Add(seriesEffortNeeded);

			{
				foreach (var work in works)
				{
					const int pctScale = 100;
					var pctShouldBeDone = (work.WorkingDaysLeft == 0 ? 1 : 1 - (work.WorkingDaysLeft / (double)work.WorkingDaysCount)) * pctScale;
					var pctDone = (work.TagetPctWorkTime > 1 ? 1 : work.TagetPctWorkTime) * pctScale;
					var effortNeeded = work.AvgWorkTimePerDayToCompletion.TotalHours;

					int idx = seriesDone.Points.AddY(pctDone);
					seriesDone.Points[idx].AxisLabel = work.Name;
					seriesShouldBeDone.Points.AddY(pctDone > pctShouldBeDone ? 0 : pctShouldBeDone - pctDone);
					seriesRemaining.Points.AddY(pctDone > pctShouldBeDone ? pctScale - pctDone : pctScale - pctShouldBeDone);
					seriesEffortNeeded.Points.AddY(effortNeeded);
				}

				seriesDone.LegendText = EmailStats.ChartFinished;
				seriesShouldBeDone.LegendText = EmailStats.ChartTargetTime;
				seriesRemaining.LegendText = EmailStats.ChartRemainingTime;
				seriesEffortNeeded.LegendText = EmailStats.ChartEffortNeeded;
			}
			#endregion

			return chart;
		}


		public static Chart CreateWorkWarnTargetTimeChart(List<EmailStatsHelper.WorkWithProgress> workProgresses)
		{
			var works = workProgresses
			   .Where(n => n.TimeAfterTargetWorkTime > TimeSpan.Zero)
			   .OrderBy(n => n.TimeAfterTargetWorkTime)
			   .Select(n => new BarData() { Label = n.Name, YValue = n.TimeAfterTargetWorkTime.TotalHours })
			   .ToList();
			if (works.Count == 0) return null;
			return CreateBarChart(works, "{0} " + EmailStats.ChartHours);
		}

		public static Chart CreateWorkWarnEndDateChart(List<EmailStatsHelper.WorkWithProgress> workProgresses)
		{
			var works = workProgresses
			   .Where(n => n.TimeAfterEndDate > TimeSpan.Zero)
			   .OrderBy(n => n.TimeAfterEndDate)
			   .Select(n => new BarData() { Label = n.Name, YValue = n.TimeAfterEndDate.TotalDays })
			   .ToList();
			if (works.Count == 0) return null;
			return CreateBarChart(works, "{0} " + EmailStats.ChartDays);
		}

		internal static Chart CreateWorkIntervalsAndActivityChart(List<AggregateWorkItemInterval> intervals, List<WorkItem> workItems, List<ManualWorkItem> manualWorkItems, DateTime startDate, DateTime endDate, UserStatInfo userStatInfo)
		{
			return CreateWorkIntervalsAndActivityChart(intervals, workItems, manualWorkItems, Enumerable.Empty<MobileWorkItem>().ToList(), startDate, endDate, userStatInfo);
		}

		public static Chart CreateWorkIntervalsAndActivityChart(List<AggregateWorkItemInterval> intervals, List<WorkItem> workItems, List<ManualWorkItem> manualWorkItems, List<MobileWorkItem> mobileWorkItems, DateTime startDate, DateTime endDate, UserStatInfo userStatInfo)
		{
			//this is a waste of memory and cpu
			workItems = new List<WorkItem>(workItems);
			workItems.DeleteIntervals(manualWorkItems);
			intervals = new List<AggregateWorkItemInterval>(intervals);
			intervals.DeleteIntervals(manualWorkItems);
			mobileWorkItems = new List<MobileWorkItem>(mobileWorkItems);
			mobileWorkItems.DeleteIntervals(manualWorkItems);

			var minStartDate = DateTime.MaxValue;
			var maxEndDate = DateTime.MinValue;
			var hasActivity = workItems.Count != 0;

			#region Chart
			var chart = new Chart
			{
				AntiAliasing = AntiAliasingStyles.All,
				TextAntiAliasingQuality = TextAntiAliasingQuality.High,
				Width = 1000,
				Height = 480,
				Palette = ChartColorPalette.SemiTransparent,
				BackGradientStyle = GradientStyle.TopBottom,
				BackColor = Color.FromArgb(216, 212, 189),
				BackSecondaryColor = Color.FromArgb(244, 244, 235),
				BorderWidth = 2,
				BorderColor = Color.FromArgb(181, 64, 1),
				BorderlineDashStyle = ChartDashStyle.Solid,
				BorderSkin = { SkinStyle = BorderSkinStyle.Emboss, }, //nice rounded border
			};
			#endregion

			#region ChartAreas
			var areaIntervals = new ChartArea()
			{
				AxisY =
				{
					//Minimum = startDate.ToOADate(),
					//Maximum = endDate.ToOADate(),
					LabelStyle = { Format = "g", },
					MajorGrid = { LineColor = Color.FromArgb(100, 0, 0, 0), Enabled = true, IntervalType = DateTimeIntervalType.Hours, },
				},
				AxisX =
				{
					MajorGrid = { Enabled = false, },
					MinorGrid = { Enabled = false, },
					Interval = 1, //don't skip any labels
				},
			};
			chart.ChartAreas.Add(areaIntervals);

			var areaActivity = new ChartArea
			{
				AlignmentOrientation = AreaAlignmentOrientations.Vertical,
				AlignWithChartArea = areaIntervals.Name,
				AlignmentStyle = AreaAlignmentStyles.All,
				AxisY =
				{
					Minimum = 0,
					Maximum = 150,
					MajorGrid = { LineColor = Color.FromArgb(10, 0, 0, 0), Enabled = true, },
					LabelStyle = { Format = "#;#;" + EmailStats.CharKeyboardShort + "\\." },
				},
				AxisX =
				{
					//Minimum = startDate.ToOADate(),
					//Maximum = endDate.ToOADate(),
					LabelStyle = { Format = "g", },
					MajorGrid = { LineColor = Color.FromArgb(100, 0, 0, 0), Enabled = true, IntervalType = DateTimeIntervalType.Hours, },
				},
				AxisY2 =
				{
					Enabled = AxisEnabled.True,
					Minimum = 0,
					Maximum = 50,
					MajorGrid = { Enabled = false, },
					LabelStyle = { Format = "#;#;" + EmailStats.ChartMouse },
					//CustomLabels = { new CustomLabel(-0.5,0.5,"Eger",0, LabelMarkStyle.LineSideMark) },
				}
				//Area3DStyle =
				//{
				//    Enable3D = true,
				//    Rotation = 20,
				//    Perspective = 10,
				//    Inclination = 28,
				//    LightStyle = LightStyle.Simplistic,
				//    PointDepth = 1000,
				//    PointGapDepth = 500,
				//    WallWidth = 20
				//},
			};
			if (workItems.Any(w => w.MouseActivity > 500))
			{
				areaActivity.AxisY2.Maximum = 150*50;
				areaActivity.AxisY2.LabelAutoFitMaxFontSize = 6;
				areaActivity.AxisY2.LabelAutoFitMinFontSize = 6;
			}
			chart.ChartAreas.Add(areaActivity);
			#endregion

			#region Legends
			var legendActivity = new Legend("Activity")
			{
				Alignment = StringAlignment.Near,
				BackColor = Color.Transparent,
				DockedToChartArea = areaActivity.Name,
				IsDockedInsideChartArea = true,
				Docking = Docking.Top,
				LegendStyle = LegendStyle.Table,
				IsTextAutoFit = true,
				ShadowColor = Color.FromArgb(128, 64, 64, 64),
				ShadowOffset = 2,
			};
			chart.Legends.Add(legendActivity);

			var legendIntervals = new Legend("WorkIntervals")
			{
				Alignment = StringAlignment.Near,
				BackColor = Color.Transparent,
				//DockedToChartArea = areaIntervals.Name,
				IsDockedInsideChartArea = false,
				Docking = Docking.Right,
				LegendStyle = LegendStyle.Column,
				IsTextAutoFit = true,
				ShadowColor = Color.FromArgb(128, 64, 64, 64),
				ShadowOffset = 2,
				BorderColor = Color.FromArgb(128, 64, 64, 64),
				BorderWidth = 1,
				AutoFitMinFontSize = 5,
				MaximumAutoSize = 25f,
				//Position = { Height = 100f, Width = 30f },
			};
			chart.Legends.Add(legendIntervals);
			#endregion

			#region Series - Activity
			foreach (var workItemsOnComp in workItems.ToLookup(n => n.ComputerId))
			{
				var seriesKey = new Series(EmailStats.CharKeyboardActivityShort + " (" + workItemsOnComp.Key + ")")
								{
									LegendText = EmailStats.CharKeyboardActivityShort,
									ChartType = SeriesChartType.Area,
									ChartArea = areaActivity.Name,
									Legend = legendActivity.Name,
									YAxisType = AxisType.Primary,
								};
				chart.Series.Add(seriesKey);
				var seriesMouse = new Series(EmailStats.ChartMouseActivityShort + " (" + workItemsOnComp.Key + ")")
								{
									LegendText = EmailStats.ChartMouseActivityShort,
									ChartType = SeriesChartType.Area,
									ChartArea = areaActivity.Name,
									Legend = legendActivity.Name,
									YAxisType = AxisType.Secondary,
								};
				chart.Series.Add(seriesMouse);

				var minutelyAggregatedActivity = GetMinutelyAggregatedActivity(workItemsOnComp.ToList());
				minStartDate = minutelyAggregatedActivity.Count > 0 && minutelyAggregatedActivity[0].StartDate < minStartDate ? minutelyAggregatedActivity[0].StartDate : minStartDate;
				maxEndDate = minutelyAggregatedActivity.Count > 0 && minutelyAggregatedActivity[minutelyAggregatedActivity.Count - 1].EndDate > maxEndDate ? minutelyAggregatedActivity[minutelyAggregatedActivity.Count - 1].EndDate : maxEndDate;
				foreach (var activity in minutelyAggregatedActivity)
				{

					seriesKey.Points.AddXY(activity.EndDate, activity.KeyboardActivity);
					seriesMouse.Points.AddXY(activity.EndDate, activity.MouseActivity);
				}

				//var avgSeries = new Series("" + workItemsOnComp.Key)
				//{
				//    ChartType = SeriesChartType.Area,
				//    IsVisibleInLegend = false,
				//    ChartArea = areaActivity.Name,
				//};
				//chart.DataManipulator.FinancialFormula(FinancialFormula.MovingAverage, "10", seriesKey, avgSeries);
			}
			#endregion

			#region Series - Intervals
			var workIdSet = GetWorkIdSet(intervals, manualWorkItems, mobileWorkItems);
			var seriesByWorkId = new Dictionary<int, Series>();
			var usedColors = new HashSet<int>();
			var spareIdx = 0;
			foreach (var workId in workIdSet) //create a series for all distinct workIds
			{
				var colIdx = workId % colorPalette.Length;
				Color color;
				if (usedColors.Contains(colIdx))
				{
					color = spareColors[spareIdx++];
					if (spareIdx >= spareColors.Length) spareIdx = 0;
				}
				else
				{
					color = colorPalette[colIdx];
					usedColors.Add(colIdx);
				}
				var series = new Series()
				{
					LegendText = WorkHierarchyService.Instance.GetWorkNameWithProjects(workId, 60),
					ChartType = SeriesChartType.RangeBar,
					//AxisLabel = " ", //don't show 0 1 2 etc labels on X axis // but this messes up axis interval 1
					//BorderWidth = 1,
					//BorderColor = Color.FromArgb(80, 40, 40, 40),
					ChartArea = areaIntervals.Name,
					Legend = legendIntervals.Name,
					Color = color,
				};
				series["PointWidth"] = "0.9";
				series["DrawSideBySide"] = "false";
				chart.Series.Add(series);
				seriesByWorkId.Add(workId, series);
			}

			var xValue = 0;
			bool first;
			foreach (var intervalsByComp in intervals.ToLookup(n => n.ComputerId))
			{
				first = true;
				foreach (var intvalByWork in intervalsByComp.ToLookup(n => n.WorkId))
				{
					var series = seriesByWorkId[intvalByWork.Key];

					foreach (var itemInterval in intvalByWork)
					{
						AddIntervalToChart(EmailStats.ChartAxisComputer, itemInterval.StartDate, itemInterval.EndDate, areaIntervals, series, ref first, ref xValue, ref minStartDate, ref maxEndDate);
					}
				}
			}

			foreach (var intervalsByMobile in mobileWorkItems.ToLookup(n => n.Imei))
			{
				first = true;
				foreach (var intvalByWork in intervalsByMobile.ToLookup(n => n.WorkId))
				{
					var series = seriesByWorkId[intvalByWork.Key];

					foreach (var itemInterval in intvalByWork)
					{
						AddIntervalToChart(EmailStats.ChartAxisSmartphone, itemInterval.StartDate, itemInterval.EndDate, areaIntervals, series, ref first, ref xValue, ref minStartDate, ref maxEndDate);
					}
				}
			}

			first = true;
			foreach (var itemInterval in manualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddWork && (n.SourceId == (byte?)ManualWorkItemSourceEnum.ServerAdhocMeeting || n.SourceId == (byte?)ManualWorkItemSourceEnum.MeetingAdd)))
			{
				AddIntervalToChart(EmailStats.ChartAxisMeeting, itemInterval.StartDate, itemInterval.EndDate, areaIntervals, seriesByWorkId[itemInterval.WorkId ?? -1], ref first, ref xValue, ref minStartDate, ref maxEndDate);
			}

			first = true;
			foreach (var itemInterval in manualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddWork && (n.SourceId == null || (n.SourceId.Value != (byte)ManualWorkItemSourceEnum.MeetingAdd && n.SourceId.Value != (byte)ManualWorkItemSourceEnum.ServerAdhocMeeting))))
			{
				AddIntervalToChart(EmailStats.ChartAxisManual, itemInterval.StartDate, itemInterval.EndDate, areaIntervals, seriesByWorkId[itemInterval.WorkId ?? -1], ref first, ref xValue, ref minStartDate, ref maxEndDate);
			}

			//users don't care
			//first = true;
			//foreach (var itemInterval in manualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddHoliday))
			//{
			//    AddIntervalToChart(EmailStats.ChartAxisHoliday, itemInterval.StartDate, itemInterval.EndDate, areaIntervals, seriesByWorkId[itemInterval.WorkId ?? -1], ref first, ref xValue, ref minStartDate, ref maxEndDate);
			//}

			//first = true;
			//foreach (var itemInterval in manualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddSickLeave))
			//{
			//    AddIntervalToChart(EmailStats.ChartAxisSickLeave, itemInterval.StartDate, itemInterval.EndDate, areaIntervals, seriesByWorkId[itemInterval.WorkId ?? -1], ref first, ref xValue, ref minStartDate, ref maxEndDate);
			//}
			#endregion

			#region Axes scale
			//truncate to startDate, endDate if necessary
			minStartDate = startDate > minStartDate ? startDate : minStartDate;
			maxEndDate = endDate < maxEndDate ? endDate : maxEndDate;

			if (minStartDate > maxEndDate) return null; //nothing to display

			SetAxisLabelsAndScale(areaActivity.AxisX, minStartDate, maxEndDate, userStatInfo);
			SetAxisLabelsAndScale(areaIntervals.AxisY, minStartDate, maxEndDate, userStatInfo); //don't know why the axes changed for RangeBars...

			if (!hasActivity)
			{
				areaActivity.Visible = false;
				chart.Height = 180;
			}
			#endregion

			return chart;
		}

		private static void AddIntervalToChart(string xAxisName, DateTime startDate, DateTime endDate, ChartArea areaIntervals, Series series, ref bool first, ref int xValue, ref DateTime minStartDate, ref DateTime maxEndDate)
		{
			minStartDate = startDate < minStartDate ? startDate : minStartDate;
			maxEndDate = endDate > maxEndDate ? endDate : maxEndDate;
			if (first)
			{
				xValue++;
				areaIntervals.AxisX.CustomLabels.Add(xValue - 0.5, xValue + 0.5, xAxisName);
				first = false;
			}
			series.Points.AddXY(xValue, startDate, endDate);
		}

		//http://betterdashboards.wordpress.com/tag/pie-charts/
		private const double detailedThresholdPct = 0.01;
		public static Chart CreateProcessChart(List<Reporter.Model.ProcessedItems.WorkItem> processedItems, TimeSpan computerWorkTime)
		{
			#region Chart
			var chart = new Chart
			{
				AntiAliasing = AntiAliasingStyles.All,
				TextAntiAliasingQuality = TextAntiAliasingQuality.High,
				Width = 700,
				Height = 300,
				Palette = ChartColorPalette.BrightPastel,
				BackGradientStyle = GradientStyle.TopBottom,
				BackColor = Color.FromArgb(216, 212, 189),
				BackSecondaryColor = Color.FromArgb(244, 244, 235),
				BorderWidth = 2,
				BorderColor = Color.FromArgb(181, 64, 1),
				BorderlineDashStyle = ChartDashStyle.Solid,
				BorderSkin = { SkinStyle = BorderSkinStyle.Emboss, }, //nice rounded border
			};
			#endregion

			#region ChartAreas
			var areaProcess = new ChartArea("Process")
			{
				BackColor = Color.Transparent,
				Area3DStyle = { Enable3D = true, Inclination = 30 }, //hax to avoid overlapping of labels
			};
			chart.ChartAreas.Add(areaProcess);
			#endregion

			#region Legends
			var legendProcess = new Legend("Processes")
			{
				Alignment = StringAlignment.Near,
				BackColor = Color.Transparent,
				DockedToChartArea = areaProcess.Name,
				IsDockedInsideChartArea = false,
				Docking = Docking.Right,
				LegendStyle = LegendStyle.Column,
				IsTextAutoFit = true,
				ShadowColor = Color.FromArgb(128, 64, 64, 64),
				ShadowOffset = 2,
				BorderColor = Color.FromArgb(128, 64, 64, 64),
				BorderWidth = 1,
				AutoFitMinFontSize = 5,
				MaximumAutoSize = 25f,
			};
			chart.Legends.Add(legendProcess);
			#endregion

			#region Series - Process
			var seriesProcess = new Series("Processes")
			{
				ChartType = SeriesChartType.Pie,
				ChartArea = areaProcess.Name,
				Legend = legendProcess.Name,
			};
			//show labels outside
			seriesProcess["PieLabelStyle"] = "Outside";
			//show lines to labels
			seriesProcess.BorderWidth = 1;
			seriesProcess.BorderColor = System.Drawing.Color.FromArgb(26, 59, 105);
			//nicer edges (for 2D)
			seriesProcess["PieDrawingStyle"] = "SoftEdge";
			chart.Series.Add(seriesProcess);

			{
				var processes = processedItems
					.GroupBy(n => n.Values.GetValueOrDefault("ProcessName"))
					.Select(n => new { ProcessName = n.Key, Duration = n.Sum(m => m.Duration.TotalMilliseconds) })
					.OrderByDescending(n => n.Duration)
					.ToList();
				var allDuration = processes.Sum(n => n.Duration);

				var remainingDuration = 0d;
				foreach (var process in processes)
				{
					var percentage = process.Duration / allDuration;
					if (percentage >= detailedThresholdPct && !string.IsNullOrEmpty(process.ProcessName))
					{
						var time = TimeSpan.FromMilliseconds(computerWorkTime.TotalMilliseconds * percentage); //this is not accurate at all...
						var idx = seriesProcess.Points.AddXY(process.ProcessName, time.TotalMilliseconds);
						seriesProcess.Points[idx].LegendText = "[" + time.ToHourMinuteString() + "] " + process.ProcessName;// +" (" + percentage.ToString("0.%") + ")";
					}
					else
					{
						remainingDuration += process.Duration;
					}
				}
				if (remainingDuration > 0)
				{
					var percentage = remainingDuration / allDuration;
					var time = TimeSpan.FromMilliseconds(computerWorkTime.TotalMilliseconds * percentage); //this is not accurate at all...
					var idx = seriesProcess.Points.AddXY(EmailStats.ChartOthers, time.TotalMilliseconds);
					seriesProcess.Points[idx].LegendText = "[" + time.ToHourMinuteString() + "] " + EmailStats.ChartOthers;// +" (" + percentage.ToString("0.%") + ")";
				}
			}
			#endregion

			return chart;
		}

		/*
		public static Chart CreateProcessChart2(List<ActiveWindowGroup> activeWindowsGroupped, TimeSpan computerWorkTime)
		{
			#region Chart
			var chart = new Chart
			{
				AntiAliasing = AntiAliasingStyles.All,
				TextAntiAliasingQuality = TextAntiAliasingQuality.High,
				Width = 1000,
				Height = 480,
				Palette = ChartColorPalette.BrightPastel,
				BackGradientStyle = GradientStyle.TopBottom,
				BackColor = Color.FromArgb(216, 212, 189),
				BackSecondaryColor = Color.FromArgb(244, 244, 235),
				BorderWidth = 2,
				BorderColor = Color.FromArgb(181, 64, 1),
				BorderlineDashStyle = ChartDashStyle.Solid,
				BorderSkin = { SkinStyle = BorderSkinStyle.Emboss, }, //nice rounded border
			};
			#endregion

			#region ChartAreas
			var areaProcess = new ChartArea("Process")
			{
				BackColor = Color.Transparent,
				Area3DStyle = { Enable3D = true, Inclination = 30 }, //hax to avoid overlapping of labels
			};
			chart.ChartAreas.Add(areaProcess);

			var areaTitle = new ChartArea("Title")
			{
				AlignmentOrientation = AreaAlignmentOrientations.Vertical,
				AlignWithChartArea = areaProcess.Name,
				AlignmentStyle = AreaAlignmentStyles.PlotPosition,
				BackColor = Color.Transparent,
				Area3DStyle = { Enable3D = true, Inclination = 30 }, //hax to avoid overlapping of labels
			};
			chart.ChartAreas.Add(areaTitle);
			#endregion

			#region Legends
			var legendProcess = new Legend("Processes")
			{
				Alignment = StringAlignment.Near,
				BackColor = Color.Transparent,
				DockedToChartArea = areaProcess.Name,
				IsDockedInsideChartArea = false,
				Docking = Docking.Right,
				LegendStyle = LegendStyle.Column,
				IsTextAutoFit = true,
				ShadowColor = Color.FromArgb(128, 64, 64, 64),
				ShadowOffset = 2,
				BorderColor = Color.FromArgb(128, 64, 64, 64),
				BorderWidth = 1,
				AutoFitMinFontSize = 5,
				MaximumAutoSize = 25f,
			};
			chart.Legends.Add(legendProcess);

			var legendTitle = new Legend("Titles")
			{
				Alignment = StringAlignment.Near,
				BackColor = Color.Transparent,
				DockedToChartArea = areaTitle.Name,
				IsDockedInsideChartArea = false,
				Docking = Docking.Right,
				LegendStyle = LegendStyle.Column,
				IsTextAutoFit = true,
				ShadowColor = Color.FromArgb(128, 64, 64, 64),
				ShadowOffset = 2,
				BorderColor = Color.FromArgb(128, 64, 64, 64),
				BorderWidth = 1,
				AutoFitMinFontSize = 5,
				MaximumAutoSize = 25f,
			};
			chart.Legends.Add(legendTitle);
			#endregion

			#region Series - Process
			var seriesProcess = new Series("Processes")
			{
				ChartType = SeriesChartType.Pie,
				ChartArea = areaProcess.Name,
				Legend = legendProcess.Name,
			};
			//show labels outside
			seriesProcess["PieLabelStyle"] = "Outside";
			//show lines to labels
			seriesProcess.BorderWidth = 1;
			seriesProcess.BorderColor = System.Drawing.Color.FromArgb(26, 59, 105);
			//nicer edges (for 2D)
			seriesProcess["PieDrawingStyle"] = "SoftEdge";
			chart.Series.Add(seriesProcess);

			{
				var processes = activeWindowsGroupped
					.GroupBy(n => n.ProcessName)
					.Select(n => new { ProcessName = n.Key, Count = n.Sum(m => m.Count) })
					.OrderByDescending(n => n.Count)
					.ToList();
				var allCount = processes.Sum(n => n.Count);

				var remainingCount = 0;
				foreach (var process in processes)
				{
					var percentage = process.Count / (double)allCount;
					if (percentage >= detailedThresholdPct)
					{
						var time = TimeSpan.FromMilliseconds(computerWorkTime.TotalMilliseconds * percentage); //this is not accurate at all...
						var idx = seriesProcess.Points.AddXY(process.ProcessName, time.TotalMilliseconds);
						seriesProcess.Points[idx].LegendText = "[" + time.ToHourMinuteString() + "] " + process.ProcessName;// +" (" + percentage.ToString("0.%") + ")";
					}
					else
					{
						remainingCount += process.Count;
					}
				}
				if (remainingCount > 0)
				{
					var percentage = remainingCount / (double)allCount;
					var time = TimeSpan.FromMilliseconds(computerWorkTime.TotalMilliseconds * percentage); //this is not accurate at all...
					var idx = seriesProcess.Points.AddXY("Egyéb", time.TotalMilliseconds);
					seriesProcess.Points[idx].LegendText = "[" + time.ToHourMinuteString() + "] " + "Egyéb";// +" (" + percentage.ToString("0.%") + ")";
				}
			}
			#endregion

			#region Series - Title
			var seriesTitle = new Series("Titles")
			{
				ChartType = SeriesChartType.Pie,
				ChartArea = areaTitle.Name,
				Legend = legendTitle.Name,
			};
			//show labels outside
			seriesTitle["PieLabelStyle"] = "Outside";
			//show lines to labels
			seriesTitle.BorderWidth = 1;
			seriesTitle.BorderColor = System.Drawing.Color.FromArgb(26, 59, 105);
			//nicer edges (for 2D)
			seriesTitle["PieDrawingStyle"] = "SoftEdge";
			chart.Series.Add(seriesTitle);

			{
				var processes = activeWindowsGroupped
					.OrderByDescending(n => n.Count)
					.ToList();
				var allCount = processes.Sum(n => n.Count);

				var remainingCount = 0;
				foreach (var process in processes)
				{
					var percentage = process.Count / (double)allCount;
					if (percentage >= detailedThresholdPct)
					{
						var time = TimeSpan.FromMilliseconds(computerWorkTime.TotalMilliseconds * percentage); //this is not accurate at all...
						var idx = seriesTitle.Points.AddXY(process.Title, time.TotalMilliseconds);
						seriesTitle.Points[idx].LegendText = "[" + time.ToHourMinuteString() + "] " + process.Title + "(" + process.ProcessName + ")";// +" (" + percentage.ToString("0.%") + ")";
					}
					else
					{
						remainingCount += process.Count;
					}
				}
				if (remainingCount > 0)
				{
					var percentage = remainingCount / (double)allCount;
					var time = TimeSpan.FromMilliseconds(computerWorkTime.TotalMilliseconds * percentage); //this is not accurate at all...
					var idx = seriesTitle.Points.AddXY("Egyéb", time.TotalMilliseconds);
					seriesTitle.Points[idx].LegendText = "[" + time.ToHourMinuteString() + "] " + "Egyéb";// +" (" + percentage.ToString("0.%") + ")";
				}
			}
			#endregion

			return chart;
		}
		*/

		private static void SetAxisLabelsAndScale(Axis axis, DateTime minStartDate, DateTime maxEndDate, UserStatInfo userStatInfo)
		{
			//I haven't found any better solution to align the scale of axes
			axis.Minimum = minStartDate.ToOADate();
			axis.Maximum = maxEndDate.ToOADate();

			//set grid interval to 1 hour
			axis.MajorGrid.IntervalType = DateTimeIntervalType.Hours;
			axis.MajorGrid.Interval = 1;
			axis.MajorTickMark.IntervalType = DateTimeIntervalType.Hours;
			axis.MajorTickMark.Interval = 1;

			var culture = new CultureInfo(string.IsNullOrEmpty(userStatInfo.CultureId) ? EmailStatsHelper.DefaultCulture : userStatInfo.CultureId);

			//convert axis labels to local time
			var firstWholeHour = new DateTime(minStartDate.Year, minStartDate.Month, minStartDate.Day, minStartDate.Hour, 00, 00);
			if (firstWholeHour < minStartDate) firstWholeHour = firstWholeHour.AddHours(1);

			bool hourlyLabels = firstWholeHour.AddHours(1) <= maxEndDate; //if we have at least two whole hours
			var startDate = hourlyLabels ? firstWholeHour : minStartDate;
			var labelDuration = hourlyLabels ? TimeSpan.FromHours(1) : new TimeSpan((maxEndDate.Ticks - minStartDate.Ticks) / (maxEndDate - minStartDate < TimeSpan.FromMinutes(5) ? 1 : 3));

			var format = "g";
			for (DateTime currDate = startDate; currDate <= maxEndDate; currDate = currDate.Add(labelDuration))
			{
				axis.CustomLabels.Add(
					currDate.AddTicks(-labelDuration.Ticks / 2).ToOADate(),
					currDate.AddTicks(labelDuration.Ticks / 2).ToOADate(),
					currDate.FromUtcToLocal(userStatInfo.TimeZone).ToString(format, culture));
				format = (currDate.FromUtcToLocal(userStatInfo.TimeZone).Day != currDate.Add(labelDuration).FromUtcToLocal(userStatInfo.TimeZone).Day)
					? "g"
					: "t";
			}
			Debug.Assert(axis.CustomLabels.Count > 1);
		}

		private static HashSet<int> GetWorkIdSet(List<AggregateWorkItemInterval> intervals, List<ManualWorkItem> manualWorkItems, List<MobileWorkItem> mobileWorkItems)
		{
			return new HashSet<int>(
				intervals.Select(n => n.WorkId)
				.Concat(mobileWorkItems.Select(n => n.WorkId))
				.Concat(manualWorkItems
					.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddHoliday
						|| n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddSickLeave
						|| n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddWork)
					.Select(n => n.WorkId ?? -1))
				.Distinct()
				);
		}

		private static List<AggregatedActivity> GetMinutelyAggregatedActivity(List<WorkItem> workItems)
		{
			var aggrBuilder = new AggregateActivityBuilder();
			aggrBuilder.AddWorkItems(workItems);
			//make sure the graph starts and ends at 0
			if (aggrBuilder.AggregatedInterval.HasValue)
			{
				var startDate = aggrBuilder.AggregatedInterval.Value.StartDate.AddMinutes(-1);
				var endDate = aggrBuilder.AggregatedInterval.Value.EndDate.AddMinutes(1);
				return aggrBuilder.GetMinutelyAggregatedActivity(startDate, endDate);
			}
			return new List<AggregatedActivity>();
		}

		private static readonly Color[] colorPalette =
		{
			Color.FromArgb(163, 59, 76),
			Color.FromArgb(234, 85, 109),
			Color.FromArgb(237, 110, 131),
			Color.FromArgb(240, 136, 153),
			Color.FromArgb(161, 79, 80),
			Color.FromArgb(197, 96, 98),
			Color.FromArgb(231, 113, 115),
			Color.FromArgb(235, 134, 136),
			Color.FromArgb(238, 156, 157),
			Color.FromArgb(154, 104, 84),
			Color.FromArgb(187, 127, 103),
			Color.FromArgb(220, 149, 121),
			Color.FromArgb(225, 165, 141),
			Color.FromArgb(231, 181, 161),
			Color.FromArgb(163, 108, 73),
			Color.FromArgb(234, 155, 105),
			Color.FromArgb(237, 170, 127),
			Color.FromArgb(240, 185, 150),
			Color.FromArgb(170, 109, 53),
			Color.FromArgb(208, 133, 65),
			Color.FromArgb(244, 156, 76),
			Color.FromArgb(246, 171, 103),
			Color.FromArgb(247, 186, 130),
			Color.FromArgb(172, 119, 42),
			Color.FromArgb(210, 145, 51),
			Color.FromArgb(247, 170, 60),
			Color.FromArgb(248, 183, 89),
			Color.FromArgb(249, 196, 119),
			Color.FromArgb(176, 134, 24),
			Color.FromArgb(214, 163, 30),
			Color.FromArgb(252, 192, 35),
			Color.FromArgb(253, 211, 101),
			Color.FromArgb(161, 141, 33),
			Color.FromArgb(196, 172, 40),
			Color.FromArgb(230, 202, 47),
			Color.FromArgb(234, 210, 78),
			Color.FromArgb(238, 218, 110),
			Color.FromArgb(135, 147, 31),
			Color.FromArgb(164, 179, 37),
			Color.FromArgb(193, 210, 44),
			Color.FromArgb(202, 217, 75),
			Color.FromArgb(212, 224, 108),
			Color.FromArgb(114, 137, 49),
			Color.FromArgb(139, 167, 60),
			Color.FromArgb(163, 196, 70),
			Color.FromArgb(191, 214, 126),
			Color.FromArgb(112, 163, 98),
			Color.FromArgb(132, 192, 115),
			Color.FromArgb(150, 201, 136),
			Color.FromArgb(169, 211, 157),
			Color.FromArgb(60, 124, 80),
			Color.FromArgb(73, 151, 98),
			Color.FromArgb(86, 178, 115),
			Color.FromArgb(111, 189, 136),
			Color.FromArgb(137, 201, 157),
			Color.FromArgb(12, 110, 80),
			Color.FromArgb(14, 134, 97),
			Color.FromArgb(17, 158, 114),
			Color.FromArgb(52, 172, 135),
			Color.FromArgb(89, 187, 157),
			Color.FromArgb(15, 113, 101),
			Color.FromArgb(19, 138, 123),
			Color.FromArgb(22, 162, 144),
			Color.FromArgb(57, 176, 161),
			Color.FromArgb(92, 190, 178),
			Color.FromArgb(20, 118, 133),
			Color.FromArgb(25, 144, 162),
			Color.FromArgb(29, 169, 190),
			Color.FromArgb(37, 110, 134),
			Color.FromArgb(45, 134, 163),
			Color.FromArgb(53, 158, 192),
			Color.FromArgb(83, 172, 201),
			Color.FromArgb(114, 187, 211),
			Color.FromArgb(71, 108, 145),
			Color.FromArgb(87, 132, 177),
			Color.FromArgb(102, 155, 208),
			Color.FromArgb(125, 170, 215),
			Color.FromArgb(148, 185, 222),
			Color.FromArgb(77, 101, 138),
			Color.FromArgb(110, 145, 197),
			Color.FromArgb(132, 161, 206),
			Color.FromArgb(154, 178, 215),
			Color.FromArgb(84, 90, 126),
			Color.FromArgb(103, 110, 154),
			Color.FromArgb(121, 129, 181),
			Color.FromArgb(141, 148, 192),
			Color.FromArgb(161, 167, 203),
			Color.FromArgb(112, 103, 150),
			Color.FromArgb(132, 121, 176),
			Color.FromArgb(150, 141, 188),
			Color.FromArgb(169, 161, 200),
			Color.FromArgb(103, 77, 117),
			Color.FromArgb(126, 94, 143),
			Color.FromArgb(148, 110, 168),
			Color.FromArgb(164, 132, 181),
			Color.FromArgb(180, 154, 194),
			Color.FromArgb(113, 75, 112),
			Color.FromArgb(138, 91, 136),
			Color.FromArgb(162, 107, 160),
			Color.FromArgb(176, 129, 174),
			Color.FromArgb(190, 152, 189),
			Color.FromArgb(128, 71, 103),
			Color.FromArgb(156, 87, 126),
			Color.FromArgb(183, 102, 148),
			Color.FromArgb(194, 125, 164),
			Color.FromArgb(205, 148, 180),
			Color.FromArgb(142, 66, 92),
			Color.FromArgb(173, 81, 112),
			Color.FromArgb(211, 119, 150),
			Color.FromArgb(219, 143, 169),
		};

		private static readonly Color[] spareColors =
		{
			Color.FromArgb(199, 72, 93),
			Color.FromArgb(199, 132, 89),
			Color.FromArgb(252, 201, 68),
			Color.FromArgb(177, 205, 98),
			Color.FromArgb(92, 134, 80),
			Color.FromArgb(63, 182, 200),
			Color.FromArgb(97, 195, 210),
			Color.FromArgb(94, 123, 168),
			Color.FromArgb(92, 84, 123),
			Color.FromArgb(203, 95, 132),
		};
	}
}

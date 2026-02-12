using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Stats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class IntervalWorkTimeStatsBuilderTests //very incomplete (copy pase from DetailedWorkTimeStatsBuilderTests)
	{
		#region Empty tests
		[Fact]
		public void EmptyStats()
		{
			var stats = new IntervalWorkTimeStatsBuilder();
			var result = stats.GetIntervalWorkTime(DateTime.MinValue, DateTime.MaxValue);
			Assert.Equal(DateTime.MinValue, result.StartDate);
			Assert.Equal(DateTime.MaxValue, result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.SumWorkTime);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTimeWithoutCorrection);
		}
		#endregion

		#region Computer tests
		[Fact]
		public void SimpleOverlappingCompIntervals()
		{
			var stats = new IntervalWorkTimeStatsBuilder();
			stats.RefreshAggregateWorkItemIntervals(new[] {
			new AggregateWorkItemInterval()
			{
				Id = 1,
				StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
			},
			new AggregateWorkItemInterval()
			{
				Id = 2,
				StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
			}});
			var result = stats.GetIntervalWorkTime(DateTime.MinValue, DateTime.MaxValue);
			Assert.Equal(DateTime.MinValue, result.StartDate);
			Assert.Equal(DateTime.MaxValue, result.EndDate);
			Assert.Equal(TimeSpan.FromHours(4), result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(3), result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(3), result.SumWorkTime);
			Assert.Equal(TimeSpan.Zero, result.ComputerCorrectionTime);
			Assert.Equal(TimeSpan.FromHours(4), result.ComputerWorkTimeWithoutCorrection);
		}

		[Fact]
		public void SimpleOverlappingCompIntervalsGetSmallerInterval()
		{
			var stats = new IntervalWorkTimeStatsBuilder();
			stats.RefreshAggregateWorkItemIntervals(new[] {
			new AggregateWorkItemInterval()
			{
				Id = 1,
				WorkId = 1,
				StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
			},
			new AggregateWorkItemInterval()
			{
				Id = 2,
				WorkId = 2,
				StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
			}});
			var result = stats.GetIntervalWorkTime(new DateTime(2010, 10, 04, 10, 20, 00), new DateTime(2010, 10, 04, 12, 20, 00));
			Assert.Equal(new DateTime(2010, 10, 04, 10, 20, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 04, 12, 20, 00), result.EndDate);
			Assert.Equal(TimeSpan.FromHours(3), result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(2), result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(2), result.SumWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(100), result.ComputerWorkTimeById[1]);
			Assert.Equal(TimeSpan.FromMinutes(80), result.ComputerWorkTimeById[2]);
			Assert.Equal(2, result.ComputerWorkTimeById.Count);
			Assert.Equal(0, result.IvrWorkTimeById.Count);
			Assert.Equal(TimeSpan.Zero, result.ComputerCorrectionTime);
			Assert.Equal(TimeSpan.FromHours(3), result.ComputerWorkTimeWithoutCorrection);

			result = stats.GetIntervalWorkTime(new DateTime(2010, 10, 03, 10, 30, 00), new DateTime(2010, 10, 03, 12, 30, 00));
			Assert.Equal(new DateTime(2010, 10, 03, 10, 30, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 03, 12, 30, 00), result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.SumWorkTime);
			Assert.Equal(0, result.ComputerWorkTimeById.Count);
			Assert.Equal(0, result.IvrWorkTimeById.Count);
			Assert.Equal(TimeSpan.Zero, result.ComputerCorrectionTime);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTimeWithoutCorrection);
		}

		[Fact]
		public void SimpleOverlappingCompIntervalsWithDeleteAndSmallerInterval()
		{
			var stats = new IntervalWorkTimeStatsBuilder();
			stats.RefreshAggregateWorkItemIntervals(new[] {
			new AggregateWorkItemInterval()
			{
				Id = 1,
				WorkId = 1,
				StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
			},
			new AggregateWorkItemInterval()
			{
				Id = 2,
				WorkId = 2,
				StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
			}});
			stats.RefreshManualWorkItems(new[] {
			new ManualWorkItem()
			{
				Id = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
				StartDate = new DateTime(2010, 10, 04, 10, 30, 00),
				EndDate = new DateTime(2010, 10, 04, 11, 30, 00),
			}});
			var result = stats.GetIntervalWorkTime(new DateTime(2010, 10, 04, 10, 20, 00), new DateTime(2010, 10, 04, 12, 20, 00));
			Assert.Equal(new DateTime(2010, 10, 04, 10, 20, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 04, 12, 20, 00), result.EndDate);
			Assert.Equal(TimeSpan.FromHours(1.5), result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(1), result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.SumWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(40), result.ComputerWorkTimeById[1]);
			Assert.Equal(TimeSpan.FromMinutes(50), result.ComputerWorkTimeById[2]);
			Assert.Equal(2, result.ComputerWorkTimeById.Count);
			Assert.Equal(0, result.IvrWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(-1.5), result.ComputerCorrectionTime);
			Assert.Equal(TimeSpan.FromHours(3), result.ComputerWorkTimeWithoutCorrection);
		}
		#endregion

		#region Manual tests
		[Fact]
		public void CanAddHoliday()
		{
			var stats = new IntervalWorkTimeStatsBuilder();
			stats.RefreshManualWorkItems(new[] { new ManualWorkItem()
			{
				Id = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddHoliday,
				StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
			}});

			var result = stats.GetIntervalWorkTime(new DateTime(2010, 10, 26, 00, 00, 00), new DateTime(2010, 10, 27, 00, 00, 00));
			Assert.Equal(new DateTime(2010, 10, 26, 00, 00, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 27, 00, 00, 00), result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(8), result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(8), result.ManuallyAddedTime);
			Assert.Equal(TimeSpan.Zero, result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.Zero, result.SickLeaveTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(8), result.SumWorkTime);
		}

		[Fact]
		public void CanAddSickLeave()
		{
			var stats = new IntervalWorkTimeStatsBuilder();
			stats.RefreshManualWorkItems(new[] { new ManualWorkItem()
			{
				Id = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddSickLeave,
				StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
			}});

			var result = stats.GetIntervalWorkTime(new DateTime(2010, 10, 26, 00, 00, 00), new DateTime(2010, 10, 27, 00, 00, 00));
			Assert.Equal(new DateTime(2010, 10, 26, 00, 00, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 27, 00, 00, 00), result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(8), result.ManuallyAddedTime);
			Assert.Equal(TimeSpan.Zero, result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.FromHours(8), result.SickLeaveTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(8), result.SumWorkTime);
		}

		[Fact]
		public void CanAddWorkTime()
		{
			var stats = new IntervalWorkTimeStatsBuilder();
			stats.RefreshManualWorkItems(new[] { new ManualWorkItem()
			{
				Id = 1,
				WorkId = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
				StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
			}});

			var result = stats.GetIntervalWorkTime(new DateTime(2010, 10, 26, 00, 00, 00), new DateTime(2010, 10, 27, 00, 00, 00));
			Assert.Equal(new DateTime(2010, 10, 26, 00, 00, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 27, 00, 00, 00), result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(8), result.ManuallyAddedTime);
			Assert.Equal(TimeSpan.FromHours(8), result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.Zero, result.SickLeaveTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(8), result.SumWorkTime);
		}

		[Fact]
		public void ManuallyAddedWorkTimeIsTruncated()
		{
			var stats = new IntervalWorkTimeStatsBuilder();
			stats.RefreshManualWorkItems(new[] { new ManualWorkItem()
			{
				Id = 1,
				WorkId = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
				StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
			}});

			var result = stats.GetIntervalWorkTime(new DateTime(2010, 10, 26, 12, 00, 00), new DateTime(2010, 10, 26, 14, 00, 00));
			Assert.Equal(new DateTime(2010, 10, 26, 12, 00, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 26, 14, 00, 00), result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(2), result.ManuallyAddedTime);
			Assert.Equal(TimeSpan.FromHours(2), result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.Zero, result.SickLeaveTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(2), result.SumWorkTime);
		}

		[Fact]
		public void HolidayIsTruncated()
		{
			var stats = new IntervalWorkTimeStatsBuilder();
			stats.RefreshManualWorkItems(new[] { new ManualWorkItem()
			{
				Id = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddHoliday,
				StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
			}});

			var result = stats.GetIntervalWorkTime(new DateTime(2010, 10, 26, 12, 00, 00), new DateTime(2010, 10, 26, 14, 00, 00));
			Assert.Equal(new DateTime(2010, 10, 26, 12, 00, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 26, 14, 00, 00), result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(2), result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(2), result.ManuallyAddedTime);
			Assert.Equal(TimeSpan.Zero, result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.Zero, result.SickLeaveTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(2), result.SumWorkTime);
		}

		[Fact]
		public void SickLeaveIsTruncated()
		{
			var stats = new IntervalWorkTimeStatsBuilder();
			stats.RefreshManualWorkItems(new[] { new ManualWorkItem()
			{
				Id = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddSickLeave,
				StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
			}});

			var result = stats.GetIntervalWorkTime(new DateTime(2010, 10, 26, 12, 00, 00), new DateTime(2010, 10, 26, 14, 00, 00));
			Assert.Equal(new DateTime(2010, 10, 26, 12, 00, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 26, 14, 00, 00), result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(2), result.ManuallyAddedTime);
			Assert.Equal(TimeSpan.Zero, result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.FromHours(2), result.SickLeaveTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(2), result.SumWorkTime);
		}
		#endregion

		#region Mobile tests
		[Fact]
		public void SimpleOverlappingMobileIntervals()
		{
			var stats = new IntervalWorkTimeStatsBuilder();
			stats.RefreshMobileWorkItems(new[] {
			new MobileWorkItem()
			{
				Id = 1,
				StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
			},
			new MobileWorkItem()
			{
				Id = 2,
				StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
			}});
			var result = stats.GetIntervalWorkTime(DateTime.MinValue, DateTime.MaxValue);
			Assert.Equal(DateTime.MinValue, result.StartDate);
			Assert.Equal(DateTime.MaxValue, result.EndDate);
			Assert.Equal(TimeSpan.FromHours(4), result.MobileWorkTime);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(3), result.NetMobileWorkTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(3), result.SumWorkTime);
			Assert.Equal(TimeSpan.Zero, result.ComputerCorrectionTime);
			Assert.Equal(TimeSpan.Zero, result.MobileCorrectionTime);
			Assert.Equal(TimeSpan.FromHours(4), result.MobileWorkTimeWithoutCorrection);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTimeWithoutCorrection);
		}

		[Fact]
		public void SimpleOverlappingMobileIntervalsGetSmallerInterval()
		{
			var stats = new IntervalWorkTimeStatsBuilder();
			stats.RefreshMobileWorkItems(new[] {
			new MobileWorkItem()
			{
				Id = 1,
				WorkId = 1,
				StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
			},
			new MobileWorkItem()
			{
				Id = 2,
				WorkId = 2,
				StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
			}});
			var result = stats.GetIntervalWorkTime(new DateTime(2010, 10, 04, 10, 20, 00), new DateTime(2010, 10, 04, 12, 20, 00));
			Assert.Equal(new DateTime(2010, 10, 04, 10, 20, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 04, 12, 20, 00), result.EndDate);
			Assert.Equal(TimeSpan.FromHours(3), result.MobileWorkTime);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(2), result.NetMobileWorkTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(2), result.SumWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(100), result.MobileWorkTimeById[1]);
			Assert.Equal(TimeSpan.FromMinutes(80), result.MobileWorkTimeById[2]);
			Assert.Equal(2, result.MobileWorkTimeById.Count);
			Assert.Equal(0, result.ComputerWorkTimeById.Count);
			Assert.Equal(0, result.IvrWorkTimeById.Count);
			Assert.Equal(TimeSpan.Zero, result.ComputerCorrectionTime);
			Assert.Equal(TimeSpan.Zero, result.MobileCorrectionTime);
			Assert.Equal(TimeSpan.FromHours(3), result.MobileWorkTimeWithoutCorrection);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTimeWithoutCorrection);

			result = stats.GetIntervalWorkTime(new DateTime(2010, 10, 03, 10, 30, 00), new DateTime(2010, 10, 03, 12, 30, 00));
			Assert.Equal(new DateTime(2010, 10, 03, 10, 30, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 03, 12, 30, 00), result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.MobileWorkTime);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.Zero, result.NetMobileWorkTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.SumWorkTime);
			Assert.Equal(0, result.MobileWorkTimeById.Count);
			Assert.Equal(0, result.ComputerWorkTimeById.Count);
			Assert.Equal(0, result.IvrWorkTimeById.Count);
			Assert.Equal(TimeSpan.Zero, result.MobileCorrectionTime);
			Assert.Equal(TimeSpan.Zero, result.ComputerCorrectionTime);
			Assert.Equal(TimeSpan.Zero, result.MobileWorkTimeWithoutCorrection);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTimeWithoutCorrection);
		}

		[Fact]
		public void SimpleOverlappingMobileIntervalsWithDeleteAndSmallerInterval()
		{
			var stats = new IntervalWorkTimeStatsBuilder();
			stats.RefreshMobileWorkItems(new[] {
			new MobileWorkItem()
			{
				Id = 1,
				WorkId = 1,
				StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
			},
			new MobileWorkItem()
			{
				Id = 2,
				WorkId = 2,
				StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
			}});
			stats.RefreshManualWorkItems(new[] { new ManualWorkItem()
			{
				Id = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
				StartDate = new DateTime(2010, 10, 04, 10, 30, 00),
				EndDate = new DateTime(2010, 10, 04, 11, 30, 00),
			}});
			var result = stats.GetIntervalWorkTime(new DateTime(2010, 10, 04, 10, 20, 00), new DateTime(2010, 10, 04, 12, 20, 00));
			Assert.Equal(new DateTime(2010, 10, 04, 10, 20, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 04, 12, 20, 00), result.EndDate);
			Assert.Equal(TimeSpan.FromHours(1.5), result.MobileWorkTime);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(1), result.NetMobileWorkTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.SumWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(40), result.MobileWorkTimeById[1]);
			Assert.Equal(TimeSpan.FromMinutes(50), result.MobileWorkTimeById[2]);
			Assert.Equal(2, result.MobileWorkTimeById.Count);
			Assert.Equal(0, result.ComputerWorkTimeById.Count);
			Assert.Equal(0, result.IvrWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(-1.5), result.MobileCorrectionTime);
			Assert.Equal(TimeSpan.Zero, result.ComputerCorrectionTime);
			Assert.Equal(TimeSpan.FromHours(3), result.MobileWorkTimeWithoutCorrection);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTimeWithoutCorrection);
		}

		[Fact]
		public void SimpleOverlappingMobileIntervalsWithDeleteSpecificAndSmallerInterval()
		{
			var stats = new IntervalWorkTimeStatsBuilder();
			stats.RefreshMobileWorkItems(new[] {
			new MobileWorkItem()
			{
				Id = 1,
				WorkId = 1,
				StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
			},
			new MobileWorkItem()
			{
				Id = 2,
				WorkId = 2,
				StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
			}});
			stats.RefreshManualWorkItems(new[] { new ManualWorkItem()
			{
				Id = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteMobileInterval,
				StartDate = new DateTime(2010, 10, 04, 10, 30, 00),
				EndDate = new DateTime(2010, 10, 04, 11, 30, 00),
			}});
			var result = stats.GetIntervalWorkTime(new DateTime(2010, 10, 04, 10, 20, 00), new DateTime(2010, 10, 04, 12, 20, 00));
			Assert.Equal(new DateTime(2010, 10, 04, 10, 20, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 04, 12, 20, 00), result.EndDate);
			Assert.Equal(TimeSpan.FromHours(1.5), result.MobileWorkTime);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(1), result.NetMobileWorkTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.SumWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(40), result.MobileWorkTimeById[1]);
			Assert.Equal(TimeSpan.FromMinutes(50), result.MobileWorkTimeById[2]);
			Assert.Equal(2, result.MobileWorkTimeById.Count);
			Assert.Equal(0, result.ComputerWorkTimeById.Count);
			Assert.Equal(0, result.IvrWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(-1.5), result.MobileCorrectionTime);
			Assert.Equal(TimeSpan.Zero, result.ComputerCorrectionTime);
			Assert.Equal(TimeSpan.FromHours(3), result.MobileWorkTimeWithoutCorrection);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTimeWithoutCorrection);
		}
		#endregion


		#region Combined tests

		[Fact]
		public void CombinedIntervals()
		{
			var stats = new IntervalWorkTimeStatsBuilder();
			stats.RefreshMobileWorkItems(new[]
			{
				new MobileWorkItem() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T09:48:15"), EndDate = DateTime.Parse("2015-03-03T09:48:47"), },
				new MobileWorkItem() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T10:05:32"), EndDate = DateTime.Parse("2015-03-03T10:06:54"), },
				new MobileWorkItem() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T12:37:29"), EndDate = DateTime.Parse("2015-03-03T13:00:39"), },
				new MobileWorkItem() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:00:49"), EndDate = DateTime.Parse("2015-03-03T13:26:41"), },
				new MobileWorkItem() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:26:41"), EndDate = DateTime.Parse("2015-03-03T13:28:15"), },
				new MobileWorkItem() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:28:15"), EndDate = DateTime.Parse("2015-03-03T13:39:28"), },
				new MobileWorkItem() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:40:16"), EndDate = DateTime.Parse("2015-03-03T14:27:07"), },
				new MobileWorkItem() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:28:07"), EndDate = DateTime.Parse("2015-03-03T15:51:43"), },
				new MobileWorkItem() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T16:02:52"), EndDate = DateTime.Parse("2015-03-03T16:03:05"), },
				new MobileWorkItem() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T16:03:05"), EndDate = DateTime.Parse("2015-03-03T16:03:30"), },
				new MobileWorkItem() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T16:03:30"), EndDate = DateTime.Parse("2015-03-03T16:05:52"), },
				new MobileWorkItem() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T16:17:49"), EndDate = DateTime.Parse("2015-03-03T17:10:55"), },
				new MobileWorkItem() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T17:47:21"), EndDate = DateTime.Parse("2015-03-03T17:57:16"), },
				new MobileWorkItem() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T17:10:55"), EndDate = DateTime.Parse("2015-03-03T17:47:21"), },
				new MobileWorkItem() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T18:09:10"), EndDate = DateTime.Parse("2015-03-03T19:28:54"), },
			});
			stats.RefreshAggregateWorkItemIntervals(new[]
			{
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T12:41:12.383"), EndDate = DateTime.Parse("2015-03-03T12:41:56.423"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T12:41:56.423"), EndDate = DateTime.Parse("2015-03-03T13:15:35.653"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:15:35.653"), EndDate = DateTime.Parse("2015-03-03T13:24:34.557"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:34:42.157"), EndDate = DateTime.Parse("2015-03-03T13:35:08.007"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T13:35:08.007"), EndDate = DateTime.Parse("2015-03-03T13:38:55.720"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:38:55.720"), EndDate = DateTime.Parse("2015-03-03T13:40:18.010"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T13:40:18.010"), EndDate = DateTime.Parse("2015-03-03T13:41:05.077"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:41:05.077"), EndDate = DateTime.Parse("2015-03-03T13:41:27.713"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T13:41:27.713"), EndDate = DateTime.Parse("2015-03-03T13:42:21.720"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:42:21.720"), EndDate = DateTime.Parse("2015-03-03T13:44:39.547"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T13:44:39.547"), EndDate = DateTime.Parse("2015-03-03T13:45:33.540"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:45:33.540"), EndDate = DateTime.Parse("2015-03-03T13:46:15.580"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T13:46:15.580"), EndDate = DateTime.Parse("2015-03-03T13:46:29.060"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:46:29.060"), EndDate = DateTime.Parse("2015-03-03T13:46:36.627"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T13:46:36.627"), EndDate = DateTime.Parse("2015-03-03T13:46:47.250"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:46:47.250"), EndDate = DateTime.Parse("2015-03-03T13:47:56.343"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T13:47:56.343"), EndDate = DateTime.Parse("2015-03-03T13:48:00.710"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:48:00.710"), EndDate = DateTime.Parse("2015-03-03T13:50:28.257"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T13:50:28.257"), EndDate = DateTime.Parse("2015-03-03T13:57:56.260"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:57:56.260"), EndDate = DateTime.Parse("2015-03-03T13:58:17.537"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T13:58:17.537"), EndDate = DateTime.Parse("2015-03-03T13:58:21.297"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:58:21.297"), EndDate = DateTime.Parse("2015-03-03T13:58:26.697"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T13:58:26.697"), EndDate = DateTime.Parse("2015-03-03T13:58:29.893"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:58:29.893"), EndDate = DateTime.Parse("2015-03-03T13:58:44.760"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T13:58:44.760"), EndDate = DateTime.Parse("2015-03-03T13:58:47.927"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:58:47.927"), EndDate = DateTime.Parse("2015-03-03T14:00:07.143"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:00:07.143"), EndDate = DateTime.Parse("2015-03-03T14:12:29.757"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:12:29.757"), EndDate = DateTime.Parse("2015-03-03T14:13:38.210"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:13:38.210"), EndDate = DateTime.Parse("2015-03-03T14:14:06.353"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:14:06.353"), EndDate = DateTime.Parse("2015-03-03T14:14:13.667"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:14:13.667"), EndDate = DateTime.Parse("2015-03-03T14:19:37.447"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:19:45.233"), EndDate = DateTime.Parse("2015-03-03T14:19:45.920"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:19:45.920"), EndDate = DateTime.Parse("2015-03-03T14:20:44.077"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:20:44.077"), EndDate = DateTime.Parse("2015-03-03T14:23:47.830"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:23:47.830"), EndDate = DateTime.Parse("2015-03-03T14:23:55.583"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:23:55.583"), EndDate = DateTime.Parse("2015-03-03T14:24:08.190"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:24:08.190"), EndDate = DateTime.Parse("2015-03-03T14:25:09.013"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:25:09.013"), EndDate = DateTime.Parse("2015-03-03T14:26:31.023"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:26:31.023"), EndDate = DateTime.Parse("2015-03-03T14:26:34.410"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:26:34.410"), EndDate = DateTime.Parse("2015-03-03T14:26:38.403"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:26:38.403"), EndDate = DateTime.Parse("2015-03-03T14:26:46.577"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:26:46.577"), EndDate = DateTime.Parse("2015-03-03T14:28:24.560"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:28:24.560"), EndDate = DateTime.Parse("2015-03-03T14:28:27.057"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:28:27.057"), EndDate = DateTime.Parse("2015-03-03T14:32:46.830"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:32:46.830"), EndDate = DateTime.Parse("2015-03-03T14:33:37.653"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:33:37.653"), EndDate = DateTime.Parse("2015-03-03T14:36:15.840"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:36:15.840"), EndDate = DateTime.Parse("2015-03-03T14:36:20.253"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:36:20.253"), EndDate = DateTime.Parse("2015-03-03T14:36:45.463"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:36:45.463"), EndDate = DateTime.Parse("2015-03-03T14:37:50.080"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:37:50.080"), EndDate = DateTime.Parse("2015-03-03T14:38:40.080"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:38:40.080"), EndDate = DateTime.Parse("2015-03-03T14:38:53.400"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:38:53.400"), EndDate = DateTime.Parse("2015-03-03T14:39:33.383"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:39:33.383"), EndDate = DateTime.Parse("2015-03-03T14:39:46.660"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:39:46.660"), EndDate = DateTime.Parse("2015-03-03T14:40:09.607"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:40:09.607"), EndDate = DateTime.Parse("2015-03-03T14:41:27.217"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:41:27.217"), EndDate = DateTime.Parse("2015-03-03T14:41:44.393"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:41:44.393"), EndDate = DateTime.Parse("2015-03-03T14:41:48.277"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:41:48.277"), EndDate = DateTime.Parse("2015-03-03T14:43:15.577"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:43:15.577"), EndDate = DateTime.Parse("2015-03-03T14:43:17.777"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:43:17.777"), EndDate = DateTime.Parse("2015-03-03T14:45:09.487"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:45:09.487"), EndDate = DateTime.Parse("2015-03-03T14:45:12.140"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:45:12.140"), EndDate = DateTime.Parse("2015-03-03T14:45:13.107"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:45:13.107"), EndDate = DateTime.Parse("2015-03-03T14:46:06.070"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:46:06.070"), EndDate = DateTime.Parse("2015-03-03T14:46:09.190"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:46:09.190"), EndDate = DateTime.Parse("2015-03-03T14:51:42.080"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:54:48.767"), EndDate = DateTime.Parse("2015-03-03T14:54:49.093"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:54:49.093"), EndDate = DateTime.Parse("2015-03-03T14:57:43.020"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:57:43.020"), EndDate = DateTime.Parse("2015-03-03T14:57:45.530"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:57:45.530"), EndDate = DateTime.Parse("2015-03-03T14:58:08.417"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:58:08.417"), EndDate = DateTime.Parse("2015-03-03T14:58:10.600"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T14:58:10.600"), EndDate = DateTime.Parse("2015-03-03T15:03:42.227"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T15:08:09.920"), EndDate = DateTime.Parse("2015-03-03T15:08:11.340"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T15:08:11.340"), EndDate = DateTime.Parse("2015-03-03T15:13:56.880"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T15:13:56.880"), EndDate = DateTime.Parse("2015-03-03T15:14:52.057"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T15:14:52.057"), EndDate = DateTime.Parse("2015-03-03T15:15:26.333"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T15:15:26.333"), EndDate = DateTime.Parse("2015-03-03T15:15:31.463"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T15:15:31.463"), EndDate = DateTime.Parse("2015-03-03T15:15:46.393"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T15:15:46.393"), EndDate = DateTime.Parse("2015-03-03T15:16:18.250"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T15:16:18.250"), EndDate = DateTime.Parse("2015-03-03T15:16:44.770"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T15:16:44.770"), EndDate = DateTime.Parse("2015-03-03T15:17:12.570"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T15:17:12.570"), EndDate = DateTime.Parse("2015-03-03T15:17:18.077"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T15:17:18.077"), EndDate = DateTime.Parse("2015-03-03T15:22:43.617"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T15:34:03.800"), EndDate = DateTime.Parse("2015-03-03T15:34:09.307"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T15:34:09.307"), EndDate = DateTime.Parse("2015-03-03T15:34:12.787"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T15:34:12.787"), EndDate = DateTime.Parse("2015-03-03T15:34:45.953"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T15:34:45.953"), EndDate = DateTime.Parse("2015-03-03T15:35:26.713"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T15:35:26.713"), EndDate = DateTime.Parse("2015-03-03T15:35:58.103"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T15:35:58.103"), EndDate = DateTime.Parse("2015-03-03T15:36:00.303"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T15:36:00.303"), EndDate = DateTime.Parse("2015-03-03T15:36:04.297"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T15:36:04.297"), EndDate = DateTime.Parse("2015-03-03T15:53:26.850"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T16:44:25.087"), EndDate = DateTime.Parse("2015-03-03T16:58:29.317"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T17:03:11.397"), EndDate = DateTime.Parse("2015-03-03T17:03:11.660"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T17:03:11.660"), EndDate = DateTime.Parse("2015-03-03T17:03:56.653"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T17:03:56.653"), EndDate = DateTime.Parse("2015-03-03T17:06:34.417"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T17:06:34.417"), EndDate = DateTime.Parse("2015-03-03T17:13:59.503"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T17:47:32.023"), EndDate = DateTime.Parse("2015-03-03T17:47:32.147"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T17:47:32.147"), EndDate = DateTime.Parse("2015-03-03T17:54:01.510"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T17:55:42.520"), EndDate = DateTime.Parse("2015-03-03T17:55:42.847"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T17:55:42.847"), EndDate = DateTime.Parse("2015-03-03T18:01:01.307"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T19:23:35.800"), EndDate = DateTime.Parse("2015-03-03T19:23:36.300"), },
				new AggregateWorkItemInterval() { WorkId = 2960562, StartDate = DateTime.Parse("2015-03-03T19:23:36.300"), EndDate = DateTime.Parse("2015-03-03T19:23:44.007"), },
				new AggregateWorkItemInterval() { WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T19:23:44.007"), EndDate = DateTime.Parse("2015-03-03T19:24:07.640"), },
			});
			stats.RefreshManualWorkItems(new[]
			{
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)4, WorkId = 5411, StartDate = DateTime.Parse("2015-03-03T02:00:00"), EndDate = DateTime.Parse("2015-03-03T06:00:00"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)1, WorkId = null, StartDate = DateTime.Parse("2015-03-03T13:19:34.637"), EndDate = DateTime.Parse("2015-03-03T13:34:40.013"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)0, WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T13:19:34.637"), EndDate = DateTime.Parse("2015-03-03T13:34:40.013"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)3, WorkId = null, StartDate = DateTime.Parse("2015-03-03T13:19:34.637"), EndDate = DateTime.Parse("2015-03-03T13:24:34.637"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)1, WorkId = null, StartDate = DateTime.Parse("2015-03-03T14:14:37.507"), EndDate = DateTime.Parse("2015-03-03T14:19:43.780"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)0, WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:14:37.507"), EndDate = DateTime.Parse("2015-03-03T14:19:43.780"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)3, WorkId = null, StartDate = DateTime.Parse("2015-03-03T14:14:37.510"), EndDate = DateTime.Parse("2015-03-03T14:19:37.510"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)1, WorkId = null, StartDate = DateTime.Parse("2015-03-03T14:46:42.117"), EndDate = DateTime.Parse("2015-03-03T14:54:47.273"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)0, WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:46:42.117"), EndDate = DateTime.Parse("2015-03-03T14:54:47.273"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)3, WorkId = null, StartDate = DateTime.Parse("2015-03-03T14:46:42.117"), EndDate = DateTime.Parse("2015-03-03T14:51:42.117"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)1, WorkId = null, StartDate = DateTime.Parse("2015-03-03T14:58:42.263"), EndDate = DateTime.Parse("2015-03-03T15:08:08.027"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)0, WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T14:58:42.263"), EndDate = DateTime.Parse("2015-03-03T15:08:08.027"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)3, WorkId = null, StartDate = DateTime.Parse("2015-03-03T14:58:42.263"), EndDate = DateTime.Parse("2015-03-03T15:03:42.263"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)1, WorkId = null, StartDate = DateTime.Parse("2015-03-03T15:17:43.647"), EndDate = DateTime.Parse("2015-03-03T15:34:01.253"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)0, WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T15:17:43.647"), EndDate = DateTime.Parse("2015-03-03T15:34:01.253"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)3, WorkId = null, StartDate = DateTime.Parse("2015-03-03T15:17:43.647"), EndDate = DateTime.Parse("2015-03-03T15:22:43.647"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)1, WorkId = null, StartDate = DateTime.Parse("2015-03-03T16:53:29.347"), EndDate = DateTime.Parse("2015-03-03T17:03:09.557"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)0, WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T16:53:29.347"), EndDate = DateTime.Parse("2015-03-03T17:03:09.557"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)3, WorkId = null, StartDate = DateTime.Parse("2015-03-03T16:53:29.347"), EndDate = DateTime.Parse("2015-03-03T16:58:29.347"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)1, WorkId = null, StartDate = DateTime.Parse("2015-03-03T17:08:59.530"), EndDate = DateTime.Parse("2015-03-03T17:47:29.807"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)0, WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T17:08:59.530"), EndDate = DateTime.Parse("2015-03-03T17:47:29.807"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)3, WorkId = null, StartDate = DateTime.Parse("2015-03-03T17:08:59.530"), EndDate = DateTime.Parse("2015-03-03T17:13:59.530"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)1, WorkId = null, StartDate = DateTime.Parse("2015-03-03T17:49:01.540"), EndDate = DateTime.Parse("2015-03-03T17:55:40.307"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)0, WorkId = 15933, StartDate = DateTime.Parse("2015-03-03T17:49:01.540"), EndDate = DateTime.Parse("2015-03-03T17:55:40.307"), },
				new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)3, WorkId = null, StartDate = DateTime.Parse("2015-03-03T17:49:01.540"), EndDate = DateTime.Parse("2015-03-03T17:54:01.540"), },
			});

			var result = stats.GetIntervalWorkTime(new DateTime(2015, 03, 03, 02, 00, 00), new DateTime(2015, 03, 04, 02, 00, 00));
			Assert.Equal(374281570000, result.SumWorkTime.Ticks); //10:23:48.1570000
		}
		/*
		SELECT 
		'new MobileWorkItem() { WorkId = '+convert(varchar,[WorkId])+', StartDate = DateTime.Parse("'+convert(varchar, [StartDate], 126)+'"), EndDate = DateTime.Parse("'+convert(varchar, [EndDate], 126)+'"), },'
		  FROM [dbo].[MobileWorkItems]
		 WHERE [StartDate] < '2015-03-04'
		   AND [EndDate] > '2015-03-03'
		   AND [UserId] = 31

		SELECT 
		'new ManualWorkItem() { ManualWorkItemTypeId = (ManualWorkItemTypeEnum)'+convert(varchar,ManualWorkItemTypeId)+', WorkId = '+isnull(convert(varchar,[WorkId]),'null')+', StartDate = DateTime.Parse("'+convert(varchar, [StartDate], 126)+'"), EndDate = DateTime.Parse("'+convert(varchar, [EndDate], 126)+'"), },'
		  FROM [dbo].ManualWorkItems
		 WHERE [StartDate] < '2015-03-04'
		   AND [EndDate] > '2015-03-03'
		   AND [UserId] = 31

		 */
		#endregion
	}
}

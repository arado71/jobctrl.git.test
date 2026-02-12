using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Stats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class DetailedWorkTimeStatsBuilderTests //very incomplete
	{
		#region Empty tests
		[Fact]
		public void EmptyStats()
		{
			var stats = new DetailedWorkTimeStatsBuilder();
			var result = stats.GetDetailedWorkTime(DateTime.MinValue, DateTime.MaxValue);
			Assert.Equal(DateTime.MinValue, result.StartDate);
			Assert.Equal(DateTime.MaxValue, result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.SumWorkTime);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTimeWithoutCorrection);
			Assert.Equal(0, result.AllWorkTimeById.Count);
		}
		#endregion

		#region Computer tests
		[Fact]
		public void SimpleOverlappingCompIntervals()
		{
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddWorkItem(new WorkItem()
			{
				Id = 1,
				StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
			});
			stats.AddWorkItem(new WorkItem()
			{
				Id = 2,
				StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
			});
			var result = stats.GetDetailedWorkTime(DateTime.MinValue, DateTime.MaxValue);
			Assert.Equal(DateTime.MinValue, result.StartDate);
			Assert.Equal(DateTime.MaxValue, result.EndDate);
			Assert.Equal(TimeSpan.FromHours(4), result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(3), result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(3), result.SumWorkTime);
			Assert.Equal(TimeSpan.Zero, result.ComputerCorrectionTime);
			Assert.Equal(TimeSpan.FromHours(4), result.ComputerWorkTimeWithoutCorrection);
			Assert.Equal(1, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(4), result.AllWorkTimeById[0]);
		}

		[Fact]
		public void SimpleOverlappingCompIntervalsGetSmallerInterval()
		{
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddWorkItem(new WorkItem()
			{
				Id = 1,
				WorkId = 1,
				StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
			});
			stats.AddWorkItem(new WorkItem()
			{
				Id = 2,
				WorkId = 2,
				StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
			});
			var result = stats.GetDetailedWorkTime(new DateTime(2010, 10, 04, 10, 20, 00), new DateTime(2010, 10, 04, 12, 20, 00));
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
			Assert.Equal(2, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromMinutes(100), result.AllWorkTimeById[1]);
			Assert.Equal(TimeSpan.FromMinutes(80), result.AllWorkTimeById[2]);

			result = stats.GetDetailedWorkTime(new DateTime(2010, 10, 03, 10, 30, 00), new DateTime(2010, 10, 03, 12, 30, 00));
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
			Assert.Equal(0, result.AllWorkTimeById.Count);
		}

		[Fact]
		public void SimpleOverlappingCompIntervalsWithDeleteAndSmallerInterval()
		{
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddWorkItem(new WorkItem()
			{
				Id = 1,
				WorkId = 1,
				StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
			});
			stats.AddWorkItem(new WorkItem()
			{
				Id = 2,
				WorkId = 2,
				StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
			});
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
				StartDate = new DateTime(2010, 10, 04, 10, 30, 00),
				EndDate = new DateTime(2010, 10, 04, 11, 30, 00),
			});
			var result = stats.GetDetailedWorkTime(new DateTime(2010, 10, 04, 10, 20, 00), new DateTime(2010, 10, 04, 12, 20, 00));
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
			Assert.Equal(2, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromMinutes(40), result.AllWorkTimeById[1]);
			Assert.Equal(TimeSpan.FromMinutes(50), result.AllWorkTimeById[2]);
		}
		#endregion

		#region Manual tests
		[Fact]
		public void CanAddHoliday()
		{
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddHoliday,
				StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
			});

			var result = stats.GetDetailedWorkTime(new DateTime(2010, 10, 26, 00, 00, 00), new DateTime(2010, 10, 27, 00, 00, 00));
			Assert.Equal(new DateTime(2010, 10, 26, 00, 00, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 27, 00, 00, 00), result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(8), result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(8), result.ManuallyAddedTime);
			Assert.Equal(TimeSpan.Zero, result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.Zero, result.SickLeaveTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(8), result.SumWorkTime);
			Assert.Equal(1, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(8), result.AllWorkTimeById.Single().Value);
		}

		[Fact]
		public void CanAddSickLeave()
		{
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddSickLeave,
				StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
			});

			var result = stats.GetDetailedWorkTime(new DateTime(2010, 10, 26, 00, 00, 00), new DateTime(2010, 10, 27, 00, 00, 00));
			Assert.Equal(new DateTime(2010, 10, 26, 00, 00, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 27, 00, 00, 00), result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(8), result.ManuallyAddedTime);
			Assert.Equal(TimeSpan.Zero, result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.FromHours(8), result.SickLeaveTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(8), result.SumWorkTime);
			Assert.Equal(1, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(8), result.AllWorkTimeById.Single().Value);
		}

		[Fact]
		public void CanAddWorkTime()
		{
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 1,
				WorkId = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
				StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
			});

			var result = stats.GetDetailedWorkTime(new DateTime(2010, 10, 26, 00, 00, 00), new DateTime(2010, 10, 27, 00, 00, 00));
			Assert.Equal(new DateTime(2010, 10, 26, 00, 00, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 27, 00, 00, 00), result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(8), result.ManuallyAddedTime);
			Assert.Equal(TimeSpan.FromHours(8), result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.Zero, result.SickLeaveTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(8), result.SumWorkTime);
			Assert.Equal(1, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(8), result.AllWorkTimeById[1]);
		}

		[Fact]
		public void ManuallyAddedWorkTimeIsTruncated()
		{
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 1,
				WorkId = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
				StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
			});

			var result = stats.GetDetailedWorkTime(new DateTime(2010, 10, 26, 12, 00, 00), new DateTime(2010, 10, 26, 14, 00, 00));
			Assert.Equal(new DateTime(2010, 10, 26, 12, 00, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 26, 14, 00, 00), result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(2), result.ManuallyAddedTime);
			Assert.Equal(TimeSpan.FromHours(2), result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.Zero, result.SickLeaveTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(2), result.SumWorkTime);
			Assert.Equal(1, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(2), result.AllWorkTimeById[1]);
		}

		[Fact]
		public void HolidayIsTruncated()
		{
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddHoliday,
				StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
			});

			var result = stats.GetDetailedWorkTime(new DateTime(2010, 10, 26, 12, 00, 00), new DateTime(2010, 10, 26, 14, 00, 00));
			Assert.Equal(new DateTime(2010, 10, 26, 12, 00, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 26, 14, 00, 00), result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(2), result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(2), result.ManuallyAddedTime);
			Assert.Equal(TimeSpan.Zero, result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.Zero, result.SickLeaveTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(2), result.SumWorkTime);
			Assert.Equal(1, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(2), result.AllWorkTimeById.Single().Value);
		}

		[Fact]
		public void SickLeaveIsTruncated()
		{
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddSickLeave,
				StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
			});

			var result = stats.GetDetailedWorkTime(new DateTime(2010, 10, 26, 12, 00, 00), new DateTime(2010, 10, 26, 14, 00, 00));
			Assert.Equal(new DateTime(2010, 10, 26, 12, 00, 00), result.StartDate);
			Assert.Equal(new DateTime(2010, 10, 26, 14, 00, 00), result.EndDate);
			Assert.Equal(TimeSpan.Zero, result.ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(2), result.ManuallyAddedTime);
			Assert.Equal(TimeSpan.Zero, result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.FromHours(2), result.SickLeaveTime);
			Assert.Equal(TimeSpan.Zero, result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(2), result.SumWorkTime);
			Assert.Equal(1, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(2), result.AllWorkTimeById.Single().Value);
		}
		#endregion

		#region Mobile tests
		[Fact]
		public void SimpleOverlappingMobileIntervals()
		{
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddMobileWorkItem(new MobileWorkItem()
			{
				Id = 1,
				StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
			});
			stats.AddMobileWorkItem(new MobileWorkItem()
			{
				Id = 2,
				StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
			});
			var result = stats.GetDetailedWorkTime(DateTime.MinValue, DateTime.MaxValue);
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
			Assert.Equal(1, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(4), result.AllWorkTimeById[0]);
		}

		[Fact]
		public void SimpleOverlappingMobileIntervalsGetSmallerInterval()
		{
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddMobileWorkItem(new MobileWorkItem()
			{
				Id = 1,
				WorkId = 1,
				StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
			});
			stats.AddMobileWorkItem(new MobileWorkItem()
			{
				Id = 2,
				WorkId = 2,
				StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
			});
			var result = stats.GetDetailedWorkTime(new DateTime(2010, 10, 04, 10, 20, 00), new DateTime(2010, 10, 04, 12, 20, 00));
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
			Assert.Equal(2, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromMinutes(100), result.AllWorkTimeById[1]);
			Assert.Equal(TimeSpan.FromMinutes(80), result.AllWorkTimeById[2]);

			result = stats.GetDetailedWorkTime(new DateTime(2010, 10, 03, 10, 30, 00), new DateTime(2010, 10, 03, 12, 30, 00));
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
			Assert.Equal(0, result.AllWorkTimeById.Count);
		}

		[Fact]
		public void SimpleOverlappingMobileIntervalsWithDeleteAndSmallerInterval()
		{
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddMobileWorkItem(new MobileWorkItem()
			{
				Id = 1,
				WorkId = 1,
				StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
			});
			stats.AddMobileWorkItem(new MobileWorkItem()
			{
				Id = 2,
				WorkId = 2,
				StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
			});
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
				StartDate = new DateTime(2010, 10, 04, 10, 30, 00),
				EndDate = new DateTime(2010, 10, 04, 11, 30, 00),
			});
			var result = stats.GetDetailedWorkTime(new DateTime(2010, 10, 04, 10, 20, 00), new DateTime(2010, 10, 04, 12, 20, 00));
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
			Assert.Equal(2, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromMinutes(40), result.AllWorkTimeById[1]);
			Assert.Equal(TimeSpan.FromMinutes(50), result.AllWorkTimeById[2]);
		}

		[Fact]
		public void SimpleOverlappingMobileIntervalsWithDeleteSpecificAndSmallerInterval()
		{
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddMobileWorkItem(new MobileWorkItem()
			{
				Id = 1,
				WorkId = 1,
				StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
			});
			stats.AddMobileWorkItem(new MobileWorkItem()
			{
				Id = 2,
				WorkId = 2,
				StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
				EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
			});
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteMobileInterval,
				StartDate = new DateTime(2010, 10, 04, 10, 30, 00),
				EndDate = new DateTime(2010, 10, 04, 11, 30, 00),
			});
			var result = stats.GetDetailedWorkTime(new DateTime(2010, 10, 04, 10, 20, 00), new DateTime(2010, 10, 04, 12, 20, 00));
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
			Assert.Equal(2, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromMinutes(40), result.AllWorkTimeById[1]);
			Assert.Equal(TimeSpan.FromMinutes(50), result.AllWorkTimeById[2]);
		}
		#endregion

		#region Mixed tests
		[Fact]
		public void AllWorkTypesMergedAsOneIfWorkIdIsTheSame()
		{
			var now = new DateTime(2013, 03, 11);
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddMobileWorkItem(new MobileWorkItem()
			{
				Id = 1,
				WorkId = 1,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 1,
				WorkId = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddHoliday,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 2,
				WorkId = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddSickLeave,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 3,
				WorkId = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			stats.AddWorkItem(new WorkItem()
			{
				Id = 1,
				WorkId = 1,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			var result = stats.GetDetailedWorkTime(DateTime.MinValue, DateTime.MaxValue);
			Assert.Equal(DateTime.MinValue, result.StartDate);
			Assert.Equal(DateTime.MaxValue, result.EndDate);
			Assert.Equal(TimeSpan.FromHours(1), result.ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(1), result.SickLeaveTime);
			Assert.Equal(TimeSpan.FromHours(1), result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.MobileWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(3), result.SumWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.ComputerWorkTimeWithoutCorrection);
			Assert.Equal(1, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(5), result.AllWorkTimeById[1]);

			result = stats.GetDetailedWorkTime(now, now.AddHours(0.5));
			Assert.Equal(now, result.StartDate);
			Assert.Equal(now.AddHours(0.5), result.EndDate);
			Assert.Equal(TimeSpan.FromHours(0.5), result.ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.SickLeaveTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.MobileWorkTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(1.5), result.SumWorkTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.ComputerWorkTimeWithoutCorrection);
			Assert.Equal(1, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(2.5), result.AllWorkTimeById[1]);
		}

		[Fact]
		public void AllWorkTypesMergedAsOneIfWorkIdIsTheSameWithDelete()
		{
			var now = new DateTime(2013, 03, 11);
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddMobileWorkItem(new MobileWorkItem()
			{
				Id = 1,
				WorkId = 1,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 1,
				WorkId = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddHoliday,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 2,
				WorkId = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddSickLeave,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 3,
				WorkId = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			stats.AddWorkItem(new WorkItem()
			{
				Id = 1,
				WorkId = 1,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 4,
				WorkId = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
				StartDate = now.AddMinutes(10),
				EndDate = now.AddMinutes(40),
			});

			var result = stats.GetDetailedWorkTime(DateTime.MinValue, DateTime.MaxValue);
			Assert.Equal(DateTime.MinValue, result.StartDate);
			Assert.Equal(DateTime.MaxValue, result.EndDate);
			Assert.Equal(TimeSpan.FromHours(0.5), result.ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(1), result.SickLeaveTime);
			Assert.Equal(TimeSpan.FromHours(1), result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.MobileWorkTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(3), result.SumWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.ComputerWorkTimeWithoutCorrection);
			Assert.Equal(1, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(4), result.AllWorkTimeById[1]);
		}

		[Fact]
		public void AllWorkTypesMergedAsOneIfWorkIdIsTheSameNoManualWithDelete()
		{
			var now = new DateTime(2013, 03, 11);
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddMobileWorkItem(new MobileWorkItem()
			{
				Id = 1,
				WorkId = 1,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			stats.AddWorkItem(new WorkItem()
			{
				Id = 1,
				WorkId = 1,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 4,
				WorkId = 1,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
				StartDate = now.AddMinutes(10),
				EndDate = now.AddMinutes(40),
			});

			var result = stats.GetDetailedWorkTime(DateTime.MinValue, DateTime.MaxValue);
			Assert.Equal(DateTime.MinValue, result.StartDate);
			Assert.Equal(DateTime.MaxValue, result.EndDate);
			Assert.Equal(TimeSpan.FromHours(0.5), result.ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(0), result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(0), result.SickLeaveTime);
			Assert.Equal(TimeSpan.FromHours(0), result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.MobileWorkTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.SumWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.ComputerWorkTimeWithoutCorrection);
			Assert.Equal(1, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(1), result.AllWorkTimeById[1]);
		}

		[Fact]
		public void DifferentWorkIdsNotMerged()
		{
			var now = new DateTime(2013, 03, 11);
			var stats = new DetailedWorkTimeStatsBuilder();
			stats.AddMobileWorkItem(new MobileWorkItem()
			{
				Id = 1,
				WorkId = 1,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 1,
				WorkId = 2,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddHoliday,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 2,
				WorkId = 3,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddSickLeave,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			stats.AddManualWorkItem(new ManualWorkItem()
			{
				Id = 3,
				WorkId = 4,
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
				StartDate = now,
				EndDate = now.AddHours(1),
			});
			stats.AddWorkItem(new WorkItem()
			{
				Id = 1,
				WorkId = 5,
				StartDate = now,
				EndDate = now.AddHours(1),
			});

			var result = stats.GetDetailedWorkTime(DateTime.MinValue, DateTime.MaxValue);
			Assert.Equal(DateTime.MinValue, result.StartDate);
			Assert.Equal(DateTime.MaxValue, result.EndDate);
			Assert.Equal(TimeSpan.FromHours(1), result.ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(1), result.SickLeaveTime);
			Assert.Equal(TimeSpan.FromHours(1), result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.MobileWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(3), result.SumWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.ComputerWorkTimeWithoutCorrection);
			Assert.Equal(5, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(5).Ticks, result.AllWorkTimeById.Sum(n => n.Value.Ticks));

			result = stats.GetDetailedWorkTime(now, now.AddHours(0.5));
			Assert.Equal(now, result.StartDate);
			Assert.Equal(now.AddHours(0.5), result.EndDate);
			Assert.Equal(TimeSpan.FromHours(0.5), result.ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.HolidayTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.SickLeaveTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.MobileWorkTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.NetComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(1.5), result.SumWorkTime);
			Assert.Equal(TimeSpan.FromHours(0.5), result.ComputerWorkTimeWithoutCorrection);
			Assert.Equal(5, result.AllWorkTimeById.Count);
			Assert.Equal(TimeSpan.FromHours(2.5).Ticks, result.AllWorkTimeById.Sum(n => n.Value.Ticks));
		}
		#endregion
	}
}

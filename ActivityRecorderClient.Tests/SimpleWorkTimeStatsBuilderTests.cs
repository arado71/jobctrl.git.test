using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Stats;
using Xunit;
using WorkType = Tct.ActivityRecorderClient.Stats.WorkType;

namespace Tct.Tests.ActivityRecorderClient
{
	public class SimpleWorkTimeStatsBuilderTests
	{
		private static DateTime now = new DateTime(2014, 10, 22, 20, 00, 00);
		private const int workId = 2;

		[Fact]
		public void Add()
		{
			foreach (WorkType type in Enum.GetValues(typeof(WorkType)))
			{
				AddWithType(type);
			}
		}

		private static void AddWithType(WorkType workType)
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;

			//Act
			wtb.AddWorkInterval(new WorkInterval() { WorkType = workType, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(2) });

			//Assert
			Assert.NotNull(res);
			Assert.Equal(1, res.Stats.Count);
			Assert.Equal(TimeSpan.FromMinutes(2), res.Stats[workId].TotalWorkTime);
		}

		[Fact]
		public void AddComputerAndManual()
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;

			//Act
			wtb.AddWorkInterval(new WorkInterval() { WorkType = WorkType.Computer, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(2) });
			wtb.AddWorkInterval(new WorkInterval() { WorkType = WorkType.Manual, WorkId = workId + 1, StartDate = now, EndDate = now.AddMinutes(3) });

			//Assert
			Assert.NotNull(res);
			Assert.Equal(2, res.Stats.Count);
			Assert.Equal(TimeSpan.FromMinutes(2), res.Stats[workId].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(3), res.Stats[workId + 1].TotalWorkTime);
		}

		[Fact]
		public void DeleteEnd()
		{
			foreach (WorkType type in Enum.GetValues(typeof(WorkType)))
			{
				DeleteEndWithType(type);
			}
		}

		private static void DeleteEndWithType(WorkType type)
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;
			wtb.AddWorkInterval(new WorkInterval() { WorkType = type, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(10) });

			//Act
			wtb.DeleteWorkInterval(new IntervalWithType() { WorkType = type, StartDate = now.AddMinutes(2), EndDate = now.AddMinutes(10) });

			//Assert
			Assert.NotNull(res);
			Assert.Equal(1, res.Stats.Count);
			Assert.Equal(TimeSpan.FromMinutes(2), res.Stats[workId].TotalWorkTime);
		}

		[Fact]
		public void DeleteBegin()
		{
			foreach (WorkType type in Enum.GetValues(typeof(WorkType)))
			{
				DeleteBeginWithType(type);
			}
		}

		private static void DeleteBeginWithType(WorkType type)
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;
			wtb.AddWorkInterval(new WorkInterval() { WorkType = type, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(10) });

			//Act
			wtb.DeleteWorkInterval(new IntervalWithType() { WorkType = type, StartDate = now, EndDate = now.AddMinutes(8) });

			//Assert
			Assert.NotNull(res);
			Assert.Equal(1, res.Stats.Count);
			Assert.Equal(TimeSpan.FromMinutes(2), res.Stats[workId].TotalWorkTime);
		}

		[Fact]
		public void DeleteAll()
		{
			foreach (WorkType type in Enum.GetValues(typeof(WorkType)))
			{
				DeleteAllWithType(type);
			}
		}

		private static void DeleteAllWithType(WorkType type)
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;
			wtb.AddWorkInterval(new WorkInterval() { WorkType = type, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(10) });

			//Act
			wtb.DeleteWorkInterval(new IntervalWithType() { WorkType = type, StartDate = now.AddMinutes(-2), EndDate = now.AddMinutes(10) });

			//Assert
			Assert.NotNull(res);
			Assert.Equal(0, res.Stats.Count);
		}

		[Fact]
		public void DeleteMiddle()
		{
			foreach (WorkType type in Enum.GetValues(typeof(WorkType)))
			{
				DeleteMiddleWithType(type);
			}
		}

		private static void DeleteMiddleWithType(WorkType type)
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;
			wtb.AddWorkInterval(new WorkInterval() { WorkType = type, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(10) });

			//Act
			wtb.DeleteWorkInterval(new IntervalWithType() { WorkType = type, StartDate = now.AddMinutes(1), EndDate = now.AddMinutes(9) });

			//Assert
			Assert.NotNull(res);
			Assert.Equal(1, res.Stats.Count);
			Assert.Equal(TimeSpan.FromMinutes(2), res.Stats[workId].TotalWorkTime);
		}

		[Fact]
		public void UpdateDailyStats()
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;
			wtb.AddWorkInterval(new WorkInterval() { WorkType = WorkType.Computer, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(2) });
			wtb.AddWorkInterval(new WorkInterval() { WorkType = WorkType.Manual, WorkId = workId + 1, StartDate = now, EndDate = now.AddMinutes(3) });

			//Act
			wtb.UpdateDailyStats(new Dictionary<DateTime, DailyWorkTimeStats>()
			{
				{now.AddDays(-1).Date,
				new DailyWorkTimeStats() { Day = now.AddDays(-1).Date, TotalWorkTimeByWorkId = new Dictionary<int,TimeSpan>()
				{
					{workId+4, TimeSpan.FromMinutes(13)},
					{workId+5, TimeSpan.FromMinutes(23)},
				}}},
				{now.Date,
				new DailyWorkTimeStats() { Day = now.Date, PartialInterval = now.AddMinutes(1).TimeOfDay, TotalWorkTimeByWorkId = new Dictionary<int,TimeSpan>()
				{
					{workId+2, TimeSpan.FromMinutes(13)},
					{workId, TimeSpan.FromMinutes(23)},
				}}},
			});

			//Assert
			Assert.NotNull(res);
			Assert.Equal(5, res.Stats.Count);
			Assert.Equal(TimeSpan.FromMinutes(24), res.Stats[workId].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(2), res.Stats[workId + 1].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(13), res.Stats[workId + 2].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(13), res.Stats[workId + 4].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(23), res.Stats[workId + 5].TotalWorkTime);
		}

		[Fact]
		public void UpdateDailyStatsNoPartial()
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;
			wtb.AddWorkInterval(new WorkInterval() { WorkType = WorkType.Computer, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(2) });

			//Act
			wtb.UpdateDailyStats(new Dictionary<DateTime, DailyWorkTimeStats>()
			{
				{now.AddDays(-1).Date,
				new DailyWorkTimeStats() { Day = now.AddDays(-1).Date, TotalWorkTimeByWorkId = new Dictionary<int,TimeSpan>()
				{
					{workId+1, TimeSpan.FromMinutes(13)},
					{workId+2, TimeSpan.FromMinutes(23)},
				}}},
			});

			//Assert
			Assert.NotNull(res);
			Assert.Equal(3, res.Stats.Count);
			Assert.Equal(TimeSpan.FromMinutes(2), res.Stats[workId].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(13), res.Stats[workId + 1].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(23), res.Stats[workId + 2].TotalWorkTime);
		}

		[Fact]
		public void UpdateDailyStatsMorePartials()
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;
			wtb.AddWorkInterval(new WorkInterval() { WorkType = WorkType.Computer, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(2) });

			//Act
			wtb.UpdateDailyStats(new Dictionary<DateTime, DailyWorkTimeStats>()
			{
				{now.AddDays(-1).Date,
				new DailyWorkTimeStats() { Day = now.AddDays(-1).Date, PartialInterval = TimeSpan.FromHours(23.8), TotalWorkTimeByWorkId = new Dictionary<int,TimeSpan>()
				{
				}}},
				{now.AddDays(1).Date,
				new DailyWorkTimeStats() { Day = now.AddDays(1).Date, PartialInterval = TimeSpan.FromHours(2), TotalWorkTimeByWorkId = new Dictionary<int,TimeSpan>()
				{
					{workId+1, TimeSpan.FromMinutes(13)},
					{workId+2, TimeSpan.FromMinutes(23)},
				}}},
			});

			//Assert
			Assert.NotNull(res);
			Assert.Equal(3, res.Stats.Count);
			Assert.Equal(TimeSpan.FromMinutes(2), res.Stats[workId].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(13), res.Stats[workId + 1].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(23), res.Stats[workId + 2].TotalWorkTime);
		}

		[Fact]
		public void UpdateDailyStatsMorePartialsAggr()
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;
			wtb.AddWorkInterval(new WorkInterval() { WorkType = WorkType.Computer, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(2) });

			//Act
			wtb.UpdateDailyStats(new Dictionary<DateTime, DailyWorkTimeStats>()
			{
				{now.AddDays(-1).Date,
				new DailyWorkTimeStats() { Day = now.AddDays(-1).Date, PartialInterval = TimeSpan.FromHours(23.8), TotalWorkTimeByWorkId = new Dictionary<int,TimeSpan>()
				{
				}}},
				{now.AddDays(0).Date,
				new DailyWorkTimeStats() { Day = now.AddDays(0).Date, TotalWorkTimeByWorkId = new Dictionary<int,TimeSpan>()
				{
					{workId, TimeSpan.FromMinutes(2)},
				}}},
				{now.AddDays(1).Date,
				new DailyWorkTimeStats() { Day = now.AddDays(1).Date, PartialInterval = TimeSpan.FromHours(2), TotalWorkTimeByWorkId = new Dictionary<int,TimeSpan>()
				{
					{workId+1, TimeSpan.FromMinutes(13)},
					{workId+2, TimeSpan.FromMinutes(23)},
				}}},
			});

			//Assert
			Assert.NotNull(res);
			Assert.Equal(3, res.Stats.Count);
			Assert.Equal(TimeSpan.FromMinutes(2), res.Stats[workId].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(13), res.Stats[workId + 1].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(23), res.Stats[workId + 2].TotalWorkTime);
		}

		[Fact]
		public void UpdateDailyStatsMorePartialsAggrLocalAndServerBoth()
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;
			wtb.AddWorkInterval(new WorkInterval() { WorkType = WorkType.Computer, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(2) });
			wtb.AddWorkInterval(new WorkInterval() { WorkType = WorkType.Computer, WorkId = workId + 1, StartDate = now.AddDays(1).Date.AddHours(2), EndDate = now.AddDays(1).Date.AddHours(2).AddMinutes(3) });

			//Act
			wtb.UpdateDailyStats(new Dictionary<DateTime, DailyWorkTimeStats>()
			{
				{now.AddDays(-1).Date,
				new DailyWorkTimeStats() { Day = now.AddDays(-1).Date, PartialInterval = TimeSpan.FromHours(23.8), TotalWorkTimeByWorkId = new Dictionary<int,TimeSpan>()
				{
				}}},
				{now.AddDays(0).Date,
				new DailyWorkTimeStats() { Day = now.AddDays(0).Date, PartialInterval = now.TimeOfDay, TotalWorkTimeByWorkId = new Dictionary<int,TimeSpan>()
				{
					{workId, TimeSpan.FromMinutes(12)},
				}}},
				{now.AddDays(1).Date,
				new DailyWorkTimeStats() { Day = now.AddDays(1).Date, PartialInterval = TimeSpan.FromHours(2), TotalWorkTimeByWorkId = new Dictionary<int,TimeSpan>()
				{
					{workId+1, TimeSpan.FromMinutes(13)},
					{workId+2, TimeSpan.FromMinutes(23)},
				}}},
			});

			//Assert
			Assert.NotNull(res);
			Assert.Equal(3, res.Stats.Count);
			Assert.Equal(TimeSpan.FromMinutes(14), res.Stats[workId].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(16), res.Stats[workId + 1].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(23), res.Stats[workId + 2].TotalWorkTime);
		}

		[Fact]
		public void UpdateDailyStatsDeletedWorkTime()
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;
			wtb.AddWorkInterval(new WorkInterval() { WorkType = WorkType.Computer, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(2) });

			//Act
			wtb.UpdateDailyStats(new Dictionary<DateTime, DailyWorkTimeStats>() //worktime might be deleted on the website
			{
				{now.AddDays(0).Date,
				new DailyWorkTimeStats() { Day = now.Date, PartialInterval = now.AddMinutes(1).TimeOfDay, TotalWorkTimeByWorkId = new Dictionary<int,TimeSpan>()
				{
				}}},
			});

			//Assert
			Assert.NotNull(res);
			Assert.Equal(1, res.Stats.Count);
			Assert.Equal(TimeSpan.FromMinutes(1), res.Stats[workId].TotalWorkTime);
		}

		[Fact]
		public void DeleteCombined()
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;

			//Act
			wtb.AddWorkInterval(new WorkInterval() { WorkType = WorkType.Computer, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(10) });
			wtb.AddWorkInterval(new WorkInterval() { WorkType = WorkType.Manual, WorkId = workId + 1, StartDate = now, EndDate = now.AddMinutes(10) });
			wtb.AddWorkInterval(new WorkInterval() { WorkType = WorkType.Meeting, WorkId = workId + 2, StartDate = now, EndDate = now.AddMinutes(10) });

			wtb.DeleteWorkInterval(new IntervalWithType() { WorkType = WorkType.Computer, StartDate = now.AddMinutes(1), EndDate = now.AddMinutes(2) });
			wtb.DeleteWorkInterval(new IntervalWithType() { WorkType = WorkType.Manual, StartDate = now.AddMinutes(3), EndDate = now.AddMinutes(5) });
			wtb.DeleteWorkInterval(new IntervalWithType() { WorkType = WorkType.Meeting, StartDate = now.AddMinutes(6), EndDate = now.AddMinutes(9) });


			//Assert
			Assert.NotNull(res);
			Assert.Equal(3, res.Stats.Count);
			Assert.Equal(TimeSpan.FromMinutes(9), res.Stats[workId].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(8), res.Stats[workId + 1].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(7), res.Stats[workId + 2].TotalWorkTime);
		}

		[Fact]
		public void DeleteInvalid()
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;
			wtb.AddWorkInterval(new WorkInterval() { WorkType = WorkType.Computer, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(10) });

			//Act
			wtb.DeleteWorkInterval(new IntervalWithType() { WorkType = WorkType.Manual, StartDate = now.AddMinutes(-2), EndDate = now.AddMinutes(10) });

			//Assert
			Assert.NotNull(res);
			Assert.Equal(1, res.Stats.Count);
			Assert.Equal(TimeSpan.FromMinutes(10), res.Stats[workId].TotalWorkTime);
		}

		[Fact]
		public void ChangeWork()
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;

			var expected = TimeSpan.Zero;
			foreach (WorkType type in Enum.GetValues(typeof(WorkType)))
			{
				wtb.AddWorkInterval(new WorkInterval() { WorkType = type, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(10) });
				expected += TimeSpan.FromMinutes(10);
			}
			foreach (WorkType type in Enum.GetValues(typeof(WorkType)))
			{
				wtb.AddWorkInterval(new WorkInterval() { WorkType = type, WorkId = workId + 1, StartDate = now, EndDate = now.AddMinutes(10) });
				expected += TimeSpan.FromMinutes(10);
			}

			//Act
			wtb.ChangeWorkId(workId, workId + 2);
			wtb.ChangeWorkId(workId + 1, workId + 2);

			//Assert
			Assert.True(expected > TimeSpan.Zero);
			Assert.NotNull(res);
			Assert.Equal(1, res.Stats.Count);
			Assert.Equal(expected, res.Stats[workId + 2].TotalWorkTime);
		}

		[Fact]
		public void IntervalsAreMerged()
		{
			foreach (WorkType type in Enum.GetValues(typeof(WorkType)))
			{
				IntervalsAreMergedWithType(type);
			}
		}

		private void IntervalsAreMergedWithType(WorkType workType)
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;

			//Act
			wtb.AddWorkInterval(new WorkInterval() { WorkType = workType, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(2) });
			wtb.AddWorkInterval(new WorkInterval() { WorkType = workType, WorkId = workId, StartDate = now.AddMinutes(2), EndDate = now.AddMinutes(4) });

			//Assert
			Assert.NotNull(res);
			Assert.Equal(1, res.Stats.Count);
			Assert.Equal(TimeSpan.FromMinutes(4), res.Stats[workId].TotalWorkTime);
			var state = wtb.GetState();
			Assert.Equal(1, state.LocalWorkStats.WorkIntervalsByTypeByWorkId[workType][workId].Count);
			Assert.Equal(now, state.LocalWorkStats.WorkIntervalsByTypeByWorkId[workType][workId][0].StartDate);
			Assert.Equal(now.AddMinutes(4), state.LocalWorkStats.WorkIntervalsByTypeByWorkId[workType][workId][0].EndDate);
		}

		[Fact]
		public void IntervalsAreNotMergedForDiffWorkId()
		{
			foreach (WorkType type in Enum.GetValues(typeof(WorkType)))
			{
				IntervalsAreNotMergedForDiffWorkIdWithType(type);
			}
		}

		private void IntervalsAreNotMergedForDiffWorkIdWithType(WorkType workType)
		{
			//Arrange
			SimpleWorkTimeStats res = null;
			var wtb = new SimpleWorkTimeStatsBuilder();
			wtb.SimpleWorkTimeStatsCalculated += (_, e) => res = e.Value;

			//Act
			wtb.AddWorkInterval(new WorkInterval() { WorkType = workType, WorkId = workId, StartDate = now, EndDate = now.AddMinutes(2) });
			wtb.AddWorkInterval(new WorkInterval() { WorkType = workType, WorkId = workId + 1, StartDate = now.AddMinutes(2), EndDate = now.AddMinutes(4) });

			//Assert
			Assert.NotNull(res);
			Assert.Equal(2, res.Stats.Count);
			Assert.Equal(TimeSpan.FromMinutes(2), res.Stats[workId].TotalWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(2), res.Stats[workId + 1].TotalWorkTime);
			var state = wtb.GetState();
			Assert.Equal(1, state.LocalWorkStats.WorkIntervalsByTypeByWorkId[workType][workId].Count);
			Assert.Equal(now, state.LocalWorkStats.WorkIntervalsByTypeByWorkId[workType][workId][0].StartDate);
			Assert.Equal(now.AddMinutes(2), state.LocalWorkStats.WorkIntervalsByTypeByWorkId[workType][workId][0].EndDate);
			Assert.Equal(1, state.LocalWorkStats.WorkIntervalsByTypeByWorkId[workType][workId + 1].Count);
			Assert.Equal(now.AddMinutes(2), state.LocalWorkStats.WorkIntervalsByTypeByWorkId[workType][workId + 1][0].StartDate);
			Assert.Equal(now.AddMinutes(4), state.LocalWorkStats.WorkIntervalsByTypeByWorkId[workType][workId + 1][0].EndDate);
		}
	}
}

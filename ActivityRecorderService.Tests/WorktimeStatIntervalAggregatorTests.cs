using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService.Stats;
using Tct.ActivityRecorderService.WebsiteServiceReference;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class WorktimeStatIntervalAggregatorTests
	{
		[Fact]
		public void AddSimple()
		{
			//Prepare
			var aggr = new WorktimeStatIntervalAggregator();

			//Act
			var res1 = aggr.Add(1, 11, WorktimeStatIntervals.Today, DateTime.Today);
			var res2 = aggr.Add(1, 12, WorktimeStatIntervals.Today | WorktimeStatIntervals.Week, DateTime.Today);
			var res3 = aggr.Add(1, 13, WorktimeStatIntervals.Month, DateTime.Today);

			//Assert
			Assert.Equal(WorktimeStatIntervals.Today, res1);
			Assert.Equal(WorktimeStatIntervals.Today | WorktimeStatIntervals.Week, res2);
			Assert.Equal(WorktimeStatIntervals.Today | WorktimeStatIntervals.Week | WorktimeStatIntervals.Month, res3);
		}

		[Fact]
		public void AddThreeExpiredOne()
		{
			//Prepare
			var aggr = new WorktimeStatIntervalAggregator();

			//Act
			var res1 = aggr.Add(1, 11, WorktimeStatIntervals.Today, DateTime.Today.AddDays(-2));
			var res2 = aggr.Add(1, 12, WorktimeStatIntervals.Week, DateTime.Today);
			var res3 = aggr.Add(1, 13, WorktimeStatIntervals.Month, DateTime.Today);

			//Assert
			Assert.Equal(WorktimeStatIntervals.Today, res1);
			Assert.Equal(WorktimeStatIntervals.Week, res2);
			Assert.Equal(WorktimeStatIntervals.Week | WorktimeStatIntervals.Month, res3);
		}

		[Fact]
		public void AddFiveExpiredInSteps()
		{
			//Prepare
			var aggr = new WorktimeStatIntervalAggregator();

			//Act
			var res1 = aggr.Add(1, 11, WorktimeStatIntervals.Today, DateTime.Today);
			var res2 = aggr.Add(1, 12, WorktimeStatIntervals.Week, DateTime.Today.AddHours(13));
			var res3 = aggr.Add(1, 13, WorktimeStatIntervals.Month, DateTime.Today.AddHours(25));
			var res4 = aggr.Add(1, 14, WorktimeStatIntervals.Quarter, DateTime.Today.AddHours(36));
			var res5 = aggr.Add(1, 15, WorktimeStatIntervals.Year, DateTime.Today.AddHours(50));

			//Assert
			Assert.Equal(WorktimeStatIntervals.Today, res1);
			Assert.Equal(WorktimeStatIntervals.Today | WorktimeStatIntervals.Week, res2);
			Assert.Equal(WorktimeStatIntervals.Week | WorktimeStatIntervals.Month, res3);
			Assert.Equal(WorktimeStatIntervals.Week | WorktimeStatIntervals.Month | WorktimeStatIntervals.Quarter, res4);
			Assert.Equal(WorktimeStatIntervals.Quarter | WorktimeStatIntervals.Year, res5);
		}

		[Fact]
		public void AddSameComputer()
		{
			//Prepare
			var aggr = new WorktimeStatIntervalAggregator();

			//Act
			var res1 = aggr.Add(1, 11, WorktimeStatIntervals.Today, DateTime.Today);
			var res2 = aggr.Add(1, 11, WorktimeStatIntervals.Week, DateTime.Today.AddHours(1));
			var res3 = aggr.Add(1, 11, WorktimeStatIntervals.Month, DateTime.Today.AddHours(2));

			//Assert
			Assert.Equal(WorktimeStatIntervals.Today, res1);
			Assert.Equal(WorktimeStatIntervals.Week, res2);
			Assert.Equal(WorktimeStatIntervals.Month, res3);
		}

		[Fact]
		public void AddComplexCase()
		{
			//Prepare
			var aggr = new WorktimeStatIntervalAggregator();

			//Act
			var res1 = aggr.Add(1, 11, WorktimeStatIntervals.Today | WorktimeStatIntervals.Month, DateTime.Today);
			var res2 = aggr.Add(1, 12, WorktimeStatIntervals.Today | WorktimeStatIntervals.Week, DateTime.Today.AddHours(2));
			var res3 = aggr.Add(1, 11, WorktimeStatIntervals.Today | WorktimeStatIntervals.Month | WorktimeStatIntervals.Week, DateTime.Today.AddHours(10));
			var res4 = aggr.Add(1, 11, WorktimeStatIntervals.Today | WorktimeStatIntervals.Week | WorktimeStatIntervals.Month, DateTime.Today.AddHours(27));
			var res5 = aggr.Add(1, 11, WorktimeStatIntervals.Today | WorktimeStatIntervals.Week | WorktimeStatIntervals.Month, DateTime.Today.AddHours(52));
			var res6 = aggr.Add(1, 12, WorktimeStatIntervals.Today, DateTime.Today.AddHours(77));
			var res7 = aggr.Add(1, 11, WorktimeStatIntervals.Week, DateTime.Today.AddHours(78));

			//Assert
			Assert.Equal(WorktimeStatIntervals.Today | WorktimeStatIntervals.Month, res1);
			Assert.Equal(WorktimeStatIntervals.Today | WorktimeStatIntervals.Month | WorktimeStatIntervals.Week, res2);
			Assert.Equal(WorktimeStatIntervals.Today | WorktimeStatIntervals.Month | WorktimeStatIntervals.Week, res3);
			Assert.Equal(WorktimeStatIntervals.Today | WorktimeStatIntervals.Month | WorktimeStatIntervals.Week, res4);
			Assert.Equal(WorktimeStatIntervals.Today | WorktimeStatIntervals.Month | WorktimeStatIntervals.Week, res5);
			Assert.Equal(WorktimeStatIntervals.Today, res6);
			Assert.Equal(WorktimeStatIntervals.Today | WorktimeStatIntervals.Week, res7);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.EmailStats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class HourlyWageTests
	{
		private static readonly DateTime now = new DateTime(2011, 02, 18, 12, 00, 00);
		private static readonly long HourTicks = TimeSpan.FromHours(1).Ticks;

		[Fact]
		public void ZeroWageForZeroInterval()
		{
			//Arrange
			var wage = new HourlyWage(10);
			//Act
			var cost = wage.GetCostFor(now, now);
			//Assert
			Assert.Equal(0m, cost);
		}

		[Fact]
		public void HourlyWageHasOneHourInterval()
		{
			Assert.Equal(TimeSpan.FromHours(1), new HourlyWage(0).Interval);
		}

		[Fact]
		public void HourlyWageForTwoHours()
		{
			//Arrange
			var hWage = 123.1231234m;
			var wage = new HourlyWage(hWage);
			//Act
			var cost = wage.GetCostFor(now, now.AddHours(2));
			//Assert
			Assert.Equal(2 * hWage, cost);
		}

		[Fact]
		public void HourlyWageForTwoHoursCanChangeDefault()
		{
			//Arrange
			var hWageBad = 143534534523.1231234m;
			var hWage = 123.1231234m;
			var wage = new HourlyWage(hWageBad);
			wage.SetChange(DateTime.MinValue, hWage);
			//Act
			var cost = wage.GetCostFor(now, now.AddHours(2));
			//Assert
			Assert.Equal(2 * hWage, cost);
		}

		[Fact]
		public void HourlyWageForThreeHoursAndTwoChanges()
		{
			//Arrange
			var hWage1 = 123.1231234m;
			var hWage2 = 153.13422231234m;
			var hWage3 = 23.38791234m;
			var wage = new HourlyWage(hWage1);
			wage.SetChange(now.AddHours(1), hWage2);
			wage.SetChange(now.AddHours(2), hWage3);
			//Act
			var cost = wage.GetCostFor(now, now.AddHours(3));
			//Assert
			Assert.Equal(hWage1 + hWage2 + hWage3, cost);
		}

		[Fact]
		public void HourlyWageForThreeHoursAndTwoChangesDifferentOrder()
		{
			//Arrange
			var hWage1 = 123.1231234m;
			var hWage2 = 153.13422231234m;
			var hWage3 = 23.38791234m;
			var wage = new HourlyWage(hWage1);
			wage.SetChange(now.AddHours(2), hWage3);
			wage.SetChange(now.AddHours(1), hWage2);
			//Act
			var cost = wage.GetCostFor(now, now.AddHours(3));
			//Assert
			Assert.Equal(hWage1 + hWage2 + hWage3, cost);
		}

		[Fact]
		public void HourlyWageForThreeHoursAndTwoChangesAllCorrectedDifferentOrder()
		{
			//Arrange
			var hWage1bad = 1233.1231234m;
			var hWage2bad = 123453.1231234m;
			var hWage3bad = 123453.1231234m;
			var hWage1 = 123.1231234m;
			var hWage2 = 153.13422231234m;
			var hWage3 = 23.38791234m;
			var wage = new HourlyWage(hWage1bad);
			wage.SetChange(now.AddHours(1), hWage2bad);
			wage.SetChange(DateTime.MinValue, hWage1); //default
			wage.SetChange(now.AddHours(2), hWage3bad);
			wage.SetChange(now.AddHours(2), hWage3);
			wage.SetChange(now.AddHours(1), hWage2);
			//Act
			var cost = wage.GetCostFor(now, now.AddHours(3));
			//Assert
			Assert.Equal(hWage1 + hWage2 + hWage3, cost);
		}

		[Fact]
		public void HourlyWageForRndIntervalTwoChanges()
		{
			//Arrange
			var hWage1 = 435123.1231345435435234m;
			var hWLen1 = 912312313214231;
			var hWage2 = 13453.1342223454331234m;
			var hWLen2 = 498292175224;
			var hWage3 = 24333.3879143543534234m;
			var hWLen3 = 28498734596;
			var wage = new HourlyWage(hWage1);
			wage.SetChange(now.AddTicks(hWLen1), hWage2);
			wage.SetChange(now.AddTicks(hWLen1 + hWLen2), hWage3);
			//Act
			var cost = wage.GetCostFor(now, now.AddTicks(hWLen1 + hWLen2 + hWLen3));
			//Assert
			Assert.Equal((decimal)hWLen1 / HourTicks * hWage1
				+ (decimal)hWLen2 / HourTicks * hWage2
				+ (decimal)hWLen3 / HourTicks * hWage3, cost);
		}

		[Fact]
		public void HourlyWageForRndIntervalTwoChangesFirstHalfOnly()
		{
			//Arrange
			var hWage1 = 435123.1231345435435234m;
			var hWLen1 = 912312313214231;
			var hWage2 = 13453.1342223454331234m;
			var hWLen2 = 498292175224;
			var hWage3 = 24333.3879143543534234m;
			var wage = new HourlyWage(hWage1);
			wage.SetChange(now.AddTicks(hWLen1), hWage2);
			wage.SetChange(now.AddTicks(hWLen1 + hWLen2), hWage3);
			//Act
			var cost = wage.GetCostFor(now, now.AddTicks(hWLen1-1));
			//Assert
			Assert.Equal((decimal)(hWLen1-1) / HourTicks * hWage1, cost);
		}
		[Fact]
		public void WageBeforeSetIsZero()
		{
			//Arrange
			var wage = new HourlyWage(0);
			wage.SetChange(DateTime.MinValue, 0);
			wage.SetChange(new DateTime(2011, 08, 01), 4824);
			//Act
			var cost = wage.GetCostFor(new DateTime(2011, 05, 23, 12, 00, 00), new DateTime(2011, 05, 23, 14, 00, 00));
			//Assert
			Assert.Equal(0m, cost);
		}
	}
}

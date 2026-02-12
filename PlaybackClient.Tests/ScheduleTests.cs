using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Scheduling
{
	public class ScheduleTests
	{
		#region Never test
		[Fact]
		public void NeverTest()
		{
			//Arrange
			var schedule = Schedule.Never;

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			Assert.Equal(0, result.Count);
		}
		#endregion

		#region OneTime tests
		[Fact]
		public void OneTimeHasOneDate()
		{
			//Arrange
			var startDate = new DateTime(2010, 11, 05, 12, 30, 05);
			var schedule = Schedule.CreateOneTime(startDate);

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			Assert.Equal(1, result.Count);
			Assert.Equal(startDate, result[0]);
		}
		#endregion

		#region Daily tests
		[Fact]
		public void DailyEveryDayNoEndDateIsNotInfinite()
		{
			DailyEveryDayNoEndDateIsNotInfiniteWithMaxDateSetImpl(null);
		}

		[Fact]
		public void DailyEveryDayNoEndDateIsNotInfiniteWithMaxDateSet()
		{
			DailyEveryDayNoEndDateIsNotInfiniteWithMaxDateSetImpl(new DateTime(6000, 11, 11));
		}

		private static void DailyEveryDayNoEndDateIsNotInfiniteWithMaxDateSetImpl(DateTime? maxDate)
		{
			//Arrange
			var startDate = new DateTime(2010, 11, 05, 12, 30, 05);
			var schedule = Schedule.CreateDaily(startDate, null, 1);
			if (maxDate.HasValue) schedule.MaxDate = maxDate.Value;

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			Assert.Equal((schedule.MaxDate - startDate).Days + 1, result.Count);
			Assert.Equal(startDate, result[0]);
			Assert.True(result[result.Count - 1] < schedule.MaxDate);
			Assert.True(result[result.Count - 1] > schedule.MaxDate.AddDays(-1));
		}

		[Fact]
		public void DailyEveryDayEndsNextDay()
		{
			//Arrange
			var startDate = new DateTime(2010, 11, 05, 12, 30, 05);
			var schedule = Schedule.CreateDaily(startDate, startDate.AddDays(1), 1); //todo enddate inclusive or exclusive

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			Assert.Equal(2, result.Count);
			Assert.Equal(startDate, result[0]);
			Assert.Equal(startDate.AddDays(1), result[1]);
		}

		[Fact]
		public void DailyEveryDayEndsSameDay()
		{
			//Arrange
			var startDate = new DateTime(2010, 11, 05, 12, 30, 05);
			var schedule = Schedule.CreateDaily(startDate, startDate, 1);

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			Assert.Equal(1, result.Count);
			Assert.Equal(startDate, result[0]);
		}

		[Fact]
		public void DailyEvery3rdDay()
		{
			//Arrange
			var startDate = new DateTime(2010, 11, 05, 12, 30, 05);
			var schedule = Schedule.CreateDaily(startDate, startDate.AddDays(298), 3);

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			Assert.Equal(100, result.Count);
			for (int i = 0; i < result.Count - 1; i++)
			{
				Assert.Equal(TimeSpan.FromDays(3), result[i + 1] - result[i]);
			}
			Assert.Equal(startDate, result[0]);
		}
		#endregion

		#region Even interval tests
		[Fact]
		public void EvenInterval()
		{
			//Arrange
			var startDate = new DateTime(2010, 11, 05, 12, 30, 05);
			var endDate = startDate.AddDays(100);
			var schedule = Schedule.CreateForEvenInterval(startDate, endDate, TimeSpan.FromMinutes(436));

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			Assert.Equal((endDate - startDate).Ticks / TimeSpan.FromMinutes(436).Ticks + 1, result.Count);
			for (int i = 0; i < result.Count - 1; i++)
			{
				Assert.Equal(TimeSpan.FromMinutes(436), result[i + 1] - result[i]);
			}
			Assert.Equal(startDate, result[0]);
		}
		#endregion

		#region Weekly tests
		[Fact]
		public void WeeklyEveryWeek()
		{
			//Arrange
			var startDate = new DateTime(2010, 11, 05, 12, 30, 05);
			var endDate = startDate.AddDays(100);
			var schedule = Schedule.CreateWeekly(startDate, endDate, 1);

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			Assert.Equal((endDate - startDate).Ticks / TimeSpan.FromDays(7).Ticks + 1, result.Count);
			for (int i = 0; i < result.Count - 1; i++)
			{
				Assert.Equal(TimeSpan.FromDays(7), result[i + 1] - result[i]);
			}
			Assert.Equal(startDate, result[0]);
		}

		[Fact]
		public void WeeklyEveryWeekOnMondayCreatedOnFriday()
		{
			//Arrange
			var startDate = new DateTime(2010, 11, 05, 12, 30, 05); //Friday
			var endDate = startDate.AddDays(10000);
			var schedule = Schedule.CreateWeekly(startDate, endDate, 1, DaysOfWeek.Monday, DayOfWeek.Monday);

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			int i = 0;
			for (var date = startDate; date <= endDate; date = date.AddDays(1))
			{
				if (date.DayOfWeek == DayOfWeek.Monday)
				{
					Assert.Equal(date, result[i]);
					i++;
				}
			}
		}

		[Fact]
		public void WeeklyEveryWeekOnMondayAndSaturdayCreatedOnFriday()
		{
			//Arrange
			var startDate = new DateTime(2010, 11, 05, 12, 30, 05); //Friday
			var endDate = startDate.AddDays(10000);
			var schedule = Schedule.CreateWeekly(startDate, endDate, 1, DaysOfWeek.Monday | DaysOfWeek.Saturday, DayOfWeek.Monday);

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			int i = 0;
			for (var date = startDate; date <= endDate; date = date.AddDays(1))
			{
				if (date.DayOfWeek == DayOfWeek.Monday || date.DayOfWeek == DayOfWeek.Saturday)
				{
					Assert.Equal(date, result[i]);
					i++;
				}
			}
		}


		[Fact]
		public void WeeklyEvery5thWeekOnSundayCreatedOnFridayFirstDayOfWeekIsMonday()
		{
			//Arrange
			var startDate = new DateTime(2010, 11, 05, 12, 30, 05); //Friday
			var endDate = startDate.AddDays(10000);
			var schedule = Schedule.CreateWeekly(startDate, endDate, 5, DaysOfWeek.Sunday, DayOfWeek.Monday);

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			int i = 0;
			var firstDate = startDate.AddDays(2);
			for (var date = firstDate; date <= endDate; date = date.AddDays(5 * 7))
			{
				Assert.Equal(date, result[i]);
				i++;
			}
		}

		[Fact]
		public void WeeklyEvery5thWeekOnSundayCreatedOnFridayFirstDayOfWeekIsSunday()
		{
			//Arrange
			var startDate = new DateTime(2010, 11, 05, 12, 30, 05); //Friday
			var endDate = startDate.AddDays(10000);
			var schedule = Schedule.CreateWeekly(startDate, endDate, 5, DaysOfWeek.Sunday, DayOfWeek.Sunday);

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			int i = 0;
			var firstDate = startDate.AddDays(2).AddDays(4 * 7);
			for (var date = firstDate; date <= endDate; date = date.AddDays(5 * 7))
			{
				Assert.Equal(date, result[i]);
				i++;
			}
		}
		#endregion

		#region Monthly tests
		[Fact]
		public void MonthlyEveryMonthFromJan31()
		{
			//Arrange
			var startDate = new DateTime(2010, 01, 31, 12, 30, 05);
			var endDate = new DateTime(2011, 01, 01);
			var schedule = Schedule.CreateMonthly(startDate, endDate, 1);

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			Assert.Equal(12, result.Count);
			Assert.Equal(startDate, result[0]);
			Assert.Equal(new DateTime(2010, 02, 28, 12, 30, 05), result[1]);
			Assert.Equal(new DateTime(2010, 03, 31, 12, 30, 05), result[2]);
			Assert.Equal(new DateTime(2010, 04, 30, 12, 30, 05), result[3]);
			Assert.Equal(new DateTime(2010, 05, 31, 12, 30, 05), result[4]);
			Assert.Equal(new DateTime(2010, 06, 30, 12, 30, 05), result[5]);
			Assert.Equal(new DateTime(2010, 07, 31, 12, 30, 05), result[6]);
			Assert.Equal(new DateTime(2010, 08, 31, 12, 30, 05), result[7]);
			Assert.Equal(new DateTime(2010, 09, 30, 12, 30, 05), result[8]);
			Assert.Equal(new DateTime(2010, 10, 31, 12, 30, 05), result[9]);
			Assert.Equal(new DateTime(2010, 11, 30, 12, 30, 05), result[10]);
			Assert.Equal(new DateTime(2010, 12, 31, 12, 30, 05), result[11]);
		}

		[Fact]
		public void MonthlyEveryMonthFromFeb27AtDay31()
		{
			//Arrange
			var startDate = new DateTime(2010, 02, 27, 12, 30, 05);
			var endDate = new DateTime(2010, 03, 01);
			var schedule = Schedule.CreateMonthly(startDate, endDate, 1, 31);

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			Assert.Equal(1, result.Count);
			Assert.Equal(startDate.AddDays(1), result[0]);
		}

		[Fact]
		public void MonthlyEveryMonthFromFeb28AtDay31()
		{
			//Arrange
			var startDate = new DateTime(2010, 02, 28, 12, 30, 05);
			var endDate = new DateTime(2010, 03, 01);
			var schedule = Schedule.CreateMonthly(startDate, endDate, 1, 31);

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			Assert.Equal(1, result.Count);
			Assert.Equal(startDate, result[0]);
		}

		[Fact]
		public void MonthlyEveryMonthFromFeb28AtDay15()
		{
			//Arrange
			var startDate = new DateTime(2010, 02, 28, 12, 30, 05);
			var endDate = new DateTime(2010, 04, 01);
			var schedule = Schedule.CreateMonthly(startDate, endDate, 1, 15);

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			Assert.Equal(1, result.Count);
			Assert.Equal(new DateTime(2010, 03, 15, 12, 30, 05), result[0]);
		}

		[Fact]
		public void MonthlyEveryMonthFromJan31AtDay30()
		{
			//Arrange
			var startDate = new DateTime(2010, 01, 31, 12, 30, 05);
			var endDate = new DateTime(2010, 04, 01);
			var schedule = Schedule.CreateMonthly(startDate, endDate, 1, 30);

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			Assert.Equal(2, result.Count);
			Assert.Equal(new DateTime(2010, 02, 28, 12, 30, 05), result[0]);
			Assert.Equal(new DateTime(2010, 03, 30, 12, 30, 05), result[1]);
		}

		[Fact]
		public void MonthlyEvery12ndMonthFromFeb28AtDay31()
		{
			//Arrange
			var startDate = new DateTime(2010, 02, 28, 12, 30, 05);
			var endDate = new DateTime(2011, 03, 01);
			var schedule = Schedule.CreateMonthly(startDate, endDate, 12, 31);

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			Assert.Equal(2, result.Count);
			Assert.Equal(startDate, result[0]);
			Assert.Equal(startDate.AddYears(1), result[1]);
		}

		[Fact]
		public void MonthlyEvery12ndMonthFromFeb28AtDay31LeapYear()
		{
			//Arrange
			var startDate = new DateTime(2010, 02, 28, 12, 30, 05);
			var endDate = new DateTime(2012, 03, 01);
			var schedule = Schedule.CreateMonthly(startDate, endDate, 12, 31);

			//Act
			var result = schedule.GetOccurances().ToList();

			//Assert
			Assert.Equal(3, result.Count);
			Assert.Equal(new DateTime(2010, 02, 28, 12, 30, 05), result[0]);
			Assert.Equal(new DateTime(2011, 02, 28, 12, 30, 05), result[1]);
			Assert.Equal(new DateTime(2012, 02, 29, 12, 30, 05), result[2]);
		}
		#endregion
	}

}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.EmailStats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class DateTimeWithZoneTests
	{
		private readonly TimeZoneInfo localTimeZone = TimeZoneInfo.FromSerializedString("Central Europe Standard Time;60;(GMT+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague;Central Europe Standard Time;Central Europe Daylight Time;[01:01:0001;12:31:9999;60;[0;02:00:00;3;5;0;];[0;03:00:00;10;5;0;];];");

		//not sure if we want this
		//[Fact]
		//public void CanCreateWithLocalTime()
		//{
		//    //Arrange
		//    var localNow = DateTime.Now;
		//    var dtz = new DateTimeWithZone(localNow, TimeZoneInfo.Local);

		//    //Assert
		//    Assert.Equal(localNow, dtz.LocalTime);
		//    Assert.Equal(localNow.ToUniversalTime(), dtz.UniversalTime);
		//}

		[Fact]
		public void CanCreateWithUtcTime()
		{
			//Arrange
			var utcNow = DateTime.UtcNow;
			var dtz = new DateTimeWithZone(utcNow, TimeZoneInfo.Local);

			//Assert
			Assert.Equal(utcNow.ToLocalTime(), dtz.LocalTime);
			Assert.Equal(utcNow, dtz.UniversalTime);
		}

		[Fact]
		public void CanAccessNow()
		{
			//Arrange
			var utcNow1 = DateTime.UtcNow;
			var dtz = DateTimeWithZone.Now;
			var utcNow2 = DateTime.UtcNow;

			//Assert
			Assert.True(utcNow1.ToLocalTime() <= dtz.LocalTime);
			Assert.True(dtz.LocalTime <= utcNow2.ToLocalTime());
			Assert.True(utcNow1 <= dtz.UniversalTime);
			Assert.True(dtz.UniversalTime <= utcNow2);
		}

		[Fact]
		public void SomeSimpleConversations()
		{
			Assert.Equal(new DateTime(2010, 10, 31, 03, 30, 00),
						 new DateTime(2010, 10, 31, 04, 30, 00).FromLocalToUtc(localTimeZone));

			Assert.Equal(new DateTime(2010, 04, 01, 01, 30, 00),
						 new DateTime(2010, 04, 01, 03, 30, 00).FromLocalToUtc(localTimeZone));

			Assert.Equal(new DateTime(2010, 10, 31, 03, 30, 00).FromUtcToLocal(localTimeZone),
						 new DateTime(2010, 10, 31, 04, 30, 00));

			Assert.Equal(new DateTime(2010, 04, 01, 01, 30, 00).FromUtcToLocal(localTimeZone),
						 new DateTime(2010, 04, 01, 03, 30, 00));
		}

		[Fact]
		public void FromLocalToUtcForInvalidTimeWontThrow()
		{
			var invalidTime = new DateTime(2010, 03, 28, 02, 30, 00);
			Assert.True(localTimeZone.IsInvalidTime(invalidTime));
			Assert.DoesNotThrow(() => invalidTime.FromLocalToUtc(localTimeZone));
		}

		[Fact]
		public void FromLocalToUtcForAmbiguousTimeWontThrow()
		{
			var ambiguousTime = new DateTime(2010, 10, 31, 02, 30, 00);
			Assert.True(localTimeZone.IsAmbiguousTime(ambiguousTime));
			Assert.DoesNotThrow(() => ambiguousTime.FromLocalToUtc(localTimeZone));
		}

		[Fact]
		public void AmbiguousTimeRoundTrips()
		{
			var ambiguousTime = new DateTime(2010, 10, 31, 02, 30, 00);
			Assert.True(localTimeZone.IsAmbiguousTime(ambiguousTime));
			Assert.Equal(ambiguousTime, ambiguousTime.FromLocalToUtc(localTimeZone).FromUtcToLocal(localTimeZone));
			Assert.Equal(ambiguousTime, ambiguousTime.FromUtcToLocal(localTimeZone).FromLocalToUtc(localTimeZone));
		}

		[Fact]
		public void FromLocalToUtcCanAvoidThrowWithUtcOffsetButSameResults()
		{
			long t1 = 0, t2 = 0;
			var sw = Stopwatch.StartNew();

			//jit
			TimeZoneInfo.ConvertTime(new DateTime(1000, 01, 01), localTimeZone, TimeZoneInfo.Utc);
			localTimeZone.GetUtcOffset(new DateTime(1000, 01, 01));

			for (DateTime date = new DateTime(1000, 01, 01, 00, 00, 00, DateTimeKind.Unspecified); date < new DateTime(1010, 01, 01); date = date.AddMinutes(30))
			{
				try
				{
					sw.Reset();
					sw.Start();
					var localDate = TimeZoneInfo.ConvertTime(date, localTimeZone, TimeZoneInfo.Utc);
					t1 += sw.ElapsedTicks;
					sw.Reset();
					sw.Start();
					var localDate2 = date - localTimeZone.GetUtcOffset(date); //10% faster and won't throw
					t2 += sw.ElapsedTicks;
					Assert.Equal(localDate, localDate2);
				}
				catch (ArgumentException)
				{
					//swallow
					var dthr = date;
					Assert.DoesNotThrow(() => localTimeZone.GetUtcOffset(dthr));
				}
			}
			Console.WriteLine(t1);
			Console.WriteLine(t2);
		}

		[Fact]
		public void FromUtcToLocalConvertWithUtcOffsetHasSameResult()
		{
			for (DateTime date = new DateTime(1000, 01, 01, 00, 00, 00, DateTimeKind.Utc); date < new DateTime(1010, 01, 01); date = date.AddMinutes(30))
			{
				var localDate = TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Utc, localTimeZone);
				var localDate2 = date + localTimeZone.GetUtcOffset(date);
				Assert.Equal(localDate, localDate2);
			}
		}

		[Fact]
		public void FromUtcToLocalConvertUtcUnspecifiedHasSameResult()
		{
			for (DateTime date = new DateTime(1000, 01, 01, 00, 00, 00, DateTimeKind.Utc); date < new DateTime(1010, 01, 01); date = date.AddMinutes(30))
			{
				var localDate = TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Utc, localTimeZone);
				var localDate2 = TimeZoneInfo.ConvertTime(new DateTime(date.Ticks, DateTimeKind.Unspecified), TimeZoneInfo.Utc, localTimeZone);
				Assert.Equal(localDate, localDate2);
			}
		}

		[Fact]
		public void FromUtcToLocalSameForEveryKind()
		{
			for (DateTime date = new DateTime(1000, 01, 01, 00, 00, 00, DateTimeKind.Unspecified); date < new DateTime(1010, 01, 01); date = date.AddMinutes(30))
			{
				var localDate = date.FromUtcToLocal(localTimeZone);
				var localDate2 = new DateTime(date.Ticks, DateTimeKind.Local).FromUtcToLocal(localTimeZone);
				var localDate3 = new DateTime(date.Ticks, DateTimeKind.Utc).FromUtcToLocal(localTimeZone);
				Assert.Equal(localDate, localDate2);
				Assert.Equal(localDate, localDate3);

				var localDate4 = TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Utc, localTimeZone);
				Assert.Equal(localDate, localDate4);
			}
		}

		[Fact]
		public void FromLocalToUtcSameForEveryKind()
		{
			for (DateTime date = new DateTime(1000, 01, 01, 00, 00, 00, DateTimeKind.Unspecified); date < new DateTime(1010, 01, 01); date = date.AddMinutes(30))
			{
				var utcDate = date.FromLocalToUtc(localTimeZone);
				var utcDate2 = new DateTime(date.Ticks, DateTimeKind.Local).FromLocalToUtc(localTimeZone);
				var utcDate3 = new DateTime(date.Ticks, DateTimeKind.Utc).FromLocalToUtc(localTimeZone);
				Assert.Equal(utcDate, utcDate2);
				Assert.Equal(utcDate, utcDate3);
				if (!localTimeZone.IsInvalidTime(date))
				{
					var utcDate4 = TimeZoneInfo.ConvertTime(date, localTimeZone, TimeZoneInfo.Utc);
					Assert.Equal(utcDate, utcDate4);
				}
			}
		}

		[Fact]
		public void LocalKindWontThrow()
		{
			var date = new DateTime(1000, 01, 01, 00, 00, 00, DateTimeKind.Local);
			Assert.DoesNotThrow(() => date.FromLocalToUtc(localTimeZone));
			Assert.DoesNotThrow(() => date.FromUtcToLocal(localTimeZone));
		}

		[Fact]
		public void UnspecifiedKindWontThrow()
		{
			var date = new DateTime(1000, 01, 01, 00, 00, 00, DateTimeKind.Unspecified);
			Assert.DoesNotThrow(() => date.FromLocalToUtc(localTimeZone));
			Assert.DoesNotThrow(() => date.FromUtcToLocal(localTimeZone));
		}

		[Fact]
		public void UtcKindWontThrow()
		{
			var date = new DateTime(1000, 01, 01, 00, 00, 00, DateTimeKind.Utc);
			Assert.DoesNotThrow(() => date.FromLocalToUtc(localTimeZone));
			Assert.DoesNotThrow(() => date.FromUtcToLocal(localTimeZone));
		}

		[Fact]
		public void EmailStatsHelperUtcStartEndFor20100328_300NormalTime()
		{
			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(
				ReportType.Daily,
				new DateTime(2010, 03, 28, 00, 00, 00),
				new UserStatInfo() { StartOfDayOffset = TimeSpan.FromHours(3), TimeZone = localTimeZone }
				);
			Assert.Equal(new DateTime(2010, 03, 28, 03, 00, 00).FromLocalToUtc(localTimeZone), startEnd.StartDate);
			Assert.Equal(new DateTime(2010, 03, 29, 03, 00, 00).FromLocalToUtc(localTimeZone), startEnd.EndDate);

			Assert.Equal(new DateTime(2010, 03, 28, 01, 00, 00), new DateTime(2010, 03, 28, 03, 00, 00).FromLocalToUtc(localTimeZone));
			Assert.Equal(new DateTime(2010, 03, 29, 01, 00, 00), new DateTime(2010, 03, 29, 03, 00, 00).FromLocalToUtc(localTimeZone));

			Assert.False(localTimeZone.IsAmbiguousTime(new DateTime(2010, 03, 28, 03, 00, 00)));
			Assert.False(localTimeZone.IsAmbiguousTime(new DateTime(2010, 03, 29, 03, 00, 00)));
			Assert.False(localTimeZone.IsInvalidTime(new DateTime(2010, 03, 28, 03, 00, 00)));
			Assert.False(localTimeZone.IsInvalidTime(new DateTime(2010, 03, 29, 03, 00, 00)));
		}

		[Fact]
		public void EmailStatsHelperUtcStartEndFor20100328_230InvalidTime()
		{
			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(
				ReportType.Daily,
				new DateTime(2010, 03, 28, 00, 00, 00),
				new UserStatInfo() { StartOfDayOffset = TimeSpan.FromHours(2.5), TimeZone = localTimeZone }
				);
			Assert.Equal(new DateTime(2010, 03, 28, 02, 30, 00).FromLocalToUtc(localTimeZone), startEnd.StartDate);
			Assert.Equal(new DateTime(2010, 03, 29, 02, 30, 00).FromLocalToUtc(localTimeZone), startEnd.EndDate);

			Assert.Equal(new DateTime(2010, 03, 28, 01, 30, 00), new DateTime(2010, 03, 28, 02, 30, 00).FromLocalToUtc(localTimeZone));
			Assert.Equal(new DateTime(2010, 03, 29, 00, 30, 00), new DateTime(2010, 03, 29, 02, 30, 00).FromLocalToUtc(localTimeZone));

			Assert.False(localTimeZone.IsAmbiguousTime(new DateTime(2010, 03, 28, 02, 30, 00)));
			Assert.False(localTimeZone.IsAmbiguousTime(new DateTime(2010, 03, 29, 02, 30, 00)));
			Assert.True(localTimeZone.IsInvalidTime(new DateTime(2010, 03, 28, 02, 30, 00)));
			Assert.False(localTimeZone.IsInvalidTime(new DateTime(2010, 03, 29, 02, 30, 00)));
		}

		[Fact]
		public void EmailStatsHelperUtcStartEndFor20100327_300NormalTime()
		{
			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(
				ReportType.Daily,
				new DateTime(2010, 03, 27, 00, 00, 00),
				new UserStatInfo() { StartOfDayOffset = TimeSpan.FromHours(3), TimeZone = localTimeZone }
				);
			Assert.Equal(new DateTime(2010, 03, 27, 03, 00, 00).FromLocalToUtc(localTimeZone), startEnd.StartDate);
			Assert.Equal(new DateTime(2010, 03, 28, 03, 00, 00).FromLocalToUtc(localTimeZone), startEnd.EndDate);

			Assert.Equal(new DateTime(2010, 03, 27, 02, 00, 00), new DateTime(2010, 03, 27, 03, 00, 00).FromLocalToUtc(localTimeZone));
			Assert.Equal(new DateTime(2010, 03, 28, 01, 00, 00), new DateTime(2010, 03, 28, 03, 00, 00).FromLocalToUtc(localTimeZone));

			Assert.False(localTimeZone.IsAmbiguousTime(new DateTime(2010, 03, 27, 03, 00, 00)));
			Assert.False(localTimeZone.IsAmbiguousTime(new DateTime(2010, 03, 28, 03, 00, 00)));
			Assert.False(localTimeZone.IsInvalidTime(new DateTime(2010, 03, 27, 03, 00, 00)));
			Assert.False(localTimeZone.IsInvalidTime(new DateTime(2010, 03, 28, 03, 00, 00)));
		}

		[Fact]
		public void EmailStatsHelperUtcStartEndFor20100329_300NormalTime()
		{
			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(
				ReportType.Daily,
				new DateTime(2010, 03, 29, 00, 00, 00),
				new UserStatInfo() { StartOfDayOffset = TimeSpan.FromHours(3), TimeZone = localTimeZone }
				);
			Assert.Equal(new DateTime(2010, 03, 29, 03, 00, 00).FromLocalToUtc(localTimeZone), startEnd.StartDate);
			Assert.Equal(new DateTime(2010, 03, 30, 03, 00, 00).FromLocalToUtc(localTimeZone), startEnd.EndDate);

			Assert.Equal(new DateTime(2010, 03, 29, 01, 00, 00), new DateTime(2010, 03, 29, 03, 00, 00).FromLocalToUtc(localTimeZone));
			Assert.Equal(new DateTime(2010, 03, 30, 01, 00, 00), new DateTime(2010, 03, 30, 03, 00, 00).FromLocalToUtc(localTimeZone));

			Assert.False(localTimeZone.IsAmbiguousTime(new DateTime(2010, 03, 29, 03, 00, 00)));
			Assert.False(localTimeZone.IsAmbiguousTime(new DateTime(2010, 03, 30, 03, 00, 00)));
			Assert.False(localTimeZone.IsInvalidTime(new DateTime(2010, 03, 29, 03, 00, 00)));
			Assert.False(localTimeZone.IsInvalidTime(new DateTime(2010, 03, 30, 03, 00, 00)));
		}

		[Fact]
		public void UtcLocalRoundtripsFineWhenNotInvalidOrAmbiguous()
		{
			var tzi = localTimeZone;
			for (DateTime i = new DateTime(1980, 01, 01); i < new DateTime(1982, 01, 01); i = i.AddMinutes(30))
			{
				if (i.FromUtcToLocal(tzi).FromLocalToUtc(tzi) != i)
				{
					Assert.True(tzi.IsAmbiguousTime(i.FromUtcToLocal(tzi)));
				}
				if (!tzi.IsInvalidTime(i))
				{
					Assert.True(i.FromLocalToUtc(tzi).FromUtcToLocal(tzi) == i);
				}
			}
		}

		[Fact]
		public void FromUtcToLocalResultsKindIsUnsepcified()
		{
			Assert.Equal(DateTimeKind.Unspecified, new DateTime(2011, 07, 06, 22, 15, 00).FromUtcToLocal(localTimeZone).Kind);
		}

		[Fact]
		public void FromLocalToUtcResultsKindIsUnsepcified()
		{
			Assert.Equal(DateTimeKind.Unspecified, new DateTime(2011, 07, 06, 22, 15, 00).FromLocalToUtc(localTimeZone).Kind);
		}

		[Fact]
		public void GetLocalInclusiveDaysForReportFromLocalDayMonthly()
		{
			var now = new DateTime(2012, 03, 02);
			var month = CalculatorHelper.GetLocalInclusiveDaysForReportFromLocalDay(ReportType.Monthly, now);
			Assert.Equal(now.AddDays(-1), month.StartDate);
			Assert.Equal(now.AddMonths(1).AddDays(-2), month.EndDate);
		}

		[Fact]
		public void GetLocalInclusiveDaysForReportFromLocalDayWeekly()
		{
			var now = new DateTime(2012, 03, 02);
			var week = CalculatorHelper.GetLocalInclusiveDaysForReportFromLocalDay(ReportType.Weekly, now);
			Assert.Equal(new DateTime(2012, 02, 27), week.StartDate);
			Assert.Equal(new DateTime(2012, 03, 04), week.EndDate);
		}

		[Fact]
		public void GetLocalInclusiveDaysForReportFromLocalDayDaily()
		{
			var now = new DateTime(2012, 03, 02);
			var day = CalculatorHelper.GetLocalInclusiveDaysForReportFromLocalDay(ReportType.Daily, now);
			Assert.Equal(now, day.StartDate);
			Assert.Equal(now, day.EndDate);
		}
	}
}

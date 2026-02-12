using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.View;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class FormatTests
	{
		private static readonly DateTime endDate = new DateTime(2014, 05, 31, 0, 0, 0);

		private static string GetRemainingTime(DateTime now)
		{
			return FormatHelper.GetRemainingTime(endDate - now);
		}

		[Fact]
		public void GetRemainingTimeTests()
		{
			Assert.Equal(string.Format(Labels.Left, "3 " + Labels.DayPlural), GetRemainingTime(new DateTime(2014, 05, 28, 8, 0, 0)));
			Assert.Equal(string.Format(Labels.Left, "3 " + Labels.DayPlural), GetRemainingTime(new DateTime(2014, 05, 28, 15, 0, 0)));
			Assert.Equal(string.Format(Labels.Left, "2 " + Labels.DayPlural), GetRemainingTime(new DateTime(2014, 05, 29, 8, 0, 0)));
			Assert.Equal(string.Format(Labels.Left, "2 " + Labels.DayPlural), GetRemainingTime(new DateTime(2014, 05, 29, 23, 59, 59)));
			Assert.Equal(string.Format(Labels.Left, "23 " + Labels.HourPlural), GetRemainingTime(new DateTime(2014, 05, 30, 0, 0, 01)));
			Assert.Equal(string.Format(Labels.Left, "11 " + Labels.HourPlural), GetRemainingTime(new DateTime(2014, 05, 30, 12, 0, 01)));
			Assert.Equal(string.Format(Labels.Left, "1 " + Labels.HourSingular), GetRemainingTime(new DateTime(2014, 05, 30, 22, 0, 01)));
			Assert.Equal(string.Format(Labels.Left, "0 " + Labels.HourPlural), GetRemainingTime(new DateTime(2014, 05, 30, 23, 0, 01)));
			Assert.Equal(string.Format(Labels.OverdueSingular, "1"), GetRemainingTime(new DateTime(2014, 05, 31, 0, 0, 01)));
			Assert.Equal(string.Format(Labels.OverdueSingular, "1"), GetRemainingTime(new DateTime(2014, 05, 31, 12, 0, 01)));
			Assert.Equal(string.Format(Labels.OverduePlural, "2"), GetRemainingTime(new DateTime(2014, 06, 01, 0, 0, 01)));
		}

		[Fact]
		public void ToHourMinuteStringPerformance()
		{
			var time = TimeSpan.FromMinutes(234);
			const int it = 10000;
			Assert.Equal("0:02", TimeSpan.FromMinutes(2).ToHourMinuteString());
			var sw = System.Diagnostics.Stopwatch.StartNew();
			for (int i = 0; i < it; i++)
			{
				time.ToHourMinuteString();
			}
			Console.WriteLine(sw.Elapsed.TotalMilliseconds);
		}

		[Fact]
		public void ToHourMinuteSecondStringPerformance()
		{
			var time = TimeSpan.FromMinutes(234);
			const int it = 10000;
			Assert.Equal("00:02:00", TimeSpan.FromMinutes(2).ToHourMinuteSecondString());
			var sw = System.Diagnostics.Stopwatch.StartNew();
			for (int i = 0; i < it; i++)
			{
				time.ToHourMinuteSecondString();
			}
			Console.WriteLine(sw.Elapsed.TotalMilliseconds);
		}
	}
}

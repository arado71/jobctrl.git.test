using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.EmailStats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class DateRangeFormatterTests
	{
		private static readonly DateTime now = new DateTime(2011, 10, 24, 15, 20, 00);
		private readonly DateRangeFormatter fmt = DateRangeFormatter.EnglishLong;

		[Fact]
		public void SimpleTests()
		{
			var tests = new[] {
				new { Start = now, End = now, Expected = "0 milliseconds"},
				new { Start = now, End = now.AddMilliseconds(1), Expected = "1 millisecond"},
				new { Start = now, End = now.AddMilliseconds(232), Expected = "232 milliseconds"},
				new { Start = now, End = now.AddSeconds(1), Expected = "1 second"},
				new { Start = now, End = now.AddSeconds(23), Expected = "23 seconds"},
				new { Start = now, End = now.AddMinutes(4), Expected = "4 minutes"},
				new { Start = now, End = now.AddHours(13), Expected = "13 hours"},
				new { Start = now, End = now.AddDays(3), Expected = "3 days"},
				new { Start = now, End = now.AddDays(14), Expected = "2 weeks"},
				new { Start = now, End = now.AddMonths(3), Expected = "3 months"},
				new { Start = now, End = now.AddMonths(12), Expected = "1 year"},
				new { Start = now, End = now.AddMonths(13), Expected = "1 year, 1 month"},
				new { Start = now, End = now.AddMonths(14), Expected = "1 year, 2 months"},
				new { Start = now, End = now.AddMonths(24), Expected = "2 years"},
				new { Start = now, End = now.AddMonths(25), Expected = "2 years, 1 month"},
				new { Start = now, End = now.AddMonths(26), Expected = "2 years, 2 months"},
			};

			foreach (var test in tests)
			{
				Assert.Equal(test.Expected, fmt.GetApproxRangeString(test.Start, test.End));
			}
		}

		[Fact]
		public void RoundTests()
		{
			var tests = new[] {
				new { Start = now, End = now.AddTicks(4999), Expected = "0 milliseconds"},
				new { Start = now, End = now.AddMilliseconds(1).AddTicks(4999), Expected = "1 millisecond"},
				new { Start = now, End = now.AddTicks(5000), Expected = "1 millisecond"},
				new { Start = now, End = now.AddMilliseconds(749).AddTicks(4999), Expected = "749 milliseconds"},
				new { Start = now, End = now.AddMilliseconds(749).AddTicks(5000), Expected = "1 second"},
				new { Start = now, End = now.AddSeconds(1).AddSeconds(0.5), Expected = "2 seconds"},
				new { Start = now, End = now.AddSeconds(89).AddSeconds(0.5).AddMilliseconds(-1), Expected = "89 seconds"},
				new { Start = now, End = now.AddSeconds(89).AddSeconds(0.5), Expected = "90 seconds"},
				new { Start = now, End = now.AddSeconds(90), Expected = "2 minutes"},
				new { Start = now, End = now.AddMinutes(89).AddMinutes(0.5).AddMilliseconds(-1), Expected = "89 minutes"},
				new { Start = now, End = now.AddMinutes(89).AddMinutes(0.5), Expected = "90 minutes"},
				
				new { Start = now, End = now.AddMinutes(90), Expected = "2 hours"},
				new { Start = now, End = now.AddHours(13), Expected = "13 hours"},
				new { Start = now, End = now.AddDays(3), Expected = "3 days"},
				new { Start = now, End = now.AddDays(14), Expected = "2 weeks"},
				new { Start = now, End = now.AddMonths(3), Expected = "3 months"},

				new { Start = now, End = now.AddMonths(12), Expected = "1 year"},
				new { Start = now, End = now.AddMonths(12).AddDays(14), Expected = "1 year"},
				new { Start = now, End = now.AddMonths(12).AddDays(-14), Expected = "1 year"},
				new { Start = now, End = now.AddMonths(13), Expected = "1 year, 1 month"},
				new { Start = now, End = now.AddMonths(13).AddDays(13), Expected = "1 year, 1 month"},
				new { Start = now, End = now.AddMonths(13).AddDays(-14), Expected = "1 year, 1 month"},
				new { Start = now, End = now.AddMonths(14), Expected = "1 year, 2 months"},
				new { Start = now, End = now.AddMonths(14).AddDays(14), Expected = "1 year, 2 months"},
				new { Start = now, End = now.AddMonths(14).AddDays(-14), Expected = "1 year, 2 months"},
				new { Start = now, End = now.AddMonths(24), Expected = "2 years"},
				new { Start = now, End = now.AddMonths(24).AddDays(14), Expected = "2 years"},
				new { Start = now, End = now.AddMonths(24).AddDays(-14), Expected = "2 years"},
				new { Start = now, End = now.AddMonths(25), Expected = "2 years, 1 month"},
				new { Start = now, End = now.AddMonths(25).AddDays(14), Expected = "2 years, 1 month"},
				new { Start = now, End = now.AddMonths(25).AddDays(-14), Expected = "2 years, 1 month"},
				new { Start = now, End = now.AddMonths(26), Expected = "2 years, 2 months"},
				new { Start = now, End = now.AddMonths(26).AddDays(14), Expected = "2 years, 2 months"},
				new { Start = now, End = now.AddMonths(26).AddDays(-14), Expected = "2 years, 2 months"},
			};

			foreach (var test in tests)
			{
				Assert.Equal(test.Expected, fmt.GetApproxRangeString(test.Start, test.End));
			}
		}

		[Fact]
		public void StrangeTests()
		{
			var tests = new[] {
				new { Start = now, End = now.AddMonths(13).AddDays(13), Expected = "1 year, 1 month"},  //2012-12-07
				new { Start = now, End = now.AddMonths(13).AddDays(14), Expected = "1 year, 2 months"}, //2012-12-08
			};

			foreach (var test in tests)
			{
				Assert.Equal(test.Expected, fmt.GetApproxRangeString(test.Start, test.End));
			}
		}
	}
}

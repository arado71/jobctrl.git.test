using System;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class DateRoundTripWithoutDbTests
	{
		private readonly DateTime now = new DateTime(2011, 05, 18, 12, 00, 00);

		[Fact]
		public void DateTimeRoundTripTest()
		{
			for (DateTime i = now.AddSeconds(-1); i < now.AddSeconds(1); i = i.AddMilliseconds(1))
			{
				int mod10 = i.Millisecond % 10;
				DateTime dbTime;
				if (mod10 == 9 || mod10 == 2 || mod10 == 6)
				{
					dbTime = i.AddMilliseconds(1);
				}
				else if (mod10 == 0 || mod10 == 3 || mod10 == 7)
				{
					dbTime = i;
				}
				else if (mod10 == 1 || mod10 == 8 || mod10 == 4)
				{
					dbTime = i.AddMilliseconds(-1);
				}
				else if (mod10 == 5)
				{
					dbTime = i.AddMilliseconds(2);
				}
				else
				{
					throw null;
				}
				Assert.Equal(dbTime, i.ToSqlRoundTripDateTime());
			}
		}

		[Fact]
		public void DateTimeRoundTripTestTicksZeroed()
		{
			Assert.Equal(now, now.AddTicks(-1).ToSqlRoundTripDateTime());

			for (DateTime i = now.AddTicks(1); i < now.AddSeconds(1); i = i.AddMilliseconds(1))
			{
				int mod10 = i.Millisecond % 10;
				DateTime dbTime;
				if (mod10 == 9 || mod10 == 2 || mod10 == 6)
				{
					dbTime = i.AddMilliseconds(1);
				}
				else if (mod10 == 0 || mod10 == 3 || mod10 == 7)
				{
					dbTime = i;
				}
				else if (mod10 == 1 || mod10 == 8 || mod10 == 4)
				{
					dbTime = i.AddMilliseconds(-1);
				}
				else if (mod10 == 5)
				{
					dbTime = i.AddMilliseconds(2);
				}
				else
				{
					throw null;
				}
				Assert.Equal(dbTime.AddTicks(-1), i.ToSqlRoundTripDateTime());
			}
		}

		[Fact]
		public void DateTimeRoundTripMsEndsWith0Or3Or7()
		{
			//for (DateTime i = now; i < now.AddMinutes(1); i = i.AddTicks(1)) //3mins
			for (DateTime i = now; i < now.AddMilliseconds(10); i = i.AddTicks(1))
			{
				var rt = i.ToSqlRoundTripDateTime();
				var ms100 = rt.Ticks % 100000;
				Assert.True(ms100 == 0 || ms100 == 30000 || ms100 == 70000);
			}
		}

		[Fact]
		public void DateTimeRoundTripsTwoTimesAreTheSame()
		{
			//for (DateTime i = now; i < now.AddMinutes(1); i = i.AddTicks(1)) //12mins
			for (DateTime i = now; i < now.AddMilliseconds(10); i = i.AddTicks(1))
			{
				var first = i.ToSqlRoundTripDateTime();
				var second = first.ToSqlRoundTripDateTime();
				Assert.Equal(first, second);
			}
		}

		[Fact]
		public void TimeDiffTest()
		{
			var first = DateTime.Parse("2010-04-01 12:00:00.000");
			var second = DateTime.Parse("2010-04-01 12:00:00.007");

			var timeTicks = first.TimeOfDay.Ticks;
			var sqlTimeTicks1 = (int)((timeTicks / 10000d * 0.3d) + 0.5d);
			timeTicks = second.TimeOfDay.Ticks;
			var sqlTimeTicks2 = (int)((timeTicks / 10000d * 0.3d) + 0.5d);

			var diff = sqlTimeTicks2 - sqlTimeTicks1;
			var realDiff = (long)((sqlTimeTicks2 / 0.3d) + 0.5d) * 10000L - (long)((sqlTimeTicks1 / 0.3d) + 0.5d) * 10000L;

			var timeTicksBack = (long)((diff / 0.3d) + 0.5d) * 10000L;
			var timeTicksBackW = (long)((diff / 0.3d) ) * 10000L;

			Console.WriteLine(sqlTimeTicks1 + " " + sqlTimeTicks2 + " " + timeTicksBack + " " + timeTicksBackW + " " + realDiff);
		}
	}
}
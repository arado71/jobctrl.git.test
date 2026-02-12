using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class CalendarCacheTests : CalendarTestsBase
	{
		private static readonly DateTime now = new DateTime(2011, 01, 17);

		public int[] CreateComplexHierarchy()
		{
			var cal1 = AddCalendar(new Calendar()
			{
				CalendarExceptions = new System.Data.Linq.EntitySet<CalendarException>()
					{
						new CalendarException() { Date = now, IsWorkDay = true },
						new CalendarException() { Date = now.AddDays(1), IsWorkDay = true, TargetWorkTimeInMinutes = 600},
						new CalendarException() { Date = now.AddDays(2), IsWorkDay = true },
						new CalendarException() { Date = now.AddDays(3), IsWorkDay = true, TargetWorkTimeInMinutes = 480},
					}
			});
			var cal2 = AddCalendar(new Calendar()
			{
				InheritedFrom = cal1.Id,
				CalendarExceptions = new System.Data.Linq.EntitySet<CalendarException>()
					{
						new CalendarException() { Date = now.AddDays(1), IsWorkDay = false },
						new CalendarException() { Date = now.AddDays(2), IsWorkDay = false },
						new CalendarException() { Date = now.AddDays(4), IsWorkDay = true, TargetWorkTimeInMinutes = 490},
						new CalendarException() { Date = now.AddDays(5), IsWorkDay = true },
					}
			});
			var cal3 = AddCalendar(new Calendar()
			{
				TargetWorkTimeInMinutesThursday = 420,
				TargetWorkTimeInMinutesFriday = 400,
				InheritedFrom = cal2.Id,
				CalendarExceptions = new System.Data.Linq.EntitySet<CalendarException>()
					{
						new CalendarException() { Date = now.AddDays(1), IsWorkDay = true, TargetWorkTimeInMinutes = 540},
						new CalendarException() { Date = now.AddDays(4), IsWorkDay = false },
						new CalendarException() { Date = now.AddDays(6), IsWorkDay = true, TargetWorkTimeInMinutes = 600},
					}
			});
			var cal4 = AddCalendar(new Calendar()
			{
				TargetWorkTimeInMinutesMonday = 415,
				TargetWorkTimeInMinutesTuesday = 410,
				TargetWorkTimeInMinutesWednesday = 405,
				InheritedFrom = cal3.Id,
				CalendarExceptions = new System.Data.Linq.EntitySet<CalendarException>()
					{
						new CalendarException() { Date = now.AddDays(1), IsWorkDay = false },
						new CalendarException() { Date = now.AddDays(2), IsWorkDay = true },
						new CalendarException() { Date = now.AddDays(8), IsWorkDay = true },
					}
			});
			var cal5 = AddCalendar(new Calendar()
			{
				CalendarExceptions = new System.Data.Linq.EntitySet<CalendarException>()
				{
					new CalendarException() { Date = now, IsWorkDay = true },
					new CalendarException() { Date = now.AddDays(1), IsWorkDay = true },
					new CalendarException() { Date = now.AddDays(2), IsWorkDay = true },
					new CalendarException() { Date = now.AddDays(3), IsWorkDay = true },
				}
			});

			//todo shuffle excpetions in db ?

			return new[] { cal1.Id, cal2.Id, cal3.Id, cal4.Id, cal5.Id };
		}

		private static void AssertWorkingDays(int calId, params bool[] isWorkDay)
		{
			for (int i = 0; i < isWorkDay.Length; i++)
			{
				Assert.True(isWorkDay[i] == CalendarHelper.IsWorkDay(calId, now.AddDays(i)), "Wrong value at idx: " + i);
			}
			//make sure isWorkDay is datefirst agnostic
			using (var context = new IvrDataClassesDataContext())
			{
				for (int dtFirst = 1; dtFirst < 8; dtFirst++)
				{
					context.ExecuteQuery<object>("SET DATEFIRST " + dtFirst + ";");
					for (int i = 0; i < isWorkDay.Length; i++)
					{
						Assert.True(isWorkDay[i] == context.IsWorkDay(calId, now.AddDays(i)), "Wrong value (DATEFIRST) at idx: " + i);
					}
				}
			}
		}

		[Fact]
		public void WorkDaysCountFourDepthExceptions()
		{
			var calIds = CreateComplexHierarchy();
			var inheritedId = calIds[3];
			var currCal = AddCalendar(new Calendar() { InheritedFrom = inheritedId, });
			AssertWorkingDays(currCal.Id, true, false, true, true, false, true, true, false, true);
			var calendarLookup = new CalendarManager().GetCalenderLookup(currCal.Id);
			var workDays = calendarLookup.GetWorkDays(now, now.AddDays(8));
			Assert.True(workDays.SequenceEqual(new[] { 0, 2, 3, 5, 6, 8 }.Select(n => now.AddDays(n))));
			Assert.Equal(6, calendarLookup.CountWorkDays(now, now.AddDays(8)));
			Assert.Equal(5, calendarLookup.CountWorkDays(now, now.AddDays(7)));
			Assert.Equal(5, calendarLookup.CountWorkDays(now, now.AddDays(6)));
			Assert.Equal(4, calendarLookup.CountWorkDays(now, now.AddDays(5)));
			Assert.Equal(3, calendarLookup.CountWorkDays(now, now.AddDays(4)));
			Assert.Equal(3, calendarLookup.CountWorkDays(now, now.AddDays(3)));
			Assert.Equal(2, calendarLookup.CountWorkDays(now, now.AddDays(2)));
			Assert.Equal(1, calendarLookup.CountWorkDays(now, now.AddDays(1)));
			Assert.Equal(1, calendarLookup.CountWorkDays(now, now.AddDays(0)));
			currCal = AddCalendar(new Calendar()
			{
				InheritedFrom = inheritedId,
				IsMondayWorkDay = true,
				IsTuesdayWorkDay = true,
				IsWednesdayWorkDay = true,
				IsThursdayWorkDay = true,
				IsFridayWorkDay = true,
				IsSaturdayWorkDay = true,
				IsSundayWorkDay = true,
			});
			AssertWorkingDays(currCal.Id, true, false, true, true, false, true, true, true, true);
		}

		[Fact]
		public void WorkTimeCountForDepth1Exceptions()
		{
			var calIds = CreateComplexHierarchy();
			var inheritedId = calIds[0];
			var currCal = AddCalendar(new Calendar() { InheritedFrom = inheritedId, });
			var calendarLookup = new CalendarManager().GetCalenderLookup(currCal.Id);
			var defaultWorkTime = TimeSpan.FromHours(2);

			var workTime1 = calendarLookup.GetTargetWorkTime(now, now, defaultWorkTime);
			var workTime2 = calendarLookup.GetTargetWorkTime(now, now.AddDays(1), defaultWorkTime);
			var workTime3 = calendarLookup.GetTargetWorkTime(now, now.AddDays(2), defaultWorkTime);
			var workTime4 = calendarLookup.GetTargetWorkTime(now, now.AddDays(3), defaultWorkTime);
			var workTime5 = calendarLookup.GetTargetWorkTime(now, now.AddDays(4), defaultWorkTime);
			var workTime6 = calendarLookup.GetTargetWorkTime(now, now.AddDays(5), defaultWorkTime);
			var workTime7 = calendarLookup.GetTargetWorkTime(now, now.AddDays(6), defaultWorkTime);
			var workTime8 = calendarLookup.GetTargetWorkTime(now, now.AddDays(7), defaultWorkTime);
			var workTime9 = calendarLookup.GetTargetWorkTime(now, now.AddDays(8), defaultWorkTime);

			var expectedWorkTime = defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime1);
			expectedWorkTime += TimeSpan.FromMinutes(600);
			Assert.Equal(expectedWorkTime, workTime2);
			expectedWorkTime += defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime3);
			expectedWorkTime += TimeSpan.FromMinutes(480);
			Assert.Equal(expectedWorkTime, workTime4);
			Assert.Equal(expectedWorkTime, workTime5);
			Assert.Equal(expectedWorkTime, workTime6);
			Assert.Equal(expectedWorkTime, workTime7);
			Assert.Equal(expectedWorkTime, workTime8);
			Assert.Equal(expectedWorkTime, workTime9);
		}

		[Fact]
		public void WorkTimeCountForDepth2Exceptions()
		{
			var calIds = CreateComplexHierarchy();
			var inheritedId = calIds[1];
			var currCal = AddCalendar(new Calendar() { InheritedFrom = inheritedId, });
			var calendarLookup = new CalendarManager().GetCalenderLookup(currCal.Id);
			var defaultWorkTime = TimeSpan.FromHours(2);

			var workTime1 = calendarLookup.GetTargetWorkTime(now, now, defaultWorkTime);
			var workTime2 = calendarLookup.GetTargetWorkTime(now, now.AddDays(1), defaultWorkTime);
			var workTime3 = calendarLookup.GetTargetWorkTime(now, now.AddDays(2), defaultWorkTime);
			var workTime4 = calendarLookup.GetTargetWorkTime(now, now.AddDays(3), defaultWorkTime);
			var workTime5 = calendarLookup.GetTargetWorkTime(now, now.AddDays(4), defaultWorkTime);
			var workTime6 = calendarLookup.GetTargetWorkTime(now, now.AddDays(5), defaultWorkTime);
			var workTime7 = calendarLookup.GetTargetWorkTime(now, now.AddDays(6), defaultWorkTime);
			var workTime8 = calendarLookup.GetTargetWorkTime(now, now.AddDays(7), defaultWorkTime);
			var workTime9 = calendarLookup.GetTargetWorkTime(now, now.AddDays(8), defaultWorkTime);

			var expectedWorkTime = defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime1);
			Assert.Equal(expectedWorkTime, workTime2);
			Assert.Equal(expectedWorkTime, workTime3);
			expectedWorkTime += TimeSpan.FromMinutes(480);
			Assert.Equal(expectedWorkTime, workTime4);
			expectedWorkTime += TimeSpan.FromMinutes(490);
			Assert.Equal(expectedWorkTime, workTime5);
			expectedWorkTime += defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime6);
			Assert.Equal(expectedWorkTime, workTime7);
			Assert.Equal(expectedWorkTime, workTime8);
			Assert.Equal(expectedWorkTime, workTime9);
		}

		[Fact]
		public void WorkTimeCountForDepth3Exceptions()
		{
			var calIds = CreateComplexHierarchy();
			var inheritedId = calIds[2];
			var currCal = AddCalendar(new Calendar() { InheritedFrom = inheritedId, });
			var calendarLookup = new CalendarManager().GetCalenderLookup(currCal.Id);
			var defaultWorkTime = TimeSpan.FromHours(2);

			var workTime1 = calendarLookup.GetTargetWorkTime(now, now, defaultWorkTime);
			var workTime2 = calendarLookup.GetTargetWorkTime(now, now.AddDays(1), defaultWorkTime);
			var workTime3 = calendarLookup.GetTargetWorkTime(now, now.AddDays(2), defaultWorkTime);
			var workTime4 = calendarLookup.GetTargetWorkTime(now, now.AddDays(3), defaultWorkTime);
			var workTime5 = calendarLookup.GetTargetWorkTime(now, now.AddDays(4), defaultWorkTime);
			var workTime6 = calendarLookup.GetTargetWorkTime(now, now.AddDays(5), defaultWorkTime);
			var workTime7 = calendarLookup.GetTargetWorkTime(now, now.AddDays(6), defaultWorkTime);
			var workTime8 = calendarLookup.GetTargetWorkTime(now, now.AddDays(7), defaultWorkTime);
			var workTime9 = calendarLookup.GetTargetWorkTime(now, now.AddDays(8), defaultWorkTime);

			var expectedWorkTime = defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime1);
			expectedWorkTime += TimeSpan.FromMinutes(540);
			Assert.Equal(expectedWorkTime, workTime2);
			Assert.Equal(expectedWorkTime, workTime3);
			expectedWorkTime += TimeSpan.FromMinutes(480);
			Assert.Equal(expectedWorkTime, workTime4);
			Assert.Equal(expectedWorkTime, workTime5);
			expectedWorkTime += defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime6);
			expectedWorkTime += TimeSpan.FromMinutes(600);
			Assert.Equal(expectedWorkTime, workTime7);
			Assert.Equal(expectedWorkTime, workTime8);
			Assert.Equal(expectedWorkTime, workTime9);
		}

		[Fact]
		public void WorkTimeCountForDepth4Exceptions()
		{
			var calIds = CreateComplexHierarchy();
			var inheritedId = calIds[3];
			var currCal = AddCalendar(new Calendar() { InheritedFrom = inheritedId, });
			var calendarLookup = new CalendarManager().GetCalenderLookup(currCal.Id);
			var defaultWorkTime = TimeSpan.FromHours(2);

			var workTime1 = calendarLookup.GetTargetWorkTime(now, now, defaultWorkTime);
			var workTime2 = calendarLookup.GetTargetWorkTime(now, now.AddDays(1), defaultWorkTime);
			var workTime3 = calendarLookup.GetTargetWorkTime(now, now.AddDays(2), defaultWorkTime);
			var workTime4 = calendarLookup.GetTargetWorkTime(now, now.AddDays(3), defaultWorkTime);
			var workTime5 = calendarLookup.GetTargetWorkTime(now, now.AddDays(4), defaultWorkTime);
			var workTime6 = calendarLookup.GetTargetWorkTime(now, now.AddDays(5), defaultWorkTime);
			var workTime7 = calendarLookup.GetTargetWorkTime(now, now.AddDays(6), defaultWorkTime);
			var workTime8 = calendarLookup.GetTargetWorkTime(now, now.AddDays(7), defaultWorkTime);
			var workTime9 = calendarLookup.GetTargetWorkTime(now, now.AddDays(8), defaultWorkTime);

			var expectedWorkTime = TimeSpan.FromMinutes(415);
			Assert.Equal(expectedWorkTime, workTime1);
			Assert.Equal(expectedWorkTime, workTime2);
			expectedWorkTime += TimeSpan.FromMinutes(405);
			Assert.Equal(expectedWorkTime, workTime3);
			expectedWorkTime += TimeSpan.FromMinutes(480);
			Assert.Equal(expectedWorkTime, workTime4);
			Assert.Equal(expectedWorkTime, workTime5);
			expectedWorkTime += defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime6);
			expectedWorkTime += TimeSpan.FromMinutes(600);
			Assert.Equal(expectedWorkTime, workTime7);
			Assert.Equal(expectedWorkTime, workTime8);
			expectedWorkTime += TimeSpan.FromMinutes(410);
			Assert.Equal(expectedWorkTime, workTime9);
		}

		[Fact]
		public void WorkDaysCountNoExceptions()
		{
			var currCal = AddCalendar(new Calendar()
			{
				IsMondayWorkDay = true,
				IsTuesdayWorkDay = true,
				IsWednesdayWorkDay = true,
				IsThursdayWorkDay = true,
				IsFridayWorkDay = true,
				IsSaturdayWorkDay = false,
				IsSundayWorkDay = false
			});
			AssertWorkingDays(currCal.Id, true, true, true, true, true, false, false, true, true);
			var calendarLookup = new CalendarManager().GetCalenderLookup(currCal.Id);
			var workDays = calendarLookup.GetWorkDays(now, now.AddDays(8));
			Assert.True(workDays.SequenceEqual(new[] { 0, 1, 2, 3, 4, 7, 8 }.Select(n => now.AddDays(n))));
			Assert.Equal(7, CalendarHelper.CountDaysBetween(workDays, now, now.AddDays(8)));
		}

		[Fact]
		public void TargetWorkTimeIntervalBeforeFirst()
		{
			var calIds = CreateComplexHierarchy();
			var inheritedId = calIds[4];
			var currCal = AddCalendar(new Calendar() { InheritedFrom = inheritedId, });
			var calendarLookup = new CalendarManager().GetCalenderLookup(currCal.Id);
			var defaultWorkTime = TimeSpan.FromHours(2);
			var targetWorkTimeIntervals = new List<TargetWorkTimeInterval>
			{
				new TargetWorkTimeInterval { StartDate = now.AddDays(5), TargetWorkTime = TimeSpan.FromMinutes(100)},
				new TargetWorkTimeInterval { StartDate = now.AddDays(7), TargetWorkTime = TimeSpan.FromMinutes(110)},
			};

			var workTime1 = calendarLookup.GetTargetWorkTime(now, now, defaultWorkTime, targetWorkTimeIntervals);
			var workTime2 = calendarLookup.GetTargetWorkTime(now, now.AddDays(1), defaultWorkTime, targetWorkTimeIntervals);
			var workTime3 = calendarLookup.GetTargetWorkTime(now, now.AddDays(2), defaultWorkTime, targetWorkTimeIntervals);
			var workTime4 = calendarLookup.GetTargetWorkTime(now, now.AddDays(3), defaultWorkTime, targetWorkTimeIntervals);

			var expectedWorkTime = defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime1);
			expectedWorkTime += defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime2);
			expectedWorkTime += defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime3);
			expectedWorkTime += defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime4);
		}

		[Fact]
		public void TargetWorkTimeIntervalOverlapped()
		{
			var calIds = CreateComplexHierarchy();
			var inheritedId = calIds[4];
			var currCal = AddCalendar(new Calendar() { InheritedFrom = inheritedId, });
			var calendarLookup = new CalendarManager().GetCalenderLookup(currCal.Id);
			var defaultWorkTime = TimeSpan.FromHours(2);
			var targetWorkTimeIntervals = new List<TargetWorkTimeInterval>
			{
				new TargetWorkTimeInterval { StartDate = now.AddDays(2), TargetWorkTime = TimeSpan.FromMinutes(100)},
				new TargetWorkTimeInterval { StartDate = now.AddDays(3), TargetWorkTime = TimeSpan.FromMinutes(110)},
			};

			var workTime1 = calendarLookup.GetTargetWorkTime(now, now, defaultWorkTime, targetWorkTimeIntervals);
			var workTime2 = calendarLookup.GetTargetWorkTime(now, now.AddDays(1), defaultWorkTime, targetWorkTimeIntervals);
			var workTime3 = calendarLookup.GetTargetWorkTime(now, now.AddDays(2), defaultWorkTime, targetWorkTimeIntervals);
			var workTime4 = calendarLookup.GetTargetWorkTime(now, now.AddDays(3), defaultWorkTime, targetWorkTimeIntervals);

			var expectedWorkTime = defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime1);
			expectedWorkTime += defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime2);
			expectedWorkTime += TimeSpan.FromMinutes(100);
			Assert.Equal(expectedWorkTime, workTime3);
			expectedWorkTime += TimeSpan.FromMinutes(110);
			Assert.Equal(expectedWorkTime, workTime4);
		}

		[Fact]
		public void TargetWorkTimeIntervalAfterLast()
		{
			var calIds = CreateComplexHierarchy();
			var inheritedId = calIds[4];
			var currCal = AddCalendar(new Calendar() { InheritedFrom = inheritedId, });
			var calendarLookup = new CalendarManager().GetCalenderLookup(currCal.Id);
			var defaultWorkTime = TimeSpan.FromMinutes(110);
			var targetWorkTimeIntervals = new List<TargetWorkTimeInterval>
			{
				new TargetWorkTimeInterval { StartDate = now.AddDays(-2), TargetWorkTime = TimeSpan.FromMinutes(100)},
				new TargetWorkTimeInterval { StartDate = now.AddDays(-1), TargetWorkTime = TimeSpan.FromMinutes(110)},
			};

			var workTime1 = calendarLookup.GetTargetWorkTime(now, now, defaultWorkTime, targetWorkTimeIntervals);
			var workTime2 = calendarLookup.GetTargetWorkTime(now, now.AddDays(1), defaultWorkTime, targetWorkTimeIntervals);
			var workTime3 = calendarLookup.GetTargetWorkTime(now, now.AddDays(2), defaultWorkTime, targetWorkTimeIntervals);
			var workTime4 = calendarLookup.GetTargetWorkTime(now, now.AddDays(3), defaultWorkTime, targetWorkTimeIntervals);

			var expectedWorkTime = defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime1);
			expectedWorkTime += defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime2);
			expectedWorkTime += defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime3);
			expectedWorkTime += defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime4);
		}

		[Fact]
		public void TargetWorkTimeIntervalMixed()
		{
			var calIds = CreateComplexHierarchy();
			var inheritedId = calIds[0];
			var currCal = AddCalendar(new Calendar() { InheritedFrom = inheritedId, });
			var calendarLookup = new CalendarManager().GetCalenderLookup(currCal.Id);
			var defaultWorkTime = TimeSpan.FromHours(2);
			var targetWorkTimeIntervals = new List<TargetWorkTimeInterval>
			{
				new TargetWorkTimeInterval { StartDate = now.AddDays(2), TargetWorkTime = TimeSpan.FromMinutes(100)},
				new TargetWorkTimeInterval { StartDate = now.AddDays(4), TargetWorkTime = TimeSpan.FromMinutes(110)},
			};

			var workTime1 = calendarLookup.GetTargetWorkTime(now, now, defaultWorkTime, targetWorkTimeIntervals);
			var workTime2 = calendarLookup.GetTargetWorkTime(now, now.AddDays(1), defaultWorkTime, targetWorkTimeIntervals);
			var workTime3 = calendarLookup.GetTargetWorkTime(now, now.AddDays(2), defaultWorkTime, targetWorkTimeIntervals);
			var workTime4 = calendarLookup.GetTargetWorkTime(now, now.AddDays(3), defaultWorkTime, targetWorkTimeIntervals);
			var workTime5 = calendarLookup.GetTargetWorkTime(now, now.AddDays(1), defaultWorkTime, targetWorkTimeIntervals);

			var expectedWorkTime = defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime1);
			expectedWorkTime += TimeSpan.FromMinutes(600);
			Assert.Equal(expectedWorkTime, workTime2);
			expectedWorkTime += TimeSpan.FromMinutes(100);
			Assert.Equal(expectedWorkTime, workTime3);
			expectedWorkTime += TimeSpan.FromMinutes(480);
			Assert.Equal(expectedWorkTime, workTime4);
			expectedWorkTime -= TimeSpan.FromMinutes(580);
			Assert.Equal(expectedWorkTime, workTime5);
		}

	}
}

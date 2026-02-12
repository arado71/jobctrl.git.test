using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{

	public class CalendarWorktimeSchedulesTests : CalendarTestsBase
	{
		private static readonly DateTime now = new DateTime(2011, 01, 17);

		private int[] CreateCalendars()
		{
			var cal1 = AddCalendar(new Calendar()
			{
				CalendarExceptions = new System.Data.Linq.EntitySet<CalendarException>()
				{
					new CalendarException() { Date = now, IsWorkDay = true },
					new CalendarException() { Date = now.AddDays(1), IsWorkDay = true, TargetWorkTimeInMinutes = 600 },
					new CalendarException() { Date = now.AddDays(2), IsWorkDay = true },
					new CalendarException() { Date = now.AddDays(3), IsWorkDay = true, TargetWorkTimeInMinutes = 480 },
				}
			});

			JobControlDataClassesDataContext.TimeSchedulesDataTest.Clear();
			JobControlDataClassesDataContext.TimeSchedulesDataTest.AddRange(new[]
			{
				new WorktimeSchedulesItem { Day = now.AddDays(1), IsWorkDay = false, }, // 2011/01/18 -8
				new WorktimeSchedulesItem { Day = now.AddDays(4), IsWorkDay = true, StartDate = now.AddDays(4).AddHours(5), EndDate = now.AddDays(4).AddHours(8) }, // 2011/01/21 -5
				new WorktimeSchedulesItem { Day = now.AddDays(6), IsWorkDay = true, StartDate = now.AddDays(6).AddHours(20), EndDate = now.AddDays(7).AddHours(4) }, // 2011/01/23 +4 -4
			});

			//  M  T  W  T  F  S  S
			//                 1  2
			//  3  4  5  6  7  8  9
			// 10 11 12 13 14 15 16
			// 17*18 19 20*21 22*23
			// 24 25 26 27 28 29 30
			// 31

			var cal2 = AddCalendar(new Calendar()
			{
				IsMondayWorkDay = true,
				IsTuesdayWorkDay = true,
				IsWednesdayWorkDay = true,
				IsThursdayWorkDay = true,
				IsFridayWorkDay = true,
			});

			var cal3 = AddCalendar(new Calendar()
			{
				IsMondayWorkDay = true, TargetWorkTimeInMinutesMonday = 480,
				IsTuesdayWorkDay = true, TargetWorkTimeInMinutesTuesday = 480,
				IsWednesdayWorkDay = true, TargetWorkTimeInMinutesWednesday = 480,
				IsThursdayWorkDay = true, TargetWorkTimeInMinutesThursday = 480,
				IsFridayWorkDay = true, TargetWorkTimeInMinutesFriday = 480,
			});

			JobControlDataClassesDataContext.TimeSchedulesEndOfDayInMinutes = 0;
			JobControlDataClassesDataContext.TimeSchedulesTimeZoneTest = TimeZoneInfo.FromSerializedString("Central Europe Standard Time;60;(UTC+01:00) Budapest, Belgrád, Ljubljana, Pozsony, Prága;Közép-európai téli idő ;Közép-európai nyári idő ;[01:01:0001;12:31:9999;60;[0;02:00:00;3;5;0;];[0;03:00:00;10;5;0;];];");

			return new[] { cal1.Id, cal2.Id, cal3.Id };
		}

		[Fact]
		public void WorkTimeCountComplex()
		{
			var calIds = CreateCalendars();
			var inheritedId = calIds[0];
			var currCal = AddCalendar(new Calendar() { InheritedFrom = inheritedId, });
			var calendarLookup = new CalendarManager().GetCalenderLookup(currCal.Id);
			var timeScheduleLookup = new WorktimeSchedulesLookup(1, calendarLookup);
			var defaultWorkTime = TimeSpan.FromHours(2);

			var workTime1 = timeScheduleLookup.GetTargetWorkTime(now, now, defaultWorkTime);
			var workTime2 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(1), defaultWorkTime);
			var workTime3 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(2), defaultWorkTime);
			var workTime4 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(3), defaultWorkTime);
			var workTime5 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(4), defaultWorkTime);
			var workTime6 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(5), defaultWorkTime);
			var workTime7 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(6), defaultWorkTime);
			var workTime8 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(7), defaultWorkTime);
			var workTime9 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(8), defaultWorkTime);

			var expectedWorkTime = defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime1);
			Assert.Equal(expectedWorkTime, workTime2);
			expectedWorkTime += defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime3);
			expectedWorkTime += TimeSpan.FromMinutes(480);
			Assert.Equal(expectedWorkTime, workTime4);
			expectedWorkTime += TimeSpan.FromHours(3);
			Assert.Equal(expectedWorkTime, workTime5);
			Assert.Equal(expectedWorkTime, workTime6);
			expectedWorkTime += TimeSpan.FromHours(3);
			Assert.Equal(expectedWorkTime, workTime7);
			expectedWorkTime += TimeSpan.FromHours(5);
			Assert.Equal(expectedWorkTime, workTime8);
			Assert.Equal(expectedWorkTime, workTime9);
		}

		[Fact]
		public void WorkTimeCountWithoutTimeSchedule()
		{
			var calIds = CreateCalendars();
			var inheritedId = calIds[0];
			var currCal = AddCalendar(new Calendar() { InheritedFrom = inheritedId, });
			var calendarLookup = new CalendarManager().GetCalenderLookup(currCal.Id);
			var timeScheduleLookup = new WorktimeSchedulesLookup(1, calendarLookup);
			var defaultWorkTime = TimeSpan.FromHours(2);

			var workTime1 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(2), now.AddDays(2), defaultWorkTime);
			var workTime2 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(2), now.AddDays(3), defaultWorkTime);
			var expectedWorkTime = defaultWorkTime;
			Assert.Equal(expectedWorkTime, workTime1);
			expectedWorkTime += TimeSpan.FromMinutes(480);
			Assert.Equal(expectedWorkTime, workTime2);
		}

		[Fact]
		public void WorkTimeCountAlternativeEndtimeOfDay()
		{
			var calIds = CreateCalendars();
			var inheritedId = calIds[0];
			var currCal = AddCalendar(new Calendar() { InheritedFrom = inheritedId, });
			var calendarLookup = new CalendarManager().GetCalenderLookup(currCal.Id);
			var timeScheduleLookup = new WorktimeSchedulesLookup(1, calendarLookup);
			var defaultWorkTime = TimeSpan.FromHours(2);
			JobControlDataClassesDataContext.TimeSchedulesEndOfDayInMinutes = (int)TimeSpan.FromHours(4).TotalMinutes;

			var workTime6 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(5), now.AddDays(5), defaultWorkTime);
			var workTime7 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(5), now.AddDays(6), defaultWorkTime);
			var workTime8 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(5), now.AddDays(7), defaultWorkTime);

			var expectedWorkTime = TimeSpan.Zero;
			Assert.Equal(expectedWorkTime, workTime6);
			expectedWorkTime += TimeSpan.FromHours(7);
			Assert.Equal(expectedWorkTime, workTime7);
			expectedWorkTime += TimeSpan.FromHours(1);
			Assert.Equal(expectedWorkTime, workTime8);
		}

		[Fact]
		public void WorkTimeCountOtherTimezone()
		{
			var calIds = CreateCalendars();
			var inheritedId = calIds[0];
			var currCal = AddCalendar(new Calendar() { InheritedFrom = inheritedId, });
			var calendarLookup = new CalendarManager().GetCalenderLookup(currCal.Id);
			var timeScheduleLookup = new WorktimeSchedulesLookup(1, calendarLookup);
			var defaultWorkTime = TimeSpan.FromHours(2);
			JobControlDataClassesDataContext.TimeSchedulesTimeZoneTest = TimeZoneInfo.CreateCustomTimeZone("UTC-4", TimeSpan.FromHours(-4), "New York Daylight Time", "EDT");

			var workTime6 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(5), now.AddDays(5), defaultWorkTime);
			var workTime7 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(5), now.AddDays(6), defaultWorkTime);
			var workTime8 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(5), now.AddDays(7), defaultWorkTime);

			var expectedWorkTime = TimeSpan.Zero;
			Assert.Equal(expectedWorkTime, workTime6);
			expectedWorkTime += TimeSpan.FromHours(8);
			Assert.Equal(expectedWorkTime, workTime7);
			Assert.Equal(expectedWorkTime, workTime8);
		}

		[Fact]
		public void WorkTimeCountWeekInNormalCalendar()
		{
			var calIds = CreateCalendars();
			var calendarLookup = new CalendarManager().GetCalenderLookup(calIds[1]);
			var timeScheduleLookup = new WorktimeSchedulesLookup(1, calendarLookup);
			var defaultWorkTime = TimeSpan.FromHours(8);

			var workTime1 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(15), now.AddDays(15), defaultWorkTime); // days without worktimeschedules
			var workTime2 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(14), now.AddDays(20), defaultWorkTime); // days without worktimeschedules
			var workTime3 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(8), now.AddDays(35), defaultWorkTime); // days without worktimeschedules
			Assert.Equal(TimeSpan.FromHours(8), workTime1);
			Assert.Equal(TimeSpan.FromHours(40), workTime2);
			Assert.Equal(TimeSpan.FromHours(160), workTime3);
		}

		[Fact]
		public void WorkTimeCountWeekInNormalCalendarWithWorktimeSchedule()
		{
			var calIds = CreateCalendars();
			var calendarLookup = new CalendarManager().GetCalenderLookup(calIds[1]);
			var timeScheduleLookup = new WorktimeSchedulesLookup(1, calendarLookup);
			var defaultWorkTime = TimeSpan.FromHours(8);

			var workTime1 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(2), now.AddDays(2), defaultWorkTime); 
			var workTime2 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(6), defaultWorkTime);
			var firstDayOfMonth = now.AddDays(-now.Day + 1);
			var workTime3 = timeScheduleLookup.GetTargetWorkTime(firstDayOfMonth, firstDayOfMonth.AddDays(23), defaultWorkTime); 
			var workTime4 = timeScheduleLookup.GetTargetWorkTime(firstDayOfMonth, firstDayOfMonth.AddMonths(1).AddDays(-1), defaultWorkTime); 
			Assert.Equal(TimeSpan.FromHours(8), workTime1);
			Assert.Equal(TimeSpan.FromHours(30), workTime2);
			Assert.Equal(TimeSpan.FromHours(16 * 8 - 13), workTime3);
			Assert.Equal(TimeSpan.FromHours(21 * 8 - 13), workTime4);
		}

		[Fact]
		public void WorkTimeCountWeekInNormalCalSpecWorktimes()
		{
			var calIds = CreateCalendars();
			var calendarLookup = new CalendarManager().GetCalenderLookup(calIds[2]);
			var timeScheduleLookup = new WorktimeSchedulesLookup(1, calendarLookup);
			var defaultWorkTime = TimeSpan.FromHours(8);

			var workTime1 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(8), now.AddDays(14), defaultWorkTime); // days without worktimeschedules
			var expectedWorkTime = TimeSpan.FromHours(40);
			Assert.Equal(expectedWorkTime, workTime1);
		}

		[Fact]
		public void WorkTimeCountWeekInNormalCalSpecWorktimesWithWorktimeSchedule()
		{
			var calIds = CreateCalendars();
			var calendarLookup = new CalendarManager().GetCalenderLookup(calIds[2]);
			var timeScheduleLookup = new WorktimeSchedulesLookup(1, calendarLookup);
			var defaultWorkTime = TimeSpan.FromHours(8);

			var workTime1 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(6), defaultWorkTime);
			var expectedWorkTime = TimeSpan.FromHours(30);
			Assert.Equal(expectedWorkTime, workTime1);
		}

		[Fact]
		public void TargetWorktimeIntervalBeforeFirst()
		{
			var calIds = CreateCalendars();
			var calendarLookup = new CalendarManager().GetCalenderLookup(calIds[1]);
			var timeScheduleLookup = new WorktimeSchedulesLookup(1, calendarLookup);
			var defaultWorkTime = TimeSpan.FromHours(8);
			var targetWorkTimeIntervals = new List<TargetWorkTimeInterval>
			{
				new TargetWorkTimeInterval { StartDate = now.AddDays(24), TargetWorkTime = TimeSpan.FromHours(5)},
				new TargetWorkTimeInterval { StartDate = now.AddDays(26), TargetWorkTime = TimeSpan.FromHours(6)},
			};

			var workTime1 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(2), now.AddDays(2), defaultWorkTime, targetWorkTimeIntervals);
			var workTime2 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(6), defaultWorkTime, targetWorkTimeIntervals);
			var firstDayOfMonth = now.AddDays(-now.Day + 1);
			var workTime3 = timeScheduleLookup.GetTargetWorkTime(firstDayOfMonth, firstDayOfMonth.AddDays(23), defaultWorkTime, targetWorkTimeIntervals);
			var workTime4 = timeScheduleLookup.GetTargetWorkTime(firstDayOfMonth, firstDayOfMonth.AddMonths(1).AddDays(-1), defaultWorkTime, targetWorkTimeIntervals);
			var workTime5 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(5), defaultWorkTime, targetWorkTimeIntervals);
			Assert.Equal(TimeSpan.FromHours(8), workTime1);
			Assert.Equal(TimeSpan.FromHours(30), workTime2);
			Assert.Equal(TimeSpan.FromHours(16 * 8 - 13), workTime3);
			Assert.Equal(TimeSpan.FromHours(21 * 8 - 13), workTime4);
			Assert.Equal(TimeSpan.FromHours(27), workTime5);
		}

		[Fact]
		public void TargetWorktimeIntervalAfterLast()
		{
			var calIds = CreateCalendars();
			var calendarLookup = new CalendarManager().GetCalenderLookup(calIds[1]);
			var timeScheduleLookup = new WorktimeSchedulesLookup(1, calendarLookup);
			var defaultWorkTime = TimeSpan.FromHours(8);
			var targetWorkTimeIntervals = new List<TargetWorkTimeInterval>
			{
				new TargetWorkTimeInterval { StartDate = now.AddDays(-24), TargetWorkTime = TimeSpan.FromHours(5)},
				new TargetWorkTimeInterval { StartDate = now.AddDays(-22), TargetWorkTime = TimeSpan.FromHours(6)},
			};

			var workTime1 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(2), now.AddDays(2), defaultWorkTime, targetWorkTimeIntervals);
			var workTime2 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(6), defaultWorkTime, targetWorkTimeIntervals);
			var firstDayOfMonth = now.AddDays(-now.Day + 1);
			var workTime3 = timeScheduleLookup.GetTargetWorkTime(firstDayOfMonth, firstDayOfMonth.AddDays(23), defaultWorkTime, targetWorkTimeIntervals);
			var workTime4 = timeScheduleLookup.GetTargetWorkTime(firstDayOfMonth, firstDayOfMonth.AddMonths(1).AddDays(-1), defaultWorkTime, targetWorkTimeIntervals);
			Assert.Equal(TimeSpan.FromHours(6), workTime1);
			Assert.Equal(TimeSpan.FromHours(24), workTime2);
			Assert.Equal(TimeSpan.FromHours(16 * 6 - 7), workTime3);
			Assert.Equal(TimeSpan.FromHours(21 * 6 - 7), workTime4);
		}

		[Fact]
		public void TargetWorktimeIntervalOverlapped()
		{
			var calIds = CreateCalendars();
			var calendarLookup = new CalendarManager().GetCalenderLookup(calIds[1]);
			var timeScheduleLookup = new WorktimeSchedulesLookup(1, calendarLookup);
			var defaultWorkTime = TimeSpan.FromHours(8);
			var targetWorkTimeIntervals = new List<TargetWorkTimeInterval>
			{
				new TargetWorkTimeInterval { StartDate = now.AddDays(-7), TargetWorkTime = TimeSpan.FromHours(5)},
				new TargetWorkTimeInterval { StartDate = now.AddDays(3), TargetWorkTime = TimeSpan.FromHours(6)},
			};

			var workTime1 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(2), now.AddDays(2), defaultWorkTime, targetWorkTimeIntervals);
			var workTime2 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(6), defaultWorkTime, targetWorkTimeIntervals);
			var firstDayOfMonth = now.AddDays(-now.Day + 1);
			var workTime3 = timeScheduleLookup.GetTargetWorkTime(firstDayOfMonth, firstDayOfMonth.AddDays(23), defaultWorkTime, targetWorkTimeIntervals);
			var workTime4 = timeScheduleLookup.GetTargetWorkTime(firstDayOfMonth, firstDayOfMonth.AddMonths(1).AddDays(-1), defaultWorkTime, targetWorkTimeIntervals);
			Assert.Equal(TimeSpan.FromHours(5), workTime1);
			Assert.Equal(TimeSpan.FromHours(22), workTime2);
			Assert.Equal(TimeSpan.FromHours(5 * 8 + 8 * 5 + 3 * 6 - 6), workTime3);
			Assert.Equal(TimeSpan.FromHours(5 * 8 + 8 * 5 + 8 * 6 - 6), workTime4);
		}

		[Fact]
		public void TargetWorktimeIntervalOnlyBeforeFirst()
		{
			var calIds = CreateCalendars();
			var calendarLookup = new CalendarManager().GetCalenderLookup(calIds[1]);
			var timeScheduleLookup = new WorktimeSchedulesLookup(1, calendarLookup);
			var defaultWorkTime = TimeSpan.FromHours(7);
			var targetWorkTimeIntervals = new List<TargetWorkTimeInterval>
			{
				new TargetWorkTimeInterval { StartDate = now.AddDays(-30), TargetWorkTime = TimeSpan.FromHours(8)},
				new TargetWorkTimeInterval { StartDate = now.AddDays(24), TargetWorkTime = TimeSpan.FromHours(5)},
				new TargetWorkTimeInterval { StartDate = now.AddDays(26), TargetWorkTime = TimeSpan.FromHours(6)},
			};

			var workTime1 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(2), now.AddDays(2), defaultWorkTime, targetWorkTimeIntervals);
			var workTime2 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(6), defaultWorkTime, targetWorkTimeIntervals);
			var firstDayOfMonth = now.AddDays(-now.Day + 1);
			var workTime3 = timeScheduleLookup.GetTargetWorkTime(firstDayOfMonth, firstDayOfMonth.AddDays(23), defaultWorkTime, targetWorkTimeIntervals);
			var workTime4 = timeScheduleLookup.GetTargetWorkTime(firstDayOfMonth, firstDayOfMonth.AddMonths(1).AddDays(-1), defaultWorkTime, targetWorkTimeIntervals);
			var workTime5 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(5), defaultWorkTime, targetWorkTimeIntervals);
			Assert.Equal(TimeSpan.FromHours(8), workTime1);
			Assert.Equal(TimeSpan.FromHours(30), workTime2);
			Assert.Equal(TimeSpan.FromHours(16 * 8 - 13), workTime3);
			Assert.Equal(TimeSpan.FromHours(21 * 8 - 13), workTime4);
			Assert.Equal(TimeSpan.FromHours(27), workTime5);
		}

		[Fact]
		public void TargetWorktimeIntervalOnlyAfterLast()
		{
			var calIds = CreateCalendars();
			var calendarLookup = new CalendarManager().GetCalenderLookup(calIds[1]);
			var timeScheduleLookup = new WorktimeSchedulesLookup(1, calendarLookup);
			var defaultWorkTime = TimeSpan.FromHours(7);
			var targetWorkTimeIntervals = new List<TargetWorkTimeInterval>
			{
				new TargetWorkTimeInterval { StartDate = now.AddDays(-30), TargetWorkTime = TimeSpan.FromHours(8)},
				new TargetWorkTimeInterval { StartDate = now.AddDays(-24), TargetWorkTime = TimeSpan.FromHours(5)},
				new TargetWorkTimeInterval { StartDate = now.AddDays(-22), TargetWorkTime = TimeSpan.FromHours(6)},
			};

			var workTime1 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(2), now.AddDays(2), defaultWorkTime, targetWorkTimeIntervals);
			var workTime2 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(6), defaultWorkTime, targetWorkTimeIntervals);
			var firstDayOfMonth = now.AddDays(-now.Day + 1);
			var workTime3 = timeScheduleLookup.GetTargetWorkTime(firstDayOfMonth, firstDayOfMonth.AddDays(23), defaultWorkTime, targetWorkTimeIntervals);
			var workTime4 = timeScheduleLookup.GetTargetWorkTime(firstDayOfMonth, firstDayOfMonth.AddMonths(1).AddDays(-1), defaultWorkTime, targetWorkTimeIntervals);
			Assert.Equal(TimeSpan.FromHours(6), workTime1);
			Assert.Equal(TimeSpan.FromHours(24), workTime2);
			Assert.Equal(TimeSpan.FromHours(16 * 6 - 7), workTime3);
			Assert.Equal(TimeSpan.FromHours(21 * 6 - 7), workTime4);
		}

		[Fact]
		public void TargetWorktimeIntervalOnlyOverlapped()
		{
			var calIds = CreateCalendars();
			var calendarLookup = new CalendarManager().GetCalenderLookup(calIds[1]);
			var timeScheduleLookup = new WorktimeSchedulesLookup(1, calendarLookup);
			var defaultWorkTime = TimeSpan.FromHours(7);
			var targetWorkTimeIntervals = new List<TargetWorkTimeInterval>
			{
				new TargetWorkTimeInterval { StartDate = now.AddDays(-30), TargetWorkTime = TimeSpan.FromHours(8)},
				new TargetWorkTimeInterval { StartDate = now.AddDays(-7), TargetWorkTime = TimeSpan.FromHours(5)},
				new TargetWorkTimeInterval { StartDate = now.AddDays(3), TargetWorkTime = TimeSpan.FromHours(6)},
			};

			var workTime1 = timeScheduleLookup.GetTargetWorkTime(now.AddDays(2), now.AddDays(2), defaultWorkTime, targetWorkTimeIntervals);
			var workTime2 = timeScheduleLookup.GetTargetWorkTime(now, now.AddDays(6), defaultWorkTime, targetWorkTimeIntervals);
			var firstDayOfMonth = now.AddDays(-now.Day + 1);
			var workTime3 = timeScheduleLookup.GetTargetWorkTime(firstDayOfMonth, firstDayOfMonth.AddDays(23), defaultWorkTime, targetWorkTimeIntervals);
			var workTime4 = timeScheduleLookup.GetTargetWorkTime(firstDayOfMonth, firstDayOfMonth.AddMonths(1).AddDays(-1), defaultWorkTime, targetWorkTimeIntervals);
			Assert.Equal(TimeSpan.FromHours(5), workTime1);
			Assert.Equal(TimeSpan.FromHours(22), workTime2);
			Assert.Equal(TimeSpan.FromHours(5 * 8 + 8 * 5 + 3 * 6 - 6), workTime3);
			Assert.Equal(TimeSpan.FromHours(5 * 8 + 8 * 5 + 8 * 6 - 6), workTime4);
		}

		[Fact]
		public void WorkdaysWithCustomSchedules()
		{
			var calIds = CreateCalendars();
			var calendarLookup = new CalendarManager().GetCalenderLookup(calIds[1]);
			var timeScheduleLookup = new WorktimeSchedulesLookup(1, calendarLookup);
			var workDays = timeScheduleLookup.GetWorkDays(now, now.AddDays(8));
			Assert.Equal(new[] { 0, 2, 3, 4, 6, 7, 8 }.Select(n => now.AddDays(n)).ToList(), workDays);
		}
	}
}
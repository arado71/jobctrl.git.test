using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public abstract class CalendarTestsBase
	{
		private static readonly TestDb testDb = new TestDb();

		static CalendarTestsBase()
		{
			testDb.InitializeDatabase();
			Tct.ActivityRecorderService.Properties.Settings.Default["recorderConnectionString"] = testDb.ConnectionString;
			Tct.ActivityRecorderService.Properties.Settings.Default["jobcontrolivrConnectionString"] = testDb.ConnectionString;
		}

		protected CalendarTestsBase()
		{
			testDb.PurgeDatabase();

			SetUserStatInfoForEveryBody(new Calendar()
			{
				IsMondayWorkDay = true,
				IsTuesdayWorkDay = true,
				IsWednesdayWorkDay = true,
				IsThursdayWorkDay = true,
				IsFridayWorkDay = true,
				IsSaturdayWorkDay = true,
				IsSundayWorkDay = true
			});
		}

		protected void SetUserStatInfoForEveryBody(Calendar calendar)
		{
			SetUserStatInfoForEveryBody(calendar, TimeZoneInfo.Utc);
		}

		protected void SetUserStatInfoForEveryBody(Calendar calendar, TimeZoneInfo timeZone)
		{
			Debug.Assert(timeZone != null);
			AddCalendar(calendar);
		}

		protected Calendar AddCalendar(Calendar calendar)
		{
			Debug.Assert(calendar != null);
			using (var context = new IvrDataClassesDataContext())
			{
				context.Calendars.InsertOnSubmit(calendar);
				context.SubmitChanges();
				return calendar;
			}
		}

	}
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderService.Caching;

namespace Tct.ActivityRecorderService
{
	public class CalendarManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly object createLock = new object();
		private readonly ThreadSafeCachedFunc<int, CalendarLookup> calendarResolver;


		public CalendarManager() : base(log)
		{
			ManagerCallbackInterval = ConfigManager.CalendarUpdateInterval;
			calendarResolver = new ThreadSafeCachedFunc<int, CalendarLookup>(CreateCalendarLookup, TimeSpan.FromMilliseconds(ConfigManager.CalendarUpdateInterval)); 
		}

		protected override void ManagerCallbackImpl()
		{
			calendarResolver.Clear();
		}

		public CalendarLookup GetCalenderLookup(int calendarId)
		{
			return calendarResolver.GetOrCalculateValue(calendarId);
		}

		private static CalendarLookup CreateCalendarLookup(int calendarId)
		{
			return new CalendarLookup(calendarId);
		}

	}
}

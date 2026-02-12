using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Capturing.Extra
{
	public class CalendarManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const int CalendarUpdateInterval = 60 * 60 * 1000; // 1 hour
		private const int CalendarUpdateErrorInterval = 60 * 1000; // 1 min
		private static string FilePath => "CalendarWorkdays-" + ConfigManager.UserId;

		private bool lastSendFailed;
		private readonly object lockObj = new object();
		private List<DateTime> workdays;

		public List<DateTime> Workdays
		{
			get
			{
				lock (lockObj) return workdays?.ToList();
			}
			private set
			{
				lock (lockObj)
				{
					if (XmlSerializationHelper.AreTheSame(workdays, value) || value == null && workdays != null) return;
					log.Info("Calendar workdays changed: " + string.Join(", ", value?.Select(d => d.ToShortDateString()) ?? new string[0]));
					workdays = value;
					IsolatedStorageSerializationHelper.Save(FilePath, value);
				}
			}
		}

		public bool IsWorkday(DateTime day)
		{
			var _workdays = Workdays;
			return _workdays != null && _workdays.Max() > day ? _workdays.Any(d => d.Date == day.Date) : day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday;
		}

		public CalendarManager() : base(log)
		{
			log.Info("Loading workdays from disk");
			if (IsolatedStorageSerializationHelper.Exists(FilePath)
			    && IsolatedStorageSerializationHelper.Load(FilePath, out List<DateTime> value))
			{
				workdays = value;
			}
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				var result = ActivityRecorderClientWrapper.Execute(n => n.GetUserWorkdays(ConfigManager.UserId));
				lastSendFailed = false;
				Workdays = result;
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get Calendar workdays", log, ex);
				lastSendFailed = true; //retry shortly
			}
		}

		protected override int ManagerCallbackInterval => lastSendFailed ? CalendarUpdateErrorInterval : CalendarUpdateInterval;
	}
}

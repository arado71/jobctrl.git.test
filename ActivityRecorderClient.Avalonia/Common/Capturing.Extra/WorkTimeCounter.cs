using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Serialization;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Extra
{
	/// <summary>
	/// Thread-safe work time counter
	/// </summary>
	public class WorkTimeCounter
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static string TodaysWorkTimePath { get { return "TodaysWorkTime-" + ConfigManager.UserId; } }
		//private static string RecentWorkTimesPath { get { return "RecentWorkTimes-" + ConfigManager.UserId; } }
		private KeyValuePair<DateTime, long> todaysWorkTime;
		private readonly object thisLock = new object();
		private List<Interval> workIntervals = new List<Interval>();

		public TimeSpan TodaysWorkTime
		{
			get
			{
				var todaysDate = GetDateFromUtcTime(DateTime.UtcNow);
				lock (thisLock)
				{
					return todaysWorkTime.Key == todaysDate ? new TimeSpan(todaysWorkTime.Value) : TimeSpan.Zero;
				}
			}
		}

		public WorkTimeCounter()
		{
			todaysWorkTime = new KeyValuePair<DateTime, long>(GetDateFromUtcTime(DateTime.UtcNow), 0);
		}

		//todo get cutOffOffset from server
		private static readonly TimeSpan cutOffOffset = TimeSpan.FromHours(3);
		private static DateTime GetDateFromUtcTime(DateTime utcTime)
		{
			var time = TimeZone.CurrentTimeZone.ToLocalTime(utcTime);
			return (time - cutOffOffset).Date;
		}

		public void Load()
		{
			lock (thisLock)
			{
				KeyValuePair<DateTime, long> loadedWorkTime;
				if (!IsolatedStorageSerializationHelper.Exists(TodaysWorkTimePath)) return;
				if (!IsolatedStorageSerializationHelper.Load(TodaysWorkTimePath, out loadedWorkTime)) return;
				log.Info("Loaded work time from disk for " + loadedWorkTime.Key.ToString("yyyy-MM-dd") + ": " + new TimeSpan(loadedWorkTime.Value).ToHourMinuteSecondString());
				if (GetDateFromUtcTime(DateTime.UtcNow) != loadedWorkTime.Key) return; //if the data isn't for today
				todaysWorkTime = loadedWorkTime;
			}
		}

		public void AddWorkItem(WorkItem workItem)
		{
			if (workItem == null) return;
			var todaysDate = GetDateFromUtcTime(DateTime.UtcNow);
			if (GetDateFromUtcTime(workItem.StartDate) != todaysDate) return; //if the data isn't for today
			//there is a race here (an other thread might have written data for tomorrow which will be lost)
			//very rare I don't care (can be fixed if recent work time is also saved)
			lock (thisLock)
			{
				long workTime = 0;
				if (todaysDate == todaysWorkTime.Key)
				{
					workTime = todaysWorkTime.Value;
				}

				workTime += workItem.EndDate.Ticks - workItem.StartDate.Ticks;
				todaysWorkTime = new KeyValuePair<DateTime, long>(todaysDate, workTime);
				IsolatedStorageSerializationHelper.Save(TodaysWorkTimePath, todaysWorkTime);
				workIntervals.Add(new Interval(workItem.StartDate, workItem.EndDate));
			}
		}

		internal TimeSpan GetLocalWorkTimeFromTime(DateTime time)
		{
			lock (thisLock)
			{
				TimeSpan result = TimeSpan.Zero;
				int i = workIntervals.Count - 1;
				while (i > 0)
				{
					Interval interval = workIntervals[i];
					if (workIntervals[i].EndDate < time) break;
					if (workIntervals[i].StartDate > time)
					{
						result += interval.EndDate - interval.StartDate;
					}
					else
					{
						result += interval.EndDate - time;
					}
					i--;
				}
				return result;
			}
		}
	}
}

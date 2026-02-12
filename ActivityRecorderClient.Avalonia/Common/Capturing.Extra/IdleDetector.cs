using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Extra
{
	/// <summary>
	/// Thread-safe idle detector after work time
	/// </summary>
	public class IdleDetector
	{
		private static readonly long OneMinuteTicks = TimeSpan.FromMinutes(1).Ticks;

		public TimeSpan WorkTimeStart { get { return TimeSpan.FromMinutes(ConfigManager.WorkTimeStartInMins); } }
		public TimeSpan WorkTimeEnd { get { return TimeSpan.FromMinutes(ConfigManager.WorkTimeEndInMins); } }
		public long AfterWorkTimeIdleTicks { get { return ConfigManager.AfterWorkTimeIdleInMins * OneMinuteTicks; } }
		public long DuringWorkTimeIdleTicks { get { return ConfigManager.DuringWorkTimeIdleInMins * OneMinuteTicks; } }
		public TimeSpan IdleDuringWorkTime { get { return TimeSpan.FromTicks(Interlocked.Read(ref idleDuringWorkTimeTicks)); } }
		public TimeSpan IdleAfterWorkTime { get { return TimeSpan.FromTicks(Interlocked.Read(ref idleAfterWorkTimeTicks)); } }
		public Action<Rules.IWorkChangingRule> ProcessIdleReset { get; set; }

		private long idleDuringWorkTimeTicks;
		private long idleAfterWorkTimeTicks;
		private DateTime? lastWorkItemEnd;
		private readonly CalendarManager calendarManager;

		public IdleDetector(CalendarManager calendarManager)
		{
			this.calendarManager = calendarManager;
		}

		public void ResetIdleWorkTime()
		{
			Interlocked.Exchange(ref idleDuringWorkTimeTicks, 0);
			Interlocked.Exchange(ref idleAfterWorkTimeTicks, 0);
			lastWorkItemEnd = null;
		}

		public void AddWorkItem(WorkItem workItem)
		{
			if (workItem == null) return;
			if (workItem.MouseActivity != 0 || workItem.KeyboardActivity != 0)
			{
				ResetIdleWorkTime();
				return;
			}
			lastWorkItemEnd = workItem.EndDate;
			if (calendarManager.IsWorkday(TimeZone.CurrentTimeZone.ToLocalTime(workItem.EndDate))
				    && TimeZone.CurrentTimeZone.ToLocalTime(workItem.StartDate).TimeOfDay <= WorkTimeEnd
					&& TimeZone.CurrentTimeZone.ToLocalTime(workItem.EndDate).TimeOfDay >= WorkTimeStart)
				Interlocked.Add(ref idleDuringWorkTimeTicks, workItem.EndDate.Ticks - workItem.StartDate.Ticks);
			else
				Interlocked.Add(ref idleAfterWorkTimeTicks, workItem.EndDate.Ticks - workItem.StartDate.Ticks);
		}

		public bool IsIdleAfterWorkTime
		{
			get
			{
				var afterWorkTimeIdleTicks = AfterWorkTimeIdleTicks;
				return afterWorkTimeIdleTicks > 0 && Interlocked.Read(ref idleAfterWorkTimeTicks) > afterWorkTimeIdleTicks;
			}
		}

		public bool IsIdleDuringWorkTime
		{
			get
			{
				var duringWorkTimeIdleTicks = DuringWorkTimeIdleTicks;
				return duringWorkTimeIdleTicks > 0 && Interlocked.Read(ref idleDuringWorkTimeTicks) > duringWorkTimeIdleTicks;
			}
		}

		public void ResetIdleWorkTimeIfNecessary(Rules.IWorkChangingRule matchingRule)
		{
			ProcessIdleReset?.Invoke(matchingRule);
		}

		public long RemainingIdleTime
		{
			get
			{
				if (lastWorkItemEnd == null) return long.MaxValue;
				var aggrRemWorkTime = (calendarManager.IsWorkday(DateTime.Now) && DateTime.Now.TimeOfDay <= WorkTimeEnd && DateTime.Now.TimeOfDay >= WorkTimeStart ? DuringWorkTimeIdleTicks - Interlocked.Read(ref idleDuringWorkTimeTicks) : AfterWorkTimeIdleTicks - Interlocked.Read(ref idleAfterWorkTimeTicks)) / TimeSpan.TicksPerMillisecond;
				if (aggrRemWorkTime > ConfigManager.CaptureWorkItemInterval) return long.MaxValue;
				var lastRemWorkTime = (long)(lastWorkItemEnd.Value - DateTime.UtcNow).TotalMilliseconds + ConfigManager.CaptureWorkItemInterval;
				return lastRemWorkTime;
			}
		}

		public void ResetRemainingLastFraction()
		{
			lastWorkItemEnd = null;
		}
	}
}

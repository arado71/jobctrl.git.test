using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderClient.Meeting.Adhoc
{
	public class MeetingWorkTimeCounter
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly long startTicks;

		private long? stopTicks;
		private uint pausedInterval;
		private int? pauseStartTicks;

		public DateTime StartTime { get; private set; }
		public DateTime EndTime { get { return StartTime.AddMilliseconds((stopTicks ?? Environment.TickCount) - StartTicks); } }
		public long StartTicks { get { return startTicks; } }
		public bool IsPaused { get { return pauseStartTicks.HasValue; } }
		public bool IsStopped { get { return stopTicks.HasValue; } }

		public static MeetingWorkTimeCounter GetFinished(DateTime startDate, DateTime endDate, TimeSpan pausedDuration)
		{
			return new MeetingWorkTimeCounter(endDate - startDate, startDate, pausedDuration);
		}

		public static MeetingWorkTimeCounter StartNew(TimeSpan includedInterval)
		{
			return new MeetingWorkTimeCounter(includedInterval, null, TimeSpan.Zero);
		}
		public static MeetingWorkTimeCounter Restart(StartEndDateTime? meetingTime)
		{
			return new MeetingWorkTimeCounter(meetingTime);
		}
		private MeetingWorkTimeCounter(StartEndDateTime? meetingTime)
		{
			if (meetingTime.HasValue)
			{
				StartTime = meetingTime.Value.StartDate;
				startTicks = Environment.TickCount - (DateTime.UtcNow - meetingTime.Value.StartDate).Ticks / TimeSpan.TicksPerMillisecond;
			}
		}
		private MeetingWorkTimeCounter(TimeSpan includedInterval, DateTime? startTime, TimeSpan pausedDuration)
		{
			if (includedInterval < TimeSpan.Zero || includedInterval > TimeSpan.FromDays(24)) throw new ArgumentOutOfRangeException(nameof(includedInterval));
			var includedIdleInMs = (int)includedInterval.TotalMilliseconds;
			StartTime = startTime ?? DateTime.UtcNow.AddMilliseconds(-includedIdleInMs);
			startTicks = !startTime.HasValue ? (Environment.TickCount - includedIdleInMs) : 0;
			stopTicks = !startTime.HasValue ? default(long?) : startTicks + includedIdleInMs;
			pausedInterval = (uint)pausedDuration.TotalMilliseconds;
		}

		public void StopWork([System.Runtime.CompilerServices.CallerMemberName] string callerName = "")
		{
			if (IsStopped) return;
			stopTicks = Environment.TickCount;
			log.Info("StopWork called " + GetDuration().ToHourMinuteSecondString() + " p: " + GetPausedDuration().ToHourMinuteSecondString() + (callerName != string.Empty ? " caller: " + callerName : ""));
		}

		public void PauseWork()
		{
			if (IsStopped) return;
			if (IsPaused) return;
			pauseStartTicks = Environment.TickCount;
			log.Info("PauseWork called " + GetDuration().ToHourMinuteSecondString() + " p: " + GetPausedDuration().ToHourMinuteSecondString());
			Debug.Assert(IsPaused);
		}

		public void ResumeWork()
		{
			if (!IsPaused) return;
			pausedInterval += (uint)(Environment.TickCount - pauseStartTicks.Value);
			pauseStartTicks = null;
			log.Info("ResumeWork called " + GetDuration().ToHourMinuteSecondString() + " p: " + GetPausedDuration().ToHourMinuteSecondString());
			Debug.Assert(!IsPaused);
		}

		public TimeSpan GetDuration()
		{
			var now = Environment.TickCount;
			var end = stopTicks ?? now;
			var paused = pausedInterval + (IsPaused ? (uint)(end - pauseStartTicks.Value) : 0);
			return TimeSpan.FromMilliseconds((uint)(end - startTicks) - paused);
		}

		public TimeSpan GetPausedDuration()
		{
			var now = Environment.TickCount;
			var end = stopTicks ?? now;
			var paused = pausedInterval + (IsPaused ? (uint)(end - pauseStartTicks.Value) : 0);
			return TimeSpan.FromMilliseconds(paused);
		}
	}
}
namespace System.Runtime.CompilerServices
{
	using System;

	[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
	public sealed class CallerMemberNameAttribute : Attribute
	{
	}
}

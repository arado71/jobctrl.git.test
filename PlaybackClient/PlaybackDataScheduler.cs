using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderService;

namespace PlaybackClient
{
	/// <summary>
	/// Thread-safe class for executing PlaybackSchedules at the rigth time.
	/// </summary>
	/// <remarks>
	/// This class will call IPlaybackDataCollector to get data to send.
	/// Convert it to actualized PlaybackDataItems via IPlaybackDataConverter.
	/// Finally send them via IPlaybackDataSender.
	/// Individual PlaybackDataItems are not scheduled by this class.
	/// They are passed as a big collection to IPlaybackDataSender which will send them at the right time.
	/// </remarks>
	public class PlaybackDataScheduler : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IPlaybackDataSender sender;
		private readonly IPlaybackDataCollector collector;
		private readonly IPlaybackDataConverter converter;
		private readonly object thisLock = new object();
		private bool isDisposed;

		public PlaybackDataScheduler()
			: this(() => new PlaybackDataSender(), () => new PlaybackDataCollector(), () => new PlaybackDataConverter())
		{
		}

		public PlaybackDataScheduler(Func<IPlaybackDataSender> senderFunc, Func<IPlaybackDataCollector> collectorFunc, Func<IPlaybackDataConverter> converterFunc)
		{
			sender = senderFunc();
			collector = collectorFunc();
			converter = converterFunc();
		}

		public void Add(PlaybackSchedule schedule, DateTime? scheduleStart = null)
		{
			var utcNow = scheduleStart ?? DateTimeEx.UtcNow();
			var length = schedule.EndDate - schedule.StartDate;
			if (length <= TimeSpan.Zero) throw new ArgumentOutOfRangeException("schedule");
			log.Info("Adding schedule " + schedule + " from " + scheduleStart);
			var utcStartTime = utcNow - length;
			while (true)
			{
				if (!schedule.TryGetNextUtcStartDate(utcStartTime, out utcStartTime)) return; //no more occurances
				if (utcStartTime < utcNow) //already running so resume
				{
					Debug.Assert(utcStartTime + length > utcNow);
					ResumePlayback(schedule, utcNow, utcStartTime);
				}
				else
				{
					//significant time could have been passed since utcNow
					SchedulePlayback(schedule, utcStartTime);
					return;
				}
			}
		}

		protected virtual void SchedulePlayback(PlaybackSchedule schedule, DateTime utcFirstStart)
		{
			lock (thisLock)
			{
				if (isDisposed) return;
			}
			var utcNow = DateTimeEx.UtcNow();
			while (utcFirstStart <= utcNow) //we are cathing up
			{
				ResumePlayback(schedule, utcFirstStart, utcFirstStart); //play the whole thing
				if (!schedule.TryGetNextUtcStartDate(utcFirstStart, out utcFirstStart)) return; //no more occurances
				utcNow = DateTimeEx.UtcNow();
			}

			Debug.Assert(utcFirstStart > utcNow);
			TaskEx.Delay(utcFirstStart - utcNow) //schedule it for later execution
				.ContinueWith(ant =>
								{
									ResumePlayback(schedule, utcFirstStart, utcFirstStart); //play the whole thing
									if (!schedule.TryGetNextUtcStartDate(utcFirstStart, out utcFirstStart)) return; //no more occurances
									SchedulePlayback(schedule, utcFirstStart);
								});
		}

		protected virtual void ResumePlayback(PlaybackSchedule schedule, DateTime utcSendFrom, DateTime utcFirstStart, int retries = 0) //this should not throw
		{
			lock (thisLock)
			{
				if (isDisposed) return;
			}
			try
			{
				Debug.Assert(utcSendFrom >= utcFirstStart);
				if (utcSendFrom == utcFirstStart)
				{
					log.Info("Starting playback (" + retries + ") " + utcFirstStart);
				}
				else
				{
					log.Info("Resuming playback (" + retries + ") " + utcFirstStart + " from " + utcSendFrom);
				}
				var data = collector.GetDataFor(schedule.UserId, schedule.StartDate, schedule.EndDate); //this can be really slow
				if (data == null)
				{
					log.Warn("Found no data for " + schedule + " from: " + utcSendFrom + " start: " + utcFirstStart);
					return;
				}
				var items = converter.GetActualizedItems(data, utcFirstStart, utcSendFrom);
				sender.SendAsync(items);
			}
			catch (Exception ex)
			{
				log.Error("Playback failed " + schedule + " from: " + utcSendFrom + " start: " + utcFirstStart, ex);
				if (retries >= GetScheduleRetries())
				{
					log.Fatal("No longer retrying playback " + schedule + " from: " + utcSendFrom + " start: " + utcFirstStart);
				}
				else //retry later
				{
					TaskEx.Delay(TimeSpan.FromSeconds((retries + 1) * GetScheduleBaseRetryIntervalInSec()))
						.ContinueWith(ant =>
										{
											ResumePlayback(schedule, utcSendFrom, utcFirstStart, retries + 1);
										});
				}
			}
		}

		protected internal virtual int GetScheduleRetries()
		{
			return ConfigManager.ScheduleRetries;
		}

		protected internal virtual int GetScheduleBaseRetryIntervalInSec()
		{
			return ConfigManager.ScheduleBaseRetryIntervalInSec;
		}

		public virtual void Dispose()
		{
			lock (thisLock)
			{
				isDisposed = true;
			}
			sender.DisposeObject();
			collector.DisposeObject();
			converter.DisposeObject();
		}
	}
}

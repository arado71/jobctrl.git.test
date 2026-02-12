using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Scheduling;
using log4net;

namespace PlaybackClient
{
	public class PlaybackSchedulerFactory
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		internal static Func<IPlaybackDataSender> SenderFactory = () => new PlaybackDataSender();
		internal static Func<IPlaybackDataCollector> CollectorFactory = () => new PlaybackDataCollector();
		internal static Func<IEnumerable<KeyValuePair<int, int>>, Func<int, int>, IPlaybackDataConverter> ConverterFactory =
			(uMap, wMapFunc) => new PlaybackDataConverter(uMap, wMapFunc);

		public static int UserCount { get; set; }

		public static PlaybackDataScheduler OneTimeImport(int userId, DateTime startDate, DateTime endDate, int? newUserId = null, Func<int, int> workIdMappingFunc = null)
		{
			log.Info("Importing data for userId: " + userId + " start: " + startDate + " end: " + endDate + " newUserId: " + newUserId);
			if (endDate <= startDate) throw new ArgumentOutOfRangeException();
			var uMap = newUserId == null ? null : new[] { new KeyValuePair<int, int>(userId, newUserId.Value) };
			var scheduler = new PlaybackDataScheduler(SenderFactory, CollectorFactory, () => ConverterFactory(uMap, workIdMappingFunc));
			var start = GetImportScheduleStartDate(startDate, endDate);
			scheduler.Add(new PlaybackSchedule()
				{
					UserId = userId,
					StartDate = startDate,
					EndDate = endDate,
					TimeZoneId = TimeZoneInfo.Utc.Id,
					LocalSchedule = Schedule.CreateOneTime(start),
				},
				start //so we can send the whole schedule
				);
			return scheduler;
		}

		//we either import with the same datetimes as the original data in that case we might send too old data to the server
		//or we keep the time part only in that case we might end up data with different local time (we don't know the timezone at import)
		private static DateTime GetImportScheduleStartDate(DateTime startDate, DateTime endDate)
		{
			var length = endDate - startDate;
			var now = DateTimeEx.UtcNow();
			var start = now.Date + startDate.TimeOfDay;
			while (start + length > now) start = start.AddDays(-1);
			Debug.Assert(start + length < now);
			return start;
		}

		public static PlaybackDataScheduler StartDbSchedules()
		{
			var now = DateTimeEx.UtcNow(); //adding schedules might take a long time so save utcNow 
			log.Info("Starting db schedules at " + now);
			using (var context = new PlaybackClientDataClassesDataContext())
			{
				var schedules = context.PlaybackSchedules.ToList();
				log.Info("Loaded " + schedules.Count + " PlaybackSchedules");
				if (ConfigManager.UserMultiplierLimit > 0)
				{
					var id = 0;
					var userIds = schedules.Select(s => s.UserId).Distinct().ToList();
					if (ConfigManager.UserIdMaxCount > 0) userIds = userIds.Take(ConfigManager.UserIdMaxCount).ToList();
					var userIdMap = userIds.ToDictionary(u => u, u => id++);
					UserCount = userIdMap.Keys.Count;
					var sender = new PlaybackDataSender();
					var scheduler = new PlaybackDataScheduler(() => sender, CollectorFactory, () => new PlaybackDataConverter(userId => ConfigManager.UserIdStartIndex + userIdMap[userId]));
					foreach (var playbackSchedule in schedules.Where(s => userIdMap.ContainsKey(s.UserId)))
					{
						scheduler.Add(playbackSchedule, now);
					}
					return scheduler;
				}
				else
				{
					var scheduler = new PlaybackDataScheduler(SenderFactory, CollectorFactory, () => ConverterFactory(null, null));
					foreach (var playbackSchedule in schedules)
					{
						scheduler.Add(playbackSchedule, now);
					}
					return scheduler;
				}
			}
		}
	}
}

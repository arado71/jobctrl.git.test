using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tct.ActivityRecorderService.Caching;
using Tct.ActivityRecorderService.EmailStats;
using Tct.ActivityRecorderService.Website;
using Tct.ActivityRecorderService.WebsiteServiceReference;

namespace Tct.ActivityRecorderService.Messaging
{
	using Message = WebsiteServiceReference.Message;
	class MessageService : PeriodicManager, IMessageService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly TimeSpan userMessageCacheItemExpiration = TimeSpan.FromMinutes(10);

		private class UserDeviceKey : IEquatable<UserDeviceKey>
		{
			public int UserId { get; set; }
			public int ComputerId { get; set; }

			public bool Equals(UserDeviceKey other)
			{
				if (ReferenceEquals(null, other)) return false;
				if (ReferenceEquals(this, other)) return true;
				return UserId == other.UserId && ComputerId == other.ComputerId;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((UserDeviceKey)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (UserId * 397) ^ ComputerId;
				}
			}
		}

		private class MessagesState
		{
			public DateTime LastAccess { get; set; }
			public DateTime? LastChange { get; set; }
			public List<Message> Messages { get; set; }
		}

#if DEBUG
		private bool firstRunDebug = true;
		private List<Message> serverOnlyMessagesDebug = new List<Message>();
#endif

		private readonly Dictionary<UserDeviceKey, MessagesState> userMessagesCache = new Dictionary<UserDeviceKey, MessagesState>();
		private readonly UserIdManager userIdManager;
		private const string WORKTIME_END_MESSAGE_TYPE = "WorktimeEndNotification";

		private readonly Dictionary<int, DateTime> messagesSent;

		public MessageService()
		{
			messagesSent = new Dictionary<int, DateTime>();
			ManagerCallbackInterval = (int)refreshInterval.TotalMilliseconds;
			Start(60000);
		}

		private static MessageService instance;

		public static MessageService Instance => instance ?? (instance = new MessageService());

		private readonly TimeSpan refreshInterval = TimeSpan.FromMinutes(1f);

		public List<Message> GetMessages(int userId, DateTime? lastMessageLastChangeDate, int computerId)
		{
			var key = new UserDeviceKey { UserId = userId, ComputerId = computerId };
			var now = DateTime.UtcNow;
			List<Message> result = null;
			lock (userMessagesCache)
			{
				if (userMessagesCache.TryGetValue(key, out var value))
				{
					result = value.Messages;
					value.LastChange = lastMessageLastChangeDate;
					value.LastAccess = now;
					value.Messages = null;
				}
				else
					userMessagesCache.Add(key, new MessagesState { LastAccess = now, LastChange = lastMessageLastChangeDate });
			}
#if !DEBUG
			if (result?.Count > 0)
				using (var clientWrapper = new WebsiteClientWrapper())
					clientWrapper.Client.MarkMessagesAsSent(new Guid(ConfigManager.WebsiteSystemComponentTicket), result.Select(m => new SentMessage { ComputerId = computerId, MessageId = m.Id, SentAt = now }).ToArray());
#endif
			return result ?? new List<Message>();
		}

		protected override void ManagerCallbackImpl()
		{

			using (var clientWrapper = new WebsiteClientWrapper())
			using (var context = new JobControlDataClassesDataContext())
			{
				if (ConfigManager.ServerOnlyMessagesEnabled)
				{
					tryInsertWorktimeNotificationMessage(context, clientWrapper);
				}

				MessageTargetUserInfo[] messageTargetUserInfoArray;
				lock (userMessagesCache)
				{
					var clearThreshold = DateTime.UtcNow - userMessageCacheItemExpiration;
					userMessagesCache.RemoveAll(i => i.Value.LastAccess < clearThreshold);
					messageTargetUserInfoArray = userMessagesCache.GroupBy(u => u.Key.UserId).Select(u => new MessageTargetUserInfo { UserId = u.Key, LastChangeDate = u.Min(l => l.Value.LastChange) }).ToArray();
				}
#if DEBUG
				var messageChangesArray = new List<Message>();
				if (firstRunDebug)
				{
					firstRunDebug = false;
					messageChangesArray.AddRange(messageTargetUserInfoArray.Select(c => new Message{ TargetUserId = c.UserId, Content = "Test message for debug", CreatedAt = DateTime.UtcNow, Id = 1, Target = MessageTarget.PC }));
				}
				messageChangesArray.AddRange(serverOnlyMessagesDebug);
				serverOnlyMessagesDebug.Clear();
#else
				if (messageTargetUserInfoArray.Length == 0) return;
				log.Debug($"Calling WebsiteApi.GetMessageChanges for {messageTargetUserInfoArray.Length} user(s)...");
				var messageChangesArray = clientWrapper.Client.GetMessageChanges(new Guid(ConfigManager.WebsiteSystemComponentTicket), messageTargetUserInfoArray );
#endif
				var messChangesLookup = messageChangesArray.ToLookup(c => c.TargetUserId);
				log.Debug($"WebsiteApi.GetMessageChanges returns with {messageChangesArray.Count()} messages for {messChangesLookup.Count} user(s)");
				lock (userMessagesCache)
				{
					var userIdLookup = userMessagesCache.ToLookup(c => c.Key.UserId, c => c.Value);
					foreach (var change in messChangesLookup)
					{
						if (userIdLookup.Contains(change.Key))
							foreach (var messagesState in userIdLookup[change.Key].ToList())
							{
								var messages = change.Where(m => (messagesState.LastChange == null || (m.LastUpdatedAt ?? m.CreatedAt) > messagesState.LastChange) && m.PCLastSentAt == null).ToList();
								if (messages.Count == 0) continue;
								if (messagesState.Messages != null) messagesState.Messages.AddRange(messages);
								else messagesState.Messages = messages;
							}
					}
				}
			}
		}

		public void InsertMessage(int userId, int targetUserId, string content, string type)
		{
#if DEBUG
			serverOnlyMessagesDebug.Add(new Message()
			{
				Content = content,
				CreatedAt = DateTime.UtcNow,
				Id = -1,
				Target = WebsiteServiceReference.MessageTarget.PC,
				TargetUserId = targetUserId,
				Type = type
			});
			return;
#endif
			using (var clientWrapper = new WebsiteClientWrapper())
			using (var context = new JobControlDataClassesDataContext())
			{
				Message m = new Message()
				{
					Content = content,
					CreatedAt = DateTime.UtcNow,
					Id = -1,
					Target = WebsiteServiceReference.MessageTarget.PC,
					TargetUserId = targetUserId,
					Type = type
				};
				clientWrapper.Client.AddMessage(new Guid(context.GetAuthTicket(userId)), m);
			}
		}

		private void tryInsertWorktimeNotificationMessage(JobControlDataClassesDataContext context, WebsiteClientWrapper clientWrapper)
		{
			List<int> userIds;
			lock (userMessagesCache)
				userIds = userMessagesCache.Keys.Select(k => k.UserId).Distinct().ToList();
			//It is not so pretty to call an SP everytime we need the worktimeEnd
			using (var activityRecorderDataClassesContext = new ActivityRecorderDataClassesDataContext())
			{
				var targetUserInfos = new List<MessageTargetUserInfo>();
				var userStats = new Dictionary<int, Tuple<UserStatInfo, DateTime>>();
				foreach (var userId in userIds)
				{
					var clientSettings = activityRecorderDataClassesContext.ClientSettings.SingleOrDefault(c => c.UserId == userId);
					if (clientSettings != null && clientSettings.WorkTimeEndInMins != null)
					{
						int workTimeEndMins = clientSettings.WorkTimeEndInMins.Value;


						UserStatInfo userStatInfo = StatsDbHelper.GetUserStatsInfo(new List<int>(new int[] { userId })).First();
						TimeZoneInfo timeZoneInfo = userStatInfo.TimeZone;
						DateTime localeTime = DateTime.UtcNow.FromUtcToLocal(timeZoneInfo);
						DateTime localeDate = localeTime.Date;
						DateTime previousNotificationDateTime;
						if (messagesSent.TryGetValue(userId, out previousNotificationDateTime))
						{
							if (previousNotificationDateTime.FromUtcToLocal(timeZoneInfo).Date == localeDate)
							{
								continue;
							}
						}
						DateTime localeWorktimeEndTime = localeTime.Date.AddMinutes(workTimeEndMins);
						DateTime notificationTime = localeWorktimeEndTime.AddMinutes(-ConfigManager.MinutesBeforeWorktimeEndNotification);
						if (localeTime > notificationTime && localeTime < localeWorktimeEndTime)
						{
							targetUserInfos.Add(new MessageTargetUserInfo { UserId = userId, LastChangeDate = notificationTime });
							userStats.Add(userId, new Tuple<UserStatInfo, DateTime>(userStatInfo, localeWorktimeEndTime));
						}
					}
				}
#if DEBUG
				var messagesByUserId = userStats.Keys.Select(u => new Message { TargetUserId = u }).ToLookup(c => c.TargetUserId);
#else
				var messagesByUserId = clientWrapper.Client.GetMessageChanges(new Guid(ConfigManager.WebsiteSystemComponentTicket), targetUserInfos.ToArray()).Concat(userStats.Keys.Select(u => new Message { TargetUserId = u })).ToLookup(c => c.TargetUserId);
#endif

				foreach (var item in messagesByUserId)
				{
					// userId = item.Key
					var isAlreadyCreated = false;
					foreach (var message in item.ToList())
					{
						if (message.Type == WORKTIME_END_MESSAGE_TYPE)
						{
							messagesSent[item.Key] = message.CreatedAt;
							isAlreadyCreated = true;
							break;
						}
					}
					if (isAlreadyCreated) continue;

					log.Debug($"Creating WorktimeEndNotification on server (userId: {item.Key})...");
					var userStat = userStats[item.Key];
					var culture = CultureInfo.GetCultureInfo(string.IsNullOrEmpty(userStat.Item1.CultureId) ? EmailStatsHelper.DefaultCulture : userStat.Item1.CultureId);
					Thread.CurrentThread.CurrentCulture = culture;
					Thread.CurrentThread.CurrentUICulture = culture;
					Message m = new Message()
					{
						Content = string.Format(EmailStats.EmailStats.WorkTimeEndsSoon, userStat.Item2.ToString("HH:mm")),
						CreatedAt = DateTime.UtcNow,
						Id = -1,
						Target = WebsiteServiceReference.MessageTarget.PC,
						TargetUserId = item.Key,
						Type = WORKTIME_END_MESSAGE_TYPE
					};
#if DEBUG
					serverOnlyMessagesDebug.Add(m);
#else
					clientWrapper.Client.AddMessage(new Guid(context.GetAuthTicket(item.Key)), m);
#endif
					if (messagesSent.ContainsKey(item.Key))
						messagesSent[item.Key] = m.CreatedAt;
					else
						messagesSent.Add(item.Key, m.CreatedAt);
				}
			}
		}

		public DateTime MarkMassageAsRead(int userId, int messageId, int computerId)
		{
			log.Debug($"Marking message as read (userId: {userId} computerId: {computerId})...");
			using (var clientWrapper = new WebsiteClientWrapper())
			using (var context = new JobControlDataClassesDataContext())
			{
				DateTime currentTime = DateTime.UtcNow;
				var result = clientWrapper.Client.MarkMessageAsRead(new Guid(context.GetAuthTicket(userId)), messageId, currentTime, computerId);
				return currentTime;
			}
		}
	}
}

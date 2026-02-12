using ProxyDataRouter.ProxyServiceReference;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using log4net;
using Tct.ActivityRecorderClient;

namespace ProxyDataRouter
{
	[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
	public class CallRouter : IProxyServiceCallback, IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const string ServiceAcc = "H17ms&k#z&MS0RKYn@7Dg8JfW@bUTRkXZMw64KqG65e#GTuwrbr@p5wGjN$83S@B";
		private const string ServiceHash = "0sxwNa2rAvD$5#p%JMPa%5^ul*q6ltsleXZEv7mgi7v6ctLAd*l^IOPWAVvHGLY5";
		private static readonly CachedDictionary<Guid, int> fileGuidUserIdMap = new CachedDictionary<Guid, int>(TimeSpan.FromHours(1), true);
		private static readonly CachedDictionary<string, int> emailUidMap = new CachedDictionary<string, int>(TimeSpan.FromMinutes(1), true);

		private ConcurrentDictionary<int, string> passwordCache;

		internal ProxyClientWrapper ProxyClient { get; }

		public CallRouter(ConcurrentDictionary<int, string> passwordCache)
		{
			this.passwordCache = passwordCache;
			ProxyClient  = new ProxyClientWrapper(new InstanceContext(this));
			ProxyClient.Client.ClientCredentials.UserName.UserName = ServiceAcc;
			ProxyClient.Client.ClientCredentials.UserName.Password = ServiceHash;
			ProxyClient.Client.Open();
		}

		private void Go(int userId, Action<ActivityRecorderServiceClientWrapper> action, [CallerMemberName] string callerName = null)
		{
			using (var client = new ActivityRecorderServiceClientWrapper())
			{
				var sw = Stopwatch.StartNew();
				try
				{
					if (!passwordCache.TryGetValue(userId, out var hash)) throw new FaultException("Authentication data not found");
					client.Client.ClientCredentials.UserName.UserName = userId.ToString();
					client.Client.ClientCredentials.UserName.Password = hash;
					action(client);
					log.Debug($"{callerName} uid: {userId} finished in {sw.ElapsedMilliseconds}ms");
				}
				catch (Exception ex)
				{
					log.Debug($"{callerName} uid: {userId} failed", ex);
					throw;
				}
			}
		}

		private T Go<T>(int userId, Func<ActivityRecorderServiceClientWrapper, T> action, [CallerMemberName] string callerName = null)
		{
			using (var client = new ActivityRecorderServiceClientWrapper())
			{
				log.Debug($"{callerName} uid: {userId} started");
				var sw = Stopwatch.StartNew();
				try
				{
					if (!passwordCache.TryGetValue(userId, out var hash)) throw new FaultException("Authentication data not found");
					client.Client.ClientCredentials.UserName.UserName = userId.ToString();
					client.Client.ClientCredentials.UserName.Password = hash;
					T result = action(client);
					log.Debug($"{callerName} finished in {sw.ElapsedMilliseconds}ms");
					return result;
				}
				catch (Exception ex)
				{
					log.Debug(callerName + " failed", ex);
					throw;
				}
			}
		}

		public NotificationData GetPendingNotification(int userId, int computerId, int? lastId)
		{
			return DataMapper.To(Go(userId, client => client.Client.GetPendingNotification(userId, computerId, lastId)));
		}

		public void ConfirmNotification(NotificationResult result)
		{
			Go(result.UserId, client => client.Client.ConfirmNotification(DataMapper.To(result)));
		}

		public void AddWorkItemEx(WorkItem workItem)
		{
			Go(workItem.UserId, client => client.Client.AddWorkItemEx(DataMapper.To(workItem)));
		}

		public ClientMenu GetClientMenu(out string newVersion, int userId, string oldVersion)
		{
			string newVer = null;
			var result = Go(userId, client => DataMapper.To(client.Client.GetClientMenu(userId, oldVersion, out newVer)));
			newVersion = newVer;
			return result;
		}

		public string SetClientMenu(int userId, ClientMenu newMenu)
		{
			return Go(userId, client => client.Client.SetClientMenu(userId, DataMapper.To(newMenu)));
		}

		public ClientSetting GetClientSettings(out string newVersion, int userId, string oldVersion)
		{
			string newVer = null;
			var result = Go(userId, client => DataMapper.To(client.Client.GetClientSettings(userId, oldVersion, out newVer)));
			newVersion = newVer;
			return result;
		}

		public AuthData Authenticate(string clientInfo)
		{
			if (!int.TryParse(clientInfo, out var userId))
				if (!emailUidMap.TryGetValue(clientInfo, out userId))
					throw new FaultException("email authentication data not found");
			AuthData authData = null;
			Go(userId,client =>
			{
				authData = DataMapper.To(client.Client.Authenticate(""));

			});
			return authData;
		}

		public string GetAuthTicket(int userId)
		{
			return Go(userId, client => client.Client.GetAuthTicket(userId));
		}

		public WorkDetectorRule[] GetClientRules(out string newVersion, int userId, string oldVersion)
		{
			string newVer = null;
			var result = Go(userId, client => client.Client.GetClientRules(userId, oldVersion, out newVer)?.Select(DataMapper.To).ToArray());
			newVersion = newVer;
			return result;
		}

		public string SetClientRules(int userId, WorkDetectorRule[] newRules)
		{
			return Go(userId, client => client.Client.SetClientRules(userId, newRules?.Select(DataMapper.To).ToArray()));
		}

		public CensorRule[] GetClientCensorRules(out string newVersion, int userId, string oldVersion)
		{
			string newVer = null;
			var result = Go(userId, client => client.Client.GetClientCensorRules(userId, oldVersion, out newVer)?.Select(DataMapper.To).ToArray());
			newVersion = newVer;
			return result;
		}

		public string SetClientCensorRules(int userId, CensorRule[] newRules)
		{
			return Go(userId, client => client.Client.SetClientCensorRules(userId, newRules?.Select(DataMapper.To).ToArray()));
		}

		public void AddManualWorkItem(ManualWorkItem manualWorkItem)
		{
			Go(manualWorkItem.UserId, client => client.Client.AddManualWorkItem(DataMapper.To(manualWorkItem)));
		}

		public void StartWork(int userId, int workId, int computerId, DateTime createDate, DateTime sendDate)
		{
			Go(userId, client => client.Client.StartWork(userId, workId, computerId, createDate, sendDate));
		}

		public void StopWork(int userId, int computerId, DateTime createDate, DateTime sendDate)
		{
			Go(userId, client => client.Client.StopWork(userId, computerId, createDate, sendDate));
		}

		public ClientWorkTimeStats GetClientWorkTimeStats(int userId)
		{
			return DataMapper.To(Go(userId, client => client.Client.GetClientWorkTimeStats(userId)));
		}

		public void ReportClientVersion(int userId, int computerId, int major, int minor, int build, int revision, string application)
		{
			Go(userId, client => client.Client.ReportClientVersion(userId, computerId, major, minor, build, revision, application));
		}

		public void ReportClientComputerInfo(ClientComputerInfo info)
		{
			Go(info.UserId, client => client.Client.ReportClientComputerInfo(DataMapper.To(info)));
		}

		public void ReportClientError(ClientComputerError clientError)
		{
			Go(clientError.UserId, client => client.Client.ReportClientError(DataMapper.To(clientError)));
		}

		public ClientComputerKick GetPendingKick(int userId, int computerId)
		{
			return DataMapper.To(Go(userId, client => client.Client.GetPendingKick(userId, computerId)));
		}

		public void ConfirmKick(int userId, int computerId, int kickId, KickResult result)
		{
			Go(userId, client => client.Client.ConfirmKick(userId, computerId, kickId, DataMapper.To(result)));
		}

		public void MakeClientActive(int userId, int deviceId, bool isActive)
		{
			Go(userId, client => client.Client.MakeClientActive(userId, deviceId, isActive));
		}

		public bool AssignWorkByKey(int userId, AssignWorkData assignWorkData)
		{
			return Go(userId, client => client.Client.AssignWorkByKey(userId, DataMapper.To(assignWorkData)));
		}

		public DateTime GetServerTime(int userId, int computerId, DateTime clientTime)
		{
			return Go(userId, client => client.Client.GetServerTime(userId, computerId, clientTime));
		}

		public RuleGeneratorData[] GetLearningRuleGenerators(out string newVersion, int userId, string oldVersion)
		{
			string newVer = null;
			var result = Go(userId, client => client.Client.GetLearningRuleGenerators(userId, oldVersion, out newVer)?.Select(DataMapper.To).ToArray());
			newVersion = newVer;
			return result;
		}

		public CannedCloseReasons GetCannedCloseReasons(int userId)
		{
			return DataMapper.To(Go(userId, client => client.Client.GetCannedCloseReasons(userId)));
		}

		public CloseWorkResult CloseWork(int userId, int workId, string reason, int? reasonItemId)
		{
			return DataMapper.To(Go(userId, client => client.Client.CloseWork(userId, workId, reason, reasonItemId)));
		}

		public int AddReason(int userId, int workId, string reason, int? reasonItemId)
		{
			return Go(userId, client => client.Client.AddReason(userId, workId, reason, reasonItemId));
		}

		public int AddReasonEx(ReasonItem reasonItem)
		{
			return Go(reasonItem.UserId, client => client.Client.AddReasonEx(DataMapper.To(reasonItem)));
		}

		public ReasonStats GetReasonStats(int userId)
		{
			return DataMapper.To(Go(userId, client => client.Client.GetReasonStats(userId)));
		}

		public SimpleWorkTimeStats GetSimpleWorkTimeStats(int userId, DateTime? desiredEndDate)
		{
			return DataMapper.To(Go(userId, client => client.Client.GetSimpleWorkTimeStats(userId, desiredEndDate)));
		}

		public MeetingData GetMeetingData(int userId)
		{
			return DataMapper.To(Go(userId, client => client.Client.GetMeetingData(userId)));
		}

		public AllWorkItem[] GetAllWorks(int userId)
		{
			return Go(userId, client => client.Client.GetAllWorks(userId)).Select(DataMapper.To).ToArray();
		}

		public AssignTaskResult AssignTask(int userId, int taskId)
		{
			return DataMapper.To(Go(userId, client => client.Client.AssignTask(userId, taskId)));
		}

		public MeetingData ManageMeetings(int userId, int computerId, FinishedMeetingData finishedMeetingData)
		{
			return DataMapper.To(Go(userId, client => client.Client.ManageMeetings(userId, computerId, DataMapper.To(finishedMeetingData))));
		}

		public MeetingData ManageMeetingsSync(int userId, int computerId, FinishedMeetingData finishedMeetingData)
		{
			return DataMapper.To(Go(userId, client => client.Client.ManageMeetings(userId, computerId, DataMapper.To(finishedMeetingData))));
		}

		public void AddManualMeeting(int userId, ManualMeetingData manualMeeting)
		{
			Go(userId, client => client.Client.AddManualMeeting(userId, DataMapper.To(manualMeeting)));
		}

		public TaskReasons GetTaskReasons(int userId)
		{
			return DataMapper.To(Go(userId, client => client.Client.GetTaskReasons(userId)));
		}

		public bool AssignProjectByKey(int userId, AssignProjectData assignProjectData)
		{
			return Go(userId, client => client.Client.AssignProjectByKey(userId, DataMapper.To(assignProjectData)));
		}

		public DateTime? GetExpiryDay(int userId)
		{
			return Go(userId, client => client.Client.GetExpiryDay(userId));
		}

		public ApplicationUpdateInfo GetApplicationUpdate(int userId, string application, string currentVersion)
		{
			return DataMapper.To(Go(userId, client =>
			{
				var result = client.Client.GetApplicationUpdate(userId, application, currentVersion);
				if (result == null) return null;
				lock (fileGuidUserIdMap) fileGuidUserIdMap.Set(result.FileId, userId);
				return result;
			}));
		}

		public byte[] GetUpdateChunk(Guid fileId, long chunkIndex)
		{
			int userId;
			lock(fileGuidUserIdMap) if (!fileGuidUserIdMap.TryGetValue(fileId, out userId)) throw new FaultException("fileId not found");
			return Go(userId, client => client.Client.GetUpdateChunk(fileId, chunkIndex));
		}

		public bool AssignProjectAndWorkByKey(int userId, AssignCompositeData assignCompositeData)
		{
			return Go(userId, client => client.Client.AssignProjectAndWorkByKey(userId, DataMapper.To(assignCompositeData)));
		}

		public void AddParallelWorkItem(ParallelWorkItem parallelWorkItem)
		{
			Go(parallelWorkItem.UserId, client => client.Client.AddParallelWorkItem(DataMapper.To(parallelWorkItem)));
		}

		public ProjectManagementConstraints GetProjectManagementConstraints(int userId, int projectId)
		{
			return DataMapper.To(Go(userId, client => client.Client.GetProjectManagementConstraints(userId, projectId)));
		}

		public int CreateWork(int userId, int projectId, WorkData workData)
		{
			return Go(userId, client => client.Client.CreateWork(userId, projectId, DataMapper.To(workData)));
		}

		public void UpdateWork(int userId, WorkData workData)
		{
			Go(userId, client => client.Client.UpdateWork(userId, DataMapper.To(workData)));
		}

		public DailyWorkTimeStats[] GetDailyWorkTimeStats(int userId, long oldVersion)
		{
			return Go(userId, client => client.Client.GetDailyWorkTimeStats(userId, oldVersion))?.Select(DataMapper.To).ToArray();
		}

		public ClientWorkTimeHistory GetWorkTimeHistory(int userId, DateTime startDate, DateTime endDate)
		{
			return DataMapper.To(Go(userId, client => client.Client.GetWorkTimeHistory(userId, startDate, endDate)));
		}

		public TimeSpan GetStartOfDayOffset(int userId)
		{
			return Go(userId, client => client.Client.GetStartOfDayOffset(userId));
		}

		public bool ModifyWorkTime(int userId, WorkTimeModifications modifications)
		{
			return Go(userId, client => client.Client.ModifyWorkTime(userId, DataMapper.To(modifications)));
		}

		public WorkNames GetWorkNames(int userId, int[] workIds)
		{
			return DataMapper.To(Go(userId, client => client.Client.GetWorkNames(userId, workIds)));
		}

		public CollectorRules GetClientCollectorRules(out string newVersion, int userId, string oldVersion)
		{
			string newVer = null;
			var result = DataMapper.To(Go(userId, client => client.Client.GetClientCollectorRules(userId, oldVersion, out newVer)));
			newVersion = newVer;
			return result;
		}

		public string SetClientCollectorRules(int userId, CollectorRules newRules)
		{
			return Go(userId, client => client.Client.SetClientCollectorRules(userId, DataMapper.To(newRules)));
		}

		public void AddCollectedItem(CollectedItem collectedItem)
		{
			Go(collectedItem.UserId, client => client.Client.AddCollectedItem(DataMapper.To(collectedItem)));
		}

		public void AddAggregateCollectedItems(AggregateCollectedItems collectedItems)
		{
			Go(collectedItems.UserId, client => client.Client.AddAggregateCollectedItems(DataMapper.To(collectedItems)));
		}

		public void AddIssue(IssueData issue)
		{
			Go(issue.UserId, client => client.Client.AddIssue(DataMapper.To(issue)));
		}

		public void AddTelemetry(TelemetryItem telemetryItem)
		{
			Go(telemetryItem.UserId, client => client.Client.AddTelemetry(DataMapper.To(telemetryItem)));
		}

		public void ModifyIssue(IssueData issue)
		{
			Go(issue.UserId, client => client.Client.ModifyIssue(DataMapper.To(issue)));
		}

		public IssueData GetIssue(int userId, string issueCode)
		{
			return DataMapper.To(Go(userId, client => client.Client.GetIssue(userId, issueCode)));
		}

		public IssueData[] FilterIssues(int userId, string[] keywords, int? filterState, bool? filterOwner)
		{
			return Go(userId, client => client.Client.FilterIssues(userId, keywords, filterState, filterOwner))?.Select(DataMapper.To).ToArray();
		}

		public bool AddSnippet(Snippet data)
		{
			return Go(data.UserId, client => client.Client.AddSnippet(DataMapper.To(data)));
		}

		public GetMessagesResponse GetMessages(GetMessagesSolicit request)
		{
			return new GetMessagesResponse(new GetMessagesResponseBody(DataMapper.To(Go(request.Body.userId, client => client.Client.GetMessages(request.Body.userId, request.Body.lastMessageLastChangeDate, request.Body.computerId)))));
		}

		public DateTime MarkMessageAsRead(int userId, int messageId, int computerId)
		{
			return Go(userId, client => client.Client.MarkMessageAsRead(userId, messageId, computerId));
		}

		public GetTodoListResponse GetTodoList(GetTodoListSolicit request)
		{
			return new GetTodoListResponse(new GetTodoListResponseBody(DataMapper.To(Go(request.Body.userId, client => client.Client.GetTodoList(request.Body.userId, request.Body.date)))));
		}

		public GetTodoListItemStatusesResponse GetTodoListItemStatuses(GetTodoListItemStatusesSolicit request)
		{
			return new GetTodoListItemStatusesResponse(new GetTodoListItemStatusesResponseBody(Go(passwordCache.Keys.First(), client => client.Client.GetTodoListItemStatuses())?.Select(DataMapper.To).ToArray()));
		}

		public CreateOrUpdateTodoListResponse CreateOrUpdateTodoList(CreateOrUpdateTodoListSolicit request)
		{
			return new CreateOrUpdateTodoListResponse(new CreateOrUpdateTodoListResponseBody(Go(request.Body.todoList.UserId, client => client.Client.CreateOrUpdateTodoList(DataMapper.To(request.Body.todoList)))));
		}

		public GetMostRecentTodoListResponse GetMostRecentTodoList(GetMostRecentTodoListSolicit request)
		{
			return new GetMostRecentTodoListResponse(new GetMostRecentTodoListResponseBody(DataMapper.To(Go(request.Body.userId, client => client.Client.GetMostRecentTodoList(request.Body.userId)))));
		}

		public AcquireTodoListLockResponse AcquireTodoListLock(AcquireTodoListLockSolicit request)
		{
			return new AcquireTodoListLockResponse(new AcquireTodoListLockResponseBody(DataMapper.To(Go(request.Body.userId, client => client.Client.AcquireTodoListLock(request.Body.userId, request.Body.todoListId)))));
		}

		public bool ReleaseTodoListLock(int userId, int todoListId)
		{
			return Go(userId, client => client.Client.ReleaseTodoListLock(userId, todoListId));
		}

		public AcceptanceData GetDppInformation(int userId)
		{
			return DataMapper.To(Go(userId, client => client.Client.GetDppInformation(userId)));
		}

		public bool SetDppAcceptanceDate(int userId, DateTime acceptedAt)
		{
			return Go(userId, client => client.Client.SetDppAcceptanceDate(userId, acceptedAt));
		}

		public CloudTokenData ManageCloudTokens(int userId, string googleCalendarToken)
		{
			return DataMapper.To(Go(userId, client => client.Client.ManageCloudTokens(userId, googleCalendarToken)));
		}

		public bool ShouldSendLogs(int userId)
		{
			return Go(userId, client => client.Client.ShouldSendLogs(userId));
		}

		public GetWorkTimeStatsForUserResponse GetWorkTimeStatsForUser(GetWorkTimeStatsForUserSolicit request)
		{
			return new GetWorkTimeStatsForUserResponse(new GetWorkTimeStatsForUserResponseBody(DataMapper.To(Go(request.Body.userId, client => client.Client.GetWorkTimeStatsForUser(request.Body.userId)))));
		}

		public bool SwitchAutomaticRules(int userId)
		{
			return Go(userId, client => client.Client.SwitchAutomaticRules(userId));
		}

		public bool CheckCredential(string userId, string hash)
		{
			using (var client = new ActivityRecorderServiceClientWrapper())
			{
				try
				{
					client.Client.ClientCredentials.UserName.UserName = userId;
					client.Client.ClientCredentials.UserName.Password = hash;
					var authData = client.Client.Authenticate("");
					passwordCache.AddOrUpdate(authData.Id, hash, (i, s) => hash);
					if (userId != authData.Id.ToString())
						emailUidMap.Set(userId, authData.Id);
					log.Debug($"User: {authData.Id} authenticated successfully");
					return true;
				}
				catch (Exception ex)
				{
					log.Debug($"User: {userId} authentication failed with " + ex.Message);
					return false;
				}
			}
		}

		public void Dispose()
		{
			ProxyClient.Dispose();;
		}
	}
}

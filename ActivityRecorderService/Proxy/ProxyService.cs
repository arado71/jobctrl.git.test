using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderService.ClientComputerData;
using Tct.ActivityRecorderService.DailyAggregation;
using Tct.ActivityRecorderService.Kicks;
using Tct.ActivityRecorderService.MeetingSync;
using Tct.ActivityRecorderService.Notifications;
using Tct.ActivityRecorderService.OnlineStats;
using Tct.ActivityRecorderService.Stats;
using Tct.ActivityRecorderService.Telemetry;
using Tct.ActivityRecorderService.TODOs;
using Tct.ActivityRecorderService.WebsiteServiceReference;
using Tct.ActivityRecorderService.WorkManagement;
using Tct.ActivityRecorderService.WorkTimeHistory;
using ReasonItem = Tct.ActivityRecorderService.WorkManagement.ReasonItem;
using WorkTimeStats = Tct.ActivityRecorderService.WebsiteServiceReference.WorkTimeStats;

namespace Tct.ActivityRecorderService.Proxy
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
	[CallbackBehavior(IncludeExceptionDetailInFaults = true)]
	public class ProxyService : IActivityRecorder, IProxyService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static IProxyCallback callback;
		private static readonly ManualResetEvent isCallbackAvailEvent = new ManualResetEvent(false);

		public static IProxyCallback GetCallback()
		{
			var cb = callback;
			while (cb == null)
			{
				isCallbackAvailEvent.WaitOne(1000);
				cb = callback;
			}

			return cb;
		}

		private void Go(Action action, [CallerMemberName] string callerName = null)
		{
				var sw = Stopwatch.StartNew();
				try
				{
					action();
					log.Debug($"{callerName} finished in {sw.ElapsedMilliseconds}ms");
				}
				catch (Exception ex)
				{
					log.Debug(callerName + " failed", ex);
					throw;
				}
		}

		private T Go<T>(Func<T> func, [CallerMemberName] string callerName = null)
		{
			T result;
			var sw = Stopwatch.StartNew();
			try
			{
				result = func();
				log.Debug($"{callerName} finished in {sw.ElapsedMilliseconds}ms");
			}
			catch (Exception ex)
			{
				log.Debug(callerName + " failed", ex);
				throw;
			}
			return result;
		}

		private async Task<T> Go<T>(Func<Task<T>> func, [CallerMemberName] string callerName = null)
		{
			T result;
			var sw = Stopwatch.StartNew();
			try
			{
				result = await func();
				log.Debug($"{callerName} finished in {sw.ElapsedMilliseconds}ms");
			}
			catch (Exception ex)
			{
				log.Debug(callerName + " failed", ex);
				throw;
			}
			return result;
		}

		public void InitiateChannel()
		{
			log.Debug("Callback channel initiated");
			try
			{
				callback = OperationContext.Current.GetCallbackChannel<IProxyCallback>();
				isCallbackAvailEvent.Set();
				Thread.Sleep(30000);
				log.Debug("Callback channel closed");
			}
			catch (Exception ex)
			{
				log.Error("failed", ex);
			}
			finally
			{
				isCallbackAvailEvent.Reset();
				callback = null;
			}
		}

		public NotificationData GetPendingNotification(int userId, int computerId, int? lastId)
		{
			return Go(() => GetCallback().GetPendingNotification(userId, computerId, lastId));
		}

		public void ConfirmNotification(NotificationResult result)
		{
			Go(() => GetCallback().ConfirmNotification(result));
		}

		public void AddWorkItemEx(WorkItem workItem)
		{
			Go(() => GetCallback().AddWorkItemEx(workItem));
		}

		public ClientMenu GetClientMenu(int userId, string oldVersion, out string newVersion)
		{
			string newVer = null;
			var result = Go(() => GetCallback().GetClientMenu(userId, oldVersion, out newVer));
			newVersion = newVer;
			return result;
		}

		public string SetClientMenu(int userId, ClientMenu newMenu)
		{
			return Go(() => GetCallback().SetClientMenu(userId, newMenu));
		}

		public ClientSetting GetClientSettings(int userId, string oldVersion, out string newVersion)
		{
			string newVer = null;
			var result = Go(() => GetCallback().GetClientSettings(userId, oldVersion, out newVer));
			newVersion = newVer;
			return result;
		}

		public TotalWorkTimeStats GetTotalWorkTimeStats(int userId)
		{
			throw new NotImplementedException();
		}

		public AuthData Authenticate(string clientInfo)
		{
			var userId = OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;
			return Go(() => GetCallback().Authenticate(userId)); // hack to access user while callback call
		}

		public string GetAuthTicket(int userId)
		{
			return Go(() => GetCallback().GetAuthTicket(userId));
		}

		public List<WorkDetectorRule> GetClientRules(int userId, string oldVersion, out string newVersion)
		{
			string newVer = null;
			var result = Go(() => GetCallback().GetClientRules(userId, oldVersion, out newVer));
			newVersion = newVer;
			return result;
		}

		public string SetClientRules(int userId, List<WorkDetectorRule> newRules)
		{
			return Go(() => GetCallback().SetClientRules(userId, newRules));
		}

		public List<CensorRule> GetClientCensorRules(int userId, string oldVersion, out string newVersion)
		{
			string newVer = null;
			var result = Go(() => GetCallback().GetClientCensorRules(userId, oldVersion, out newVer));
			newVersion = newVer;
			return result;
		}

		public string SetClientCensorRules(int userId, List<CensorRule> newRules)
		{
			return Go(() => GetCallback().SetClientCensorRules(userId, newRules));
		}

		public void AddManualWorkItem(ManualWorkItem manualWorkItem)
		{
			Go(() => GetCallback().AddManualWorkItem(manualWorkItem));
		}

		public void StartWork(int userId, int workId, int computerId, DateTime createDate, DateTime sendDate)
		{
			Go(() => GetCallback().StartWork(userId, workId, computerId, createDate, sendDate));
		}

		public void StopWork(int userId, int computerId, DateTime createDate, DateTime sendDate)
		{
			Go(() => GetCallback().StopWork(userId, computerId, createDate, sendDate));
		}

		public ClientWorkTimeStats GetClientWorkTimeStats(int userId)
		{
			return Go(() => GetCallback().GetClientWorkTimeStats(userId));
		}

		public void ReportClientVersion(int userId, int computerId, int major, int minor, int build, int revision, string application)
		{
			Go(() => GetCallback().ReportClientVersion(userId, computerId, major, minor, build, revision, application));
		}

		public void ReportClientComputerInfo(ClientComputerInfo info)
		{
			Go(() => GetCallback().ReportClientComputerInfo(info));
		}

		public void ReportClientError(ClientComputerError clientError)
		{
			Go(() => GetCallback().ReportClientError(clientError));
		}

		public ClientComputerKick GetPendingKick(int userId, int computerId)
		{
			return Go(() => GetCallback().GetPendingKick(userId, computerId));
		}

		public void ConfirmKick(int userId, int computerId, int kickId, KickResult result)
		{
			Go(() => GetCallback().ConfirmKick(userId, computerId, kickId, result));
		}

		public void MakeClientActive(int userId, int deviceId, bool isActive)
		{
			Go(() => GetCallback().MakeClientActive(userId, deviceId, isActive));
		}

		public Task<bool> AssignWorkByKeyAsync(int userId, AssignWorkData assignWorkData)
		{
			return Go(() => GetCallback().AssignWorkByKeyAsync(userId, assignWorkData));
		}

		public DateTime GetServerTime(int userId, int computerId, DateTime clientTime)
		{
			return Go(() => GetCallback().GetServerTime(userId, computerId, clientTime));
		}

		public List<RuleGeneratorData> GetLearningRuleGenerators(int userId, string oldVersion, out string newVersion)
		{
			string newVer = null;
			var result = Go(() => GetCallback().GetLearningRuleGenerators(userId, oldVersion, out newVer));
			newVersion = newVer;
			return result;
		}

		public CannedCloseReasons GetCannedCloseReasons(int userId)
		{
			return Go(() => GetCallback().GetCannedCloseReasons(userId));
		}

		public CloseWorkResult CloseWork(int userId, int workId, string reason, int? reasonItemId)
		{
			return Go(() => GetCallback().CloseWork(userId, workId, reason, reasonItemId));
		}

		public int AddReason(int userId, int workId, string reason, int? reasonItemId)
		{
			return Go(() => GetCallback().AddReason(userId, workId, reason, reasonItemId));
		}

		public int AddReasonEx(ReasonItem reasonItem)
		{
			return Go(() => GetCallback().AddReasonEx(reasonItem));
		}

		public ReasonStats GetReasonStats(int userId)
		{
			return Go(() => GetCallback().GetReasonStats(userId));
		}

		public SimpleWorkTimeStats GetSimpleWorkTimeStats(int userId, DateTime? desiredEndDate)
		{
			return Go(() => GetCallback().GetSimpleWorkTimeStats(userId, desiredEndDate));
		}

		public MeetingData GetMeetingData(int userId)
		{
			return Go(() => GetCallback().GetMeetingData(userId));
		}

		public List<AllWorkItem> GetAllWorks(int userId)
		{
			return Go(() => GetCallback().GetAllWorks(userId));
		}

		public Task<AssignTaskResult> AssignTaskAsync(int userId, int taskId)
		{
			return Go(() => GetCallback().AssignTaskAsync(userId, taskId));
		}

		public Task<MeetingData> ManageMeetingsAsync(int userId, int computerId, FinishedMeetingData finishedMeetingData)
		{
			return Go(() => GetCallback().ManageMeetingsAsync(userId, computerId, finishedMeetingData));
		}

		public Task AddManualMeetingAsync(int userId, ManualMeetingData manualMeeting, int? computerId)
		{
			return Go(() => GetCallback().AddManualMeetingAsync(userId, manualMeeting, computerId));
		}

		public TaskReasons GetTaskReasons(int userId)
		{
			return Go(() => GetCallback().GetTaskReasons(userId));
		}

		public bool AssignProjectByKey(int userId, AssignProjectData assignProjectData)
		{
			return Go(() => GetCallback().AssignProjectByKey(userId, assignProjectData));
		}

		public DateTime? GetExpiryDay(int userId)
		{
			return Go(() => GetCallback().GetExpiryDay(userId));
		}

		public ApplicationUpdateInfo GetApplicationUpdate(int userId, string application, string currentVersion)
		{
			return Go(() => GetCallback().GetApplicationUpdate(userId, application, currentVersion));
		}

		public byte[] GetUpdateChunk(Guid fileId, long chunkIndex)
		{
			return Go(() => GetCallback().GetUpdateChunk(fileId, chunkIndex));
		}

		public Task<bool> AssignProjectAndWorkByKeyAsync(int userId, AssignCompositeData assignCompositeData)
		{
			return Go(() => GetCallback().AssignProjectAndWorkByKeyAsync(userId, assignCompositeData));
		}

		public void AddParallelWorkItem(ParallelWorkItem parallelWorkItem)
		{
			Go(() => GetCallback().AddParallelWorkItem(parallelWorkItem));
		}

		public Task<ProjectManagementConstraints> GetProjectManagementConstraintsAsync(int userId, int projectId)
		{
			return Go(() => GetCallback().GetProjectManagementConstraintsAsync(userId, projectId));
		}

		public Task<int> CreateWorkAsync(int userId, int projectId, WorkData workData)
		{
			return Go(() => GetCallback().CreateWorkAsync(userId, projectId, workData));
		}

		public Task UpdateWorkAsync(int userId, WorkData workData)
		{
			return Go(() => GetCallback().UpdateWorkAsync(userId, workData));
		}

		public List<DailyWorkTimeStats> GetDailyWorkTimeStats(int userId, long oldVersion)
		{
			return Go(() => GetCallback().GetDailyWorkTimeStats(userId, oldVersion));
		}

		public ClientWorkTimeHistory GetWorkTimeHistory(int userId, DateTime startDate, DateTime endDate)
		{
			return Go(() => GetCallback().GetWorkTimeHistory(userId, startDate, endDate));
		}

		public TimeSpan GetStartOfDayOffset(int userId)
		{
			return Go(() => GetCallback().GetStartOfDayOffset(userId));
		}

		public bool ModifyWorkTime(int userId, WorkTimeModifications modifications)
		{
			return Go(() => GetCallback().ModifyWorkTime(userId, modifications));
		}

		public WorkNames GetWorkNames(int userId, List<int> workIds)
		{
			return Go(() => GetCallback().GetWorkNames(userId, workIds));
		}

		public CollectorRules GetClientCollectorRules(int userId, string oldVersion, out string newVersion)
		{
			string newVer = null;
			var result = Go(() => GetCallback().GetClientCollectorRules(userId, oldVersion, out newVer));
			newVersion = newVer;
			return result;
		}

		public string SetClientCollectorRules(int userId, CollectorRules newRules)
		{
			return Go(() => GetCallback().SetClientCollectorRules(userId, newRules));
		}

		public void AddCollectedItem(CollectedItem collectedItem)
		{
			Go(() => GetCallback().AddCollectedItem(collectedItem));
		}

		public void AddAggregateCollectedItems(AggregateCollectedItems collectedItems)
		{
			Go(() => GetCallback().AddAggregateCollectedItems(collectedItems));
		}

		public void AddIssue(IssueData issue)
		{
			Go(() => GetCallback().AddIssue(issue));
		}

		public void AddTelemetry(TelemetryItem telemetryItem)
		{
			Go(() => GetCallback().AddTelemetry(telemetryItem));
		}

		public void ModifyIssue(IssueData issue)
		{
			Go(() => GetCallback().ModifyIssue(issue));
		}

		public IssueData GetIssue(int userId, string issueCode)
		{
			return Go(() => GetCallback().GetIssue(userId, issueCode));
		}

		public List<IssueData> FilterIssues(int userId, List<string> keywords, int? filterState, bool? filterOwner)
		{
			return Go(() => GetCallback().FilterIssues(userId, keywords, filterState, filterOwner));
		}

		public bool AddSnippet(Snippet data)
		{
			return Go(() => GetCallback().AddSnippet(data));
		}

		public List<Message> GetMessages(int userId, DateTime? lastMessageLastChangeDate, int computerId)
		{
			return Go(() => GetCallback().GetMessages(userId, lastMessageLastChangeDate, computerId));
		}

		public DateTime MarkMessageAsRead(int userId, int messageId, int computerId)
		{
			return Go(() => GetCallback().MarkMessageAsRead(userId, messageId, computerId));
		}

		public TodoListDTO GetTodoList(int userId, DateTime date)
		{
			return Go(() => GetCallback().GetTodoList(userId, date));
		}

		public List<TodoListItemStatusDTO> GetTodoListItemStatuses()
		{
			return Go(() => GetCallback().GetTodoListItemStatuses());
		}

		public bool CreateOrUpdateTodoList(TodoListDTO todoList)
		{
			return Go(() => GetCallback().CreateOrUpdateTodoList(todoList));
		}

		public TodoListDTO GetMostRecentTodoList(int userId)
		{
			return Go(() => GetCallback().GetMostRecentTodoList(userId));
		}

		public TodoListToken AcquireTodoListLock(int userId, int todoListId)
		{
			return Go(() => GetCallback().AcquireTodoListLock(userId, todoListId));
		}

		public bool ReleaseTodoListLock(int userId, int todoListId)
		{
			return Go(() => GetCallback().ReleaseTodoListLock(userId, todoListId));
		}

		public DisplayedReports GetDisplayedReports(int userId, string culture)
		{
			return Go(() => GetCallback().GetDisplayedReports(userId, culture));
		}

		public FavoriteReport[] GetFavoriteReports(int userId, string culture)
		{
			return Go(() => GetCallback().GetFavoriteReports(userId, culture));
		}

		public string GetDppMessage(int userId)
		{
			throw new NotImplementedException();
		}

		public AcceptanceData GetDppInformation(int userId)
		{
			return Go(() => GetCallback().GetDppInformation(userId));
		}

		public bool SetDppAcceptanceDate(int userId, DateTime acceptedAt)
		{
			return Go(() => GetCallback().SetDppAcceptanceDate(userId, acceptedAt));
		}

		public CloudTokenData ManageCloudTokens(int userId, string googleCalendarToken)
		{
			return Go(() => GetCallback().ManageCloudTokens(userId, googleCalendarToken));
		}

		public bool ShouldSendLogs(int userId)
		{
			return Go(() => GetCallback().ShouldSendLogs(userId));
		}

		public WorkTimeStats GetWorkTimeStatsForUser(int userId, int computerId, WorktimeStatIntervals intervals)
		{
			return Go(() => GetCallback().GetWorkTimeStatsForUser(userId, computerId, intervals));
		}

		public bool SwitchAutomaticRules(int userId)
		{
			return Go(() => GetCallback().SwitchAutomaticRules(userId));
		}

		public ClientTab[] GetCustomTabs(int userId, string culture)
		{
			return Go(() => GetCallback().GetCustomTabs(userId, culture));
		}

		public DisplayedReports GetDisplayedReportForTabId(int userId, string culture, string tabId, DateTime? localToday = null)
		{
			return Go(() => GetCallback().GetDisplayedReportForTabId(userId, culture, tabId, localToday));
		}

		public List<DateTime> GetUserWorkdays(int userId)
		{
			return Go(() => GetCallback().GetUserWorkdays(userId));
		}

		public void Disconnect(int userId, int computerId, string osName)
		{
			Go(() => GetCallback().Disconnect(userId, computerId, osName));
		}
	}
}

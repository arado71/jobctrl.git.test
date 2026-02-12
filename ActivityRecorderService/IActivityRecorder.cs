using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
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

namespace Tct.ActivityRecorderService
{
	[ServiceContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public interface IActivityRecorder : INotificationService
	{
#if !DEBUG //removed in 2012 July
		//[OperationContract]
		//void AddWorkItem(WorkItem workItem);
#endif
		[OperationContract]
		void AddWorkItemEx(WorkItem workItem);

		[OperationContract]
		ClientMenu GetClientMenu(int userId, string oldVersion, out string newVersion);

		[OperationContract]
		string SetClientMenu(int userId, ClientMenu newMenu);

		[OperationContract]
		ClientSetting GetClientSettings(int userId, string oldVersion, out string newVersion);

#if !DEBUG //legacy method (from 2011 Sept 2) removed in 2012 July
		//[OperationContract]
		//WorkTimeStats GetTodaysWorkTimeStats(int userId);
#endif

#if !DEBUG //legacy method (from 2012 Oct 15)
		[OperationContract]
		TotalWorkTimeStats GetTotalWorkTimeStats(int userId);
#endif

		[OperationContract]
		AuthData Authenticate(string clientInfo);

		[OperationContract]
		string GetAuthTicket(int userId);

		[OperationContract]
		List<WorkDetectorRule> GetClientRules(int userId, string oldVersion, out string newVersion);

		[OperationContract]
		string SetClientRules(int userId, List<WorkDetectorRule> newRules);

		[OperationContract]
		List<CensorRule> GetClientCensorRules(int userId, string oldVersion, out string newVersion);

		[OperationContract]
		string SetClientCensorRules(int userId, List<CensorRule> newRules);

		[OperationContract]
		void AddManualWorkItem(ManualWorkItem manualWorkItem);

		[OperationContract]
		void StartWork(int userId, int workId, int computerId, DateTime createDate, DateTime sendDate);

		[OperationContract]
		void StopWork(int userId, int computerId, DateTime createDate, DateTime sendDate);

		[OperationContract]
		ClientWorkTimeStats GetClientWorkTimeStats(int userId);

		[OperationContract]
		void ReportClientVersion(int userId, int computerId, int major, int minor, int build, int revision, string application);

		[OperationContract]
		void ReportClientComputerInfo(ClientComputerInfo info);

		[OperationContract]
		void ReportClientError(ClientComputerError clientError);

		[OperationContract]
		ClientComputerKick GetPendingKick(int userId, int computerId);

		[OperationContract]
		void ConfirmKick(int userId, int computerId, int kickId, KickResult result);

		[OperationContract]
		void MakeClientActive(int userId, int deviceId, bool isActive);

		[OperationContract(Name = "AssignWorkByKey")]
		Task<bool> AssignWorkByKeyAsync(int userId, AssignWorkData assignWorkData);

		[OperationContract]
		DateTime GetServerTime(int userId, int computerId, DateTime clientTime);

		[OperationContract]
		List<RuleGeneratorData> GetLearningRuleGenerators(int userId, string oldVersion, out string newVersion);

		[OperationContract]
		CannedCloseReasons GetCannedCloseReasons(int userId);

		[OperationContract]
		CloseWorkResult CloseWork(int userId, int workId, string reason, int? reasonItemId);

		[OperationContract]
		int AddReason(int userId, int workId, string reason, int? reasonItemId);

		[OperationContract]
		int AddReasonEx(ReasonItem reasonItem);

		[OperationContract]
		ReasonStats GetReasonStats(int userId);

		[OperationContract]
		SimpleWorkTimeStats GetSimpleWorkTimeStats(int userId, DateTime? desiredEndDate);

		[OperationContract]
		MeetingData GetMeetingData(int userId);

		[OperationContract]
		List<AllWorkItem> GetAllWorks(int userId);

		[OperationContract(Name = "AssignTask")]
		Task<AssignTaskResult> AssignTaskAsync(int userId, int taskId);

		[OperationContract(Name = "ManageMeetings")]
		Task<MeetingData> ManageMeetingsAsync(int userId, int computerId, FinishedMeetingData finishedMeetingData);

#if !DEBUG //legacy method (from 2013 Jan 25) removed in 2012 February 26 [messes up WCF performance counters in win2012 server]
		//[OperationContract]
		//AddManualMeetingsResult AddManualMeetings(int userId, IEnumerable<ManualMeetingData> manualMeetings);
#endif

		[OperationContract(Name = "AddManualMeeting")]
		Task AddManualMeetingAsync(int userId, ManualMeetingData manualMeeting, int? computerId);

		[OperationContract]
		TaskReasons GetTaskReasons(int userId);

		[OperationContract]
		bool AssignProjectByKey(int userId, AssignProjectData assignProjectData);

		[OperationContract]
		DateTime? GetExpiryDay(int userId);

		[OperationContract]
		ApplicationUpdateInfo GetApplicationUpdate(int userId, string application, string currentVersion);

		[OperationContract]
		byte[] GetUpdateChunk(Guid fileId, long chunkIndex);

		[OperationContract(Name = "AssignProjectAndWorkByKey")]
		Task<bool> AssignProjectAndWorkByKeyAsync(int userId, AssignCompositeData assignCompositeData);

		[OperationContract]
		void AddParallelWorkItem(ParallelWorkItem parallelWorkItem);

		[OperationContract(Name = "GetProjectManagementConstraints")]
		Task<ProjectManagementConstraints> GetProjectManagementConstraintsAsync(int userId, int projectId);

		[OperationContract(Name = "CreateWork")]
		Task<int> CreateWorkAsync(int userId, int projectId, WorkData workData);

		[OperationContract(Name = "UpdateWork")]
		Task UpdateWorkAsync(int userId, WorkData workData);

		[OperationContract]
		List<DailyWorkTimeStats> GetDailyWorkTimeStats(int userId, long oldVersion);

		[OperationContract]
		ClientWorkTimeHistory GetWorkTimeHistory(int userId, DateTime startDate, DateTime endDate);

		[OperationContract]
		TimeSpan GetStartOfDayOffset(int userId);

		[OperationContract]
		bool ModifyWorkTime(int userId, WorkTimeModifications modifications);

		[OperationContract]
		WorkNames GetWorkNames(int userId, List<int> workIds);

		[OperationContract]
		CollectorRules GetClientCollectorRules(int userId, string oldVersion, out string newVersion);

		[OperationContract]
		string SetClientCollectorRules(int userId, CollectorRules newRules);

//#if !DEBUG //2015-06-01 never went to live
		[OperationContract]
		void AddCollectedItem(CollectedItem collectedItem);
//#endif

		[OperationContract]
		void AddAggregateCollectedItems(AggregateCollectedItems collectedItems);

		[OperationContract]
		void AddIssue(IssueData issue);

		[OperationContract]
		void AddTelemetry(TelemetryItem telemetryItem);

		[OperationContract]
		void ModifyIssue(IssueData issue);

		[OperationContract]
		IssueData GetIssue(int userId, string issueCode);

		[OperationContract]
		List<IssueData> FilterIssues(int userId, List<string> keywords, int? filterState, bool? filterOwner);

		[OperationContract]
		bool AddSnippet(Snippet data);

		[XmlSerializerFormatAttribute]
		[OperationContract]
		List<Message> GetMessages(int userId, DateTime? lastMessageLastChangeDate, int computerId);

		[XmlSerializerFormatAttribute]
		[OperationContract]
		DateTime MarkMessageAsRead(int userId, int messageId, int computerId);

		[XmlSerializerFormat]
		[OperationContract]
		TodoListDTO GetTodoList(int userId, DateTime date);

		[XmlSerializerFormat]
		[OperationContract]
		List<TodoListItemStatusDTO> GetTodoListItemStatuses();

		[XmlSerializerFormat]
		[OperationContract]
		bool CreateOrUpdateTodoList(TodoListDTO todoList);

		[XmlSerializerFormat]
		[OperationContract]
		TodoListDTO GetMostRecentTodoList(int userId);

		[XmlSerializerFormat]
		[OperationContract]
		TodoListToken AcquireTodoListLock(int userId, int todoListId);

		[XmlSerializerFormat]
		[OperationContract]
		bool ReleaseTodoListLock(int userId, int todoListId);

		[XmlSerializerFormat]
		[OperationContract]
		DisplayedReports GetDisplayedReports(int userId, string culture);

		[XmlSerializerFormat]
		[OperationContract]
		FavoriteReport[] GetFavoriteReports(int userId, string culture);
		
#if !DEBUG // legacy method
		[OperationContract]
		string GetDppMessage(int userId);
#endif

		[OperationContract]
		AcceptanceData GetDppInformation(int userId);

		[OperationContract]
		bool SetDppAcceptanceDate(int userId, DateTime acceptedAt);

		[OperationContract]
		CloudTokenData ManageCloudTokens(int userId, string googleCalendarToken);

		[OperationContract]
		bool ShouldSendLogs(int userId);

		[XmlSerializerFormat]
		[OperationContract]
		WebsiteServiceReference.WorkTimeStats GetWorkTimeStatsForUser(int userId, int computerId, WorktimeStatIntervals intervals);

		[OperationContract]
		bool SwitchAutomaticRules(int userId);

		[XmlSerializerFormat]
		[OperationContract]
		ClientTab[] GetCustomTabs(int userId, string culture);

		[XmlSerializerFormat]
		[OperationContract]
		DisplayedReports GetDisplayedReportForTabId(int userId, string culture, string tabId, DateTime? localToday = null);

		[OperationContract]
		List<DateTime> GetUserWorkdays(int userId);

		[OperationContract]
		void Disconnect(int userId, int computerId, string osName);
	}
}